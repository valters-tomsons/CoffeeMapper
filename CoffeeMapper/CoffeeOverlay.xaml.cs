using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoffeeMapper
{
    /// <summary>
    /// Interaction logic for CoffeeOverlay.xaml
    /// </summary>
    public partial class CoffeeOverlay : Window
    {

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        public CoffeeOverlay()
        {
            InitializeComponent();

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            //ShowCursor(false);
        }

        public void PushNotification(string _msg)
        {
            NotificationText.Text = _msg;

            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 450;
            da.Duration = TimeSpan.FromSeconds(0.2);
            NotificationBar.BeginAnimation(Grid.WidthProperty, da);

            

            //DoubleAnimation da2 = new DoubleAnimation();
            //da.From = 450;
            //da.To = 0;
            //da.Duration = TimeSpan.FromSeconds(1);
            //NotificationBar.BeginAnimation(Grid.WidthProperty, da2);

            
        }
    }
}
