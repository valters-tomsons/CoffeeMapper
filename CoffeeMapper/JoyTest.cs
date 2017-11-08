using System;
using vJoyInterfaceWrap;


namespace CoffeeMapper
{
    static class JoyTest
    {
        //Taken from vJoy SDK Documentation

        public static bool isDriverInstalled(vJoy joystick)
        {
            if (joystick.vJoyEnabled())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int GetButtons(vJoy joystick, uint id)
        {
            var foo = JoyTest.ReturnAxes(joystick, id);
            
            return 0;
        }

        public static bool isVersionCompatible(vJoy joystick)
        {
            UInt32 DllVer = 0, DrvVer = 0;
            bool match = joystick.DriverMatch(ref DllVer, ref DrvVer);
            if(match)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string DriverStatus(vJoy joystick, uint id)
        {
            VjdStat status = joystick.GetVJDStatus(id);
            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    return "owned";
                case VjdStat.VJD_STAT_FREE:
                    return "free";
                case VjdStat.VJD_STAT_BUSY:
                    return "owned";
                case VjdStat.VJD_STAT_MISS:
                    return "error";
                default:
                    return "error";
            }
        }

        public static string ReturnAxes(vJoy joystick, uint id)
        {
            //Buttons
            int nBtn = joystick.GetVJDButtonNumber(id);
            int nDPov = joystick.GetVJDDiscPovNumber(id);
            int nCPov = joystick.GetVJDContPovNumber(id);

            //Axes
            bool X_Exist = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X);
            bool Y_Exist = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y);
            bool Z_Exist = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Z);
            bool RX_Exist = joystick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_RX);
            
            string res = String.Format("Device[{0}]: Buttons={1}; DiscPOVs:{2}; ContPOVs:{3}", id, nBtn, nDPov, nCPov);
            return res;
        }

        public static string AcquireDevice(vJoy joystick, uint id)
        {
            VjdStat status;
            status = joystick.GetVJDStatus(id);
            string res;
            if ((status == VjdStat.VJD_STAT_OWN) || ((status == VjdStat.VJD_STAT_FREE) && (!joystick.AcquireVJD(id))))
            {
                res = String.Format("Failed to acquire vJoy device number {0}.", id);
                return res;
            }
            else
            {
                res = String.Format("Acquired: vJoy device number {0}.", id);
                return res;
            }
        }

    }
}
