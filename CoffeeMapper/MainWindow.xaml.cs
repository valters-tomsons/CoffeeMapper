using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using vJoyInterfaceWrap;
using System.Linq;

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

        public MainWindow()
        {
            InitializeComponent();

            vJoySelfTest();
            Debug.WriteLine(JoyTest.ReturnAxes(joystick, id));
            Debug.WriteLine(JoyTest.AcquireDevice(joystick, id));

            Buttons.CollectionChanged += Buttons_Changed;
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

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeKeyboard();
        }

        private void InitializeKeyboard()
        {
            KeyboardHook = new GlobalKeyboardHook();
            KeyboardHook.KeyboardPressed += On_KeyPressed;
        }
        //[STAThread]
        private void On_KeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            if(e.KeyboardState == GlobalKeyboardHook.KeyboardState.KeyDown)
            {
                //Debug.WriteLine(e.KeyboardData.VirtualCode);
                if(!Buttons.Contains(e.KeyboardData.VirtualCode))
                {
                    Buttons.Add(e.KeyboardData.VirtualCode);
                }
                
            }
            else
            {
                Buttons.Remove(e.KeyboardData.VirtualCode);
            }

        }

        private void Buttons_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                foreach(int key in e.NewItems)
                {
                    if(key == VirtualKeyCodes.W)
                    {
                        PressBtn(11);
                    }
                    if(key == VirtualKeyCodes.S)
                    {
                        PressBtn(12);
                    }
                    if(key == VirtualKeyCodes.D)
                    {
                        PressBtn(13);
                    }
                    if(key == VirtualKeyCodes.A)
                    {
                        PressBtn(14);
                    }
                }
            }

            if(e.OldItems != null)
            {
                foreach (int key in e.OldItems)
                {
                    if (key == VirtualKeyCodes.W)
                    {
                        DeBtn(11);
                    }
                    if (key == VirtualKeyCodes.S)
                    {
                        DeBtn(12);
                    }
                    if (key == VirtualKeyCodes.D)
                    {
                        DeBtn(13);
                    }
                    if (key == VirtualKeyCodes.A)
                    {
                        DeBtn(14);
                    }
                }
            }
        }


        private void PressBtn(uint btnId)
        {
            joystick.SetBtn(true, id, btnId);
        }

        private void DeBtn(uint btnId)
        {
            joystick.SetBtn(false, id, btnId);
        }

    }
}
