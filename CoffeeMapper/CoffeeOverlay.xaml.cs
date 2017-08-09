using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CoffeeMapper
{
    /// <summary>
    /// Interaction logic for CoffeeOverlay.xaml
    /// </summary>
    public partial class CoffeeOverlay : Window
    {

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        private DispatcherTimer NotificationTimer;

        public CoffeeOverlay()
        {
            InitializeComponent();

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;

            NotificationBar.Width = 0;

            NotificationTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            NotificationTimer.Tick += NotificationTimer_Tick;
            //ShowCursor(false);
        }

        private void NotificationTimer_Tick(object sender, EventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = 450;
            da.To = 0;
            da.Duration = TimeSpan.FromSeconds(0.2);
            Dispatcher.Invoke(new Action(() => NotificationBar.BeginAnimation(Grid.WidthProperty, da)));
            NotificationTimer.Stop();
        }

        public void PushNotification(string _msg)
        {
            NotificationText.Text = String.Empty;

            if (_msg == "activated")
            {
                NotificationImage.Source = new BitmapImage(new Uri("/data/CoffeeMapper_Activated.png",UriKind.Relative));
            }
            else if (_msg == "deactivated")
            {
                NotificationImage.Source = new BitmapImage(new Uri("/data/CoffeeMapper_Deactivated.png", UriKind.Relative));
            }
            else
            {
                NotificationImage.Source = new BitmapImage(new Uri("/data/CoffeeMapper_Info.png", UriKind.Relative));
                NotificationText.Text = _msg;
            }

            DoubleAnimation da = new DoubleAnimation();
            da.From = 0;
            da.To = 450;
            da.Duration = TimeSpan.FromSeconds(0.2);
            NotificationBar.BeginAnimation(Grid.WidthProperty, da);

            NotificationTimer.Start();
        }
    }
}
