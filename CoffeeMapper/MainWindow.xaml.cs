using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using vJoyInterfaceWrap;
using System.Linq;
using System.Xml;
using System.Windows.Threading;

namespace CoffeeMapper
{

    public partial class MainWindow : Window
    {
        static public vJoy joystick = new vJoy();
        static public vJoy.JoystickState iReport;
        static public uint id = 1;
        static public bool allowFeeding = false;

        private GlobalKeyboardHook KeyboardHook;
        DispatcherTimer mouseTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(16) };

        ObservableCollection<int> Buttons = new ObservableCollection<int>();

        System.Windows.Forms.NotifyIcon trayicon;

        CoffeeOverlay overlay;

        private static int[] KeyCodes;
        private static string[] KeyNames;

        int[] oldPos = { 0, 0 };

        int CenterAxis = 16384;
        bool HandleKeys = false;
        bool TrapCursor = true;
        public int MouseSens = 600;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTrayIcon();

            //vJoy Driver Test
            vJoySelfTest();
            Debug.WriteLine(JoyTest.ReturnAxes(joystick, id));
            Debug.WriteLine(JoyTest.AcquireDevice(joystick, id));

            Buttons.CollectionChanged += Buttons_Changed;

            //Retrieve XML information
            CreateKeyArrays();
        }

        private void InitializeTrayIcon()
        {
            trayicon = new System.Windows.Forms.NotifyIcon();
            trayicon.Icon = new System.Drawing.Icon("CoffeeMapper_EXE.ico");

            trayicon.BalloonTipTitle = "CoffeeMapper will keep running in the background!";
            trayicon.BalloonTipText = "No Worries!";
            trayicon.ShowBalloonTip(1000);
            trayicon.Click += BringBackWindow;
        }

        private void BringBackWindow(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() => WindowState = WindowState.Normal));
            Dispatcher.Invoke(new Action(() => Show()));
            Dispatcher.Invoke(new Action(() => trayicon.Visible = false));
            Activate();
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

            overlay = new CoffeeOverlay();
            overlay.Show();
            ResetAllAxis();
        }

        private void InitializeKeyboardHook()
        {
            KeyboardHook = new GlobalKeyboardHook();
            KeyboardHook.KeyboardPressed += On_KeyPressed;
        }

        private void InitializeMouseHook()
        {
            mouseTimer.Tick += mouseTimer_Tick;
            mouseTimer.Start();
        }

        private void mouseTimer_Tick(object sender, EventArgs e)
        {

            if(MouseCursor.Y > oldPos[1])
            {
                int sum = (MouseCursor.Y - oldPos[1]) * MouseSens;
                //Debug.WriteLine(sum);
                joystick.SetAxis((16384 + sum), id, HID_USAGES.HID_USAGE_RY);
            }
            else if (MouseCursor.Y < oldPos[1])
            {
                int sum = (oldPos[1] - MouseCursor.Y) * MouseSens;
                //Debug.WriteLine(sum);
                joystick.SetAxis((16384 - sum), id, HID_USAGES.HID_USAGE_RY);
            }

            if (MouseCursor.X > oldPos[0])
            {
                int sum = (MouseCursor.X - oldPos[0]) * MouseSens;
                //Debug.WriteLine(sum);
                joystick.SetAxis((16384 + sum), id, HID_USAGES.HID_USAGE_RX);
            }
            else if (MouseCursor.X < oldPos[0])
            {
                int sum = (oldPos[0] - MouseCursor.X) * MouseSens;
                //Debug.WriteLine(sum);
                joystick.SetAxis((16384 - sum), id, HID_USAGES.HID_USAGE_RX);
            }
            
            if(TrapCursor)
            {
                MouseCursor.Center();
            }

            oldPos[0] = MouseCursor.X;
            oldPos[1] = MouseCursor.Y;


        }

        //[STAThread]
        //Gets called when actual key is pressed on keyboard
        private void On_KeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if(e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                if (!Buttons.Contains(e.KeyboardData.VirtualCode))
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

                        //Configuration mode
                        if (keyname == "F10")
                        {
                            TrapCursor = !TrapCursor;
                            Debug.WriteLine(MouseCursor.Show);
                            Dispatcher.Invoke(new Action(() => overlay.PushNotification($"TrapCursor = {TrapCursor}")));
                        }

                        //Turn on CoffeeMapper
                        if (keyname == "F12")
                        {
                            Dispatcher.Invoke(new Action(() => overlay.PushNotification("activated")));
                            mouseTimer.Start();
                        }

                        //Turn off CoffeeMapper
                        if (keyname == "F11")
                        {
                            Dispatcher.Invoke(new Action(()=> overlay.PushNotification("deactivated")));
                            mouseTimer.Stop();
                            
                        }

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
        private void ResetAllAxis()
        {
            foreach(HID_USAGES axis in Enum.GetValues(typeof(HID_USAGES)))
            {
                joystick.SetAxis(CenterAxis, id, axis);
            }
        }

        private void ResetAxis(string axis)
        {
            if(axis == "LEFT")
            {
                joystick.SetAxis(CenterAxis, id, HID_USAGES.HID_USAGE_X);
                joystick.SetAxis(CenterAxis, id, HID_USAGES.HID_USAGE_Y);
                return;
            }

            if(axis == "RIGHT")
            {
                joystick.SetAxis(CenterAxis, id, HID_USAGES.HID_USAGE_RX);
                joystick.SetAxis(CenterAxis, id, HID_USAGES.HID_USAGE_RY);
                return;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MouseCursor.Show = true;
            if (overlay != null)
            {
                overlay.Close();
            }
            
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                trayicon.Visible = true;
                Hide();
            }
        }
    }
}
