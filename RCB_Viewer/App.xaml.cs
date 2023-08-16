using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RCB_Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static List<Window> AllWindows = new List<Window>();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //support forms in project properties
            var screens = System.Windows.Forms.Screen.AllScreens;
            foreach (var screen in screens)
            {
                var newWindow = new MainWindow();
                newWindow.WindowState = WindowState.Normal; // Set initial state to normal
                newWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                newWindow.Left = screen.Bounds.Left;
                newWindow.Top = screen.Bounds.Top;
                newWindow.Width = screen.Bounds.Width;
                newWindow.Height = screen.Bounds.Height;

                newWindow.Show();
                // needs to be done afterwards otherwise shows on main screen
                newWindow.WindowState = WindowState.Maximized; // Maximize the window
                AllWindows.Add(newWindow);
            }
        }
    }
}
