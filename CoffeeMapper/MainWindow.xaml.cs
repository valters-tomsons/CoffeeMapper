using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using vJoyInterfaceWrap;
using System.Linq;
using System.Xml;
using System.Runtime.InteropServices;

namespace CoffeeMapper
{

    public partial class MainWindow : Window
    {
        static public vJoy joystick = new vJoy();
        static public vJoy.JoystickState iReport;
        static public uint id = 1;
        static public bool allowFeeding = false;

        private GlobalKeyboardHook KeyboardHook;

        ObservableCollection<int> Buttons = new ObservableCollection<int>();

        private static int[] KeyCodes;
        private static string[] KeyNames;

        int CenterAxis = 16384;
        bool HandleKeys = false;


        public MainWindow()
        {
            InitializeComponent();

            //vJoy Driver Test
            vJoySelfTest();
            Debug.WriteLine(JoyTest.ReturnAxes(joystick, id));
            Debug.WriteLine(JoyTest.AcquireDevice(joystick, id));

            Buttons.CollectionChanged += Buttons_Changed;

            //Retrieve XML information
            CreateKeyArrays();
        }

        private void vJoySelfTest()
        {
            if(!JoyTest.isDriverInstalled(joystick))
            {
                DriverLabel.Text = "vJoy Driver not installed!";
                Process.Start("http://vjoystick.sourceforge.net/site/index.php/download-a-install/download");
                //Application.Current.Shutdown();
                return;
            }

            if(!JoyTest.isVersionCompatible(joystick))
            {
                DriverLabel.Text = "SDK Version is outdated, unstable!";
            }

            Debug.WriteLine($"Device Status: {JoyTest.DriverStatus(joystick, id)}");
            if(JoyTest.DriverStatus(joystick, id) != "free")
            {
                DriverLabel.Text = "vJoy Device not usable, please restart Computer";
                return;
            }

            StartButton.IsEnabled = true;
        }

        private void CreateKeyArrays()
        {
            string VirtualKeysXML = AppDomain.CurrentDomain.BaseDirectory + @"\data\VirtualKeys.xml";

            List<int> codes = new List<int>();
            List<string> names = new List<string>(); 

            using (XmlReader reader = XmlReader.Create(VirtualKeysXML))
            {
                while(reader.Read())
                {
                    if(reader.IsStartElement())
                    {
                        switch(reader.Name)
                        {
                            case "VirtualKeys":
                                break;
                            case "Key":
                                codes.Add(Convert.ToInt32(reader["value"]));
                                names.Add(reader["equivalent"]);
                                break;
                        }
                    }
                }
            }

            KeyCodes = codes.ToArray();
            KeyNames = names.ToArray();
            Debug.WriteLine($"Added {KeyCodes.Length} virtual keys!");
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeKeyboardHook();
            InitializeMouseHook();
            ResetAxis();
        }

        private void InitializeKeyboardHook()
        {
            KeyboardHook = new GlobalKeyboardHook();
            KeyboardHook.KeyboardPressed += On_KeyPressed;
        }

        private void InitializeMouseHook()
        {
            //GlobalMouseHook.Start();
            //GlobalMouseHook.MouseAction += On_MouseAction;
        }

        //private void On_MouseAction(object sender, EventArgs e)
        //{
        //    Debug.WriteLine(e);
        //}

        //[STAThread]
        //Gets called when actual key is pressed on keyboard
        private void On_KeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if(e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                if(!Buttons.Contains(e.KeyboardData.VirtualCode))
                {
                    Buttons.Add(e.KeyboardData.VirtualCode);

                    if(HandleKeys)
                    {
                        e.Handled = true;
                    }
                }
                
            }
            else
            {
                Buttons.Remove(e.KeyboardData.VirtualCode);

                if (HandleKeys)
                {
                    e.Handled = true;
                }
            }

        }

        
        private void Buttons_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            //= on press
            if(e.NewItems != null)
            {
                foreach(int key in e.NewItems)
                {
                    //listen only to supported keys
                    if (KeyCodes.Contains(key))
                    {
                        int loc = Array.IndexOf(KeyCodes, key);
                        string keyname = KeyNames[loc];
                        string[] _binds = Binds(keyname);
                        string[] _values = Values(keyname);

                        if (_binds.Length >= 0)
                        {
                            var i = 0;
                            foreach (string bind in _binds)
                            {
                                HandleKey(bind, Convert.ToUInt32(_values[i]), true);
                                i++;
                            }
                        }
                    }
                }
            }

            //= on depress
            if(e.OldItems != null)
            {
                foreach (int key in e.OldItems)
                {
                    //listen only to supported keys
                    if (KeyCodes.Contains(key))
                    {
                        int loc = Array.IndexOf(KeyCodes, key);
                        string keyname = KeyNames[loc];
                        string[] _binds = Binds(keyname);
                        string[] _values = Values(keyname);

                        if (_binds.Length >= 0)
                        {
                            var i = 0;
                            foreach (string bind in _binds)
                            {
                                HandleKey(bind, Convert.ToUInt32(_values[i]), false);
                                i++;
                            }
                        }
                    }
                }
            }
        }

        private void HandleKey(string bind, uint value, bool press)
        {
            //Handle all buttons
            if(bind == "button")
            {
                joystick.SetBtn(press, id, value);
                return;
            }

            //Handle all axis
            if(bind.Contains("HID_USAGES."))
            {
                int _value = Convert.ToInt32(value);

                //Convert string axis to HID_USAGES
                HID_USAGES axis = (HID_USAGES)Enum.Parse(typeof(HID_USAGES), bind.Replace("HID_USAGES.", String.Empty));

                if (press == true)
                {
                    joystick.SetAxis(_value, id, axis);
                }
                else
                {
                    joystick.SetAxis(16384, id, axis);
                }
                
                
                
            }
        }

        //returns all bind types (eg. button/axis/etc.)
        private string[] Binds(string key)
        {
            string BindsXML = AppDomain.CurrentDomain.BaseDirectory + @"\data\KeyMappings.xml";
            List<string> _binds = new List<string>();
            using (XmlReader reader = XmlReader.Create(BindsXML))
            {
                while(reader.Read())
                {
                    switch(reader.Name)
                    {
                        case "KeyMappings":
                            break;
                        case "Bind":
                            string value = reader["actualKey"];
                            if (value == key)
                            {
                                _binds.Add(reader["target"]);
                            }
                            break;
                    }
                }
            }
            return _binds.ToArray();
        }

        //returns actual value for bind type (eg. button 13, x axis 31233)
        private string[] Values(string key)
        {
            string BindsXML = AppDomain.CurrentDomain.BaseDirectory + @"\data\KeyMappings.xml";
            List<string> _values = new List<string>();
            using (XmlReader reader = XmlReader.Create(BindsXML))
            {
                while(reader.Read())
                {
                    switch(reader.Name)
                    {
                        case "KeyMappings":
                            break;
                        case "Bind":
                            string value = reader["actualKey"];
                            if (value == key)
                            {
                                _values.Add(reader["value"]);
                            }
                            break;
                    }
                }
            }
            return _values.ToArray();
        }

        //Set all axis to center
        private void ResetAxis()
        {
            foreach(HID_USAGES axis in Enum.GetValues(typeof(HID_USAGES)))
            {
                joystick.SetAxis(CenterAxis, id, axis);
            }
        }
    }
}
