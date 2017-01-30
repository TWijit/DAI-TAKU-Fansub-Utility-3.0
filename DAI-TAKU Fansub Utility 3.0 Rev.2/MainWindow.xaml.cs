using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DAI_TAKU_Fansub_Utility_3._0_Rev._2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Encode EncodePage = new Encode();
        About AboutPage = new About();
        AQS AQSPage = new AQS();
        public MainWindow()
        {
            InitializeComponent();
            Page.Content = AboutPage;
        }

        private void Exit(object sender, MouseButtonEventArgs e)
        {
            if(EncodingStatus.Text == "No Encoding In Progress")
            {
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            else
            {
                MessageBoxResult ExitInprogress = MessageBox.Show("Encoding In Progress. Exit?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (ExitInprogress == MessageBoxResult.Yes)
                {
                    EncodingStatus.Text = "Exit";
                    foreach (Process proc in Process.GetProcessesByName("ffmpeg"))
                    {
                        proc.Kill();
                    }
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                }
            }
        }

        private void Facebook(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://www.facebook.com/DAITAKUFS/");
        }

        private void Drag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void about(object sender, MouseButtonEventArgs e)
        {
            Page.Content = AboutPage;
        }
        private void aqs(object sender, MouseButtonEventArgs e)
        {
            Page.Content = AQSPage;
        }
        private void encode(object sender, MouseButtonEventArgs e)
        {
            Page.Content = EncodePage;
        }
    }
}
