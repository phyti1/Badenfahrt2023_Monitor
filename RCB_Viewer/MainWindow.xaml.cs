using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Handle closing logic, set e.Cancel as needed
            Configurations.Instance.PrevDistance = Configurations.Instance.Distance;
        }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = Configurations.Instance;

            Configurations.Instance.Backend = new Backend();
            Closing += OnWindowClosing;
            this.WindowState = WindowState.Maximized;
        }
    }
}
