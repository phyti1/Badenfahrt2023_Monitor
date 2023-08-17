using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RCB_Viewer
{
    public class TimeSpanToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                double totalSeconds = timeSpan.TotalSeconds;
                double roundedValue = Math.Round(totalSeconds, 2);
                return $"{roundedValue} s";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool _isClosing = false;
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
            if (!_isClosing)
            {
                _isClosing = true;
                Configurations.Instance.PrevDistance = Configurations.Instance.Distance;
                //close all other instances

                foreach (var window in App.AllWindows)
                {
                    if (window != this)
                    {
                        window.Close();
                    }
                    else
                    {
                        Debug.WriteLine("found");
                    }
                }
            }
        }
        private static object _lockob = new object();
        public MainWindow()
        {
            InitializeComponent();

            DataContext = Configurations.Instance;
            Configurations.Instance.ChallengePlayers.Add(ChallengePlayer);

            ChallengePlayer.Position = TimeSpan.Zero;
            ChallengePlayer.Play();

            lock(_lockob )
            {
                if (Configurations.Instance.Backend == null)
                {
                    Configurations.Instance.Backend = new Backend();
                }
            }
            Closing += OnWindowClosing;
            //ChallengePlayer.Source = new Uri("")

            this.WindowState = WindowState.Maximized;
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Restart the media playback when it ends
            ChallengePlayer.Position = TimeSpan.Zero;
            ChallengePlayer.Play();
        }
    }
}
