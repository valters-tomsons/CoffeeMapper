using System;
using System.Runtime.InteropServices;
using static CoffeeMapper.MouseCursor;

namespace CoffeeMapper
{
    class MouseCursor
    {
        //Imports from Winapi 
        //https://msdn.microsoft.com/en-us/library/windows/desktop/ff468815(v=vs.85).aspx
        [DllImport("User32.dll")] private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")] private static extern bool GetCursorPos(out System.Drawing.Point lpPoint);
        [DllImport("user32.dll")] private static extern bool SetSystemCursor(IntPtr hcur, int uid);
        [DllImport("user32.dll")] private static extern IntPtr LoadCursorFromFile(string lpFileName);
        [DllImport("user32.dll")] private static extern IntPtr CopyIcon(IntPtr pcur);


        //Screen center coordinates
        public static int xC = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth / 2);
        public static int yC = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight / 2);

        //IDs of system cursors
        private static readonly uint DEFAULT = 32512;

        //Set cursor to screen center
        public static void Center()
        {
            SetCursorPos(xC, yC);
        }

        //Current cursor X coordinate
        public static int X {
            get
            {
                return System.Windows.Forms.Cursor.Position.X;
            }
        }

        //Current cursor Y coordinate
        public static int Y
        {
            get
            {
                return System.Windows.Forms.Cursor.Position.Y;
            }
        }

        private static bool _show;
        public static bool Show
        {
            get => _show;
            set
            {
                _show = value;
                if (value == true)
                {
                    SetSystemCursor(CopyIcon(LoadCursorFromFile(@"C:\windows\cursors\aero_arrow.cur")), (int)DEFAULT);
                }
                else
                {
                    //yes, this is funky business
                    SetSystemCursor(CopyIcon(LoadCursorFromFile($"{AppDomain.CurrentDomain.BaseDirectory}\\data\\blank.cur")), (int)DEFAULT);
                }
            }
        }
    }
}
