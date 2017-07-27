using System;
using System.Runtime.InteropServices;
using static CoffeeMapper.MouseCursor;

namespace CoffeeMapper
{
    class MouseCursor
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        public static int xC = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenWidth / 2);
        public static int yC = Convert.ToInt32(System.Windows.SystemParameters.PrimaryScreenHeight / 2);

        public static void Center()
        {
            SetCursorPos(xC, yC);
        }

        public static int X {
            get
            {
                return System.Windows.Forms.Cursor.Position.X;
            }
        }

        public static int Y
        {
            get
            {
                return System.Windows.Forms.Cursor.Position.Y;
            }
        }
    }
}
