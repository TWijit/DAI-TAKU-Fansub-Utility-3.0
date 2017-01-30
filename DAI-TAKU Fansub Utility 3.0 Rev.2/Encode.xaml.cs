using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;

namespace DAI_TAKU_Fansub_Utility_3._0_Rev._2
{
    /// <summary>
    /// Interaction logic for Encode.xaml
    /// </summary>
    public partial class Encode : Page
    {
        private readonly BackgroundWorker encodework = new BackgroundWorker();

        TimeSpan duration;
        TimeSpan time;
        double percent;
        char DoubleColon = '"';
        private TimeSpan start;
        public delegate void UpdateTextCallback(string per);

        public Encode()
        {
            InitializeComponent();
            encodework.DoWork += encodework_DoWork;
        }

        private void RawBrowse(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openraw = new OpenFileDialog();
            if (RawInput.Text != "Raw")
            {
                string folder = System.IO.Path.GetDirectoryName(RawInput.Text);
                openraw.InitialDirectory = folder;
            }
            openraw.Filter = "Video |*.mkv;*mp4";
            openraw.FilterIndex = 1;
            openraw.Multiselect = true;
            bool? userClickedOK = openraw.ShowDialog();
            if (userClickedOK == true)
            {
                RawInput.Text = openraw.FileName;
                string inputin = "";
                int indexin = openraw.FileName.LastIndexOf(".");
                if (indexin > 0)
                    inputin = openraw.FileName.Substring(0, indexin);
                string f = "_Sub.mkv";
                Output.Text = inputin + f;
            }
        }

        private void SubBrowse(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog opensub = new OpenFileDialog();
            if (SubInput.Text != "Sub")
            {
                string folder = System.IO.Path.GetDirectoryName(SubInput.Text);
                opensub.InitialDirectory = folder;
            }
            opensub.Filter = "ASS (.ass)|*.ass";
            opensub.FilterIndex = 1;
            opensub.Multiselect = true;
            bool? userClickedOK = opensub.ShowDialog();
            if (userClickedOK == true)
            {
                SubInput.Text = opensub.FileName;
            }
        }

        private void CreditBrowse(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog opencre = new OpenFileDialog();
            if (CreditInput.Text != "Credit")
            {
                string folder = System.IO.Path.GetDirectoryName(CreditInput.Text);
                opencre.InitialDirectory = folder;
            }
            opencre.Filter = "Picture |*.jpg;*.png";
            opencre.FilterIndex = 1;
            opencre.Multiselect = true;
            bool? userClickedOK = opencre.ShowDialog();
            if (userClickedOK == true)
            {
                CreditInput.Text = opencre.FileName;
            }
        }

        private void OPBrowse(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openop = new OpenFileDialog();
            if (OPInput.Text != "OP")
            {
                string folder = System.IO.Path.GetDirectoryName(OPInput.Text);
                openop.InitialDirectory = folder;
            }
            openop.Filter = "ASS (.ass)|*.ass";
            openop.FilterIndex = 1;
            openop.Multiselect = true;
            bool? userClickedOK = openop.ShowDialog();
            if (userClickedOK == true)
            {
                OPInput.Text = openop.FileName;
            }
        }

        private void EDBrowse(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog opened = new OpenFileDialog();
            if (EDInput.Text != "ED")
            {
                string folder = System.IO.Path.GetDirectoryName(EDInput.Text);
                opened.InitialDirectory = folder;
            }
            opened.Filter = "ASS (.ass)|*.ass";
            opened.FilterIndex = 1;
            opened.Multiselect = true;
            bool? userClickedOK = opened.ShowDialog();
            if (userClickedOK == true)
            {
                EDInput.Text = opened.FileName;
            }
        }

        private void OutputBrowse(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            if (Output.Text != "Output")
            {
                string folder = System.IO.Path.GetDirectoryName(Output.Text);
                saveFileDialog1.InitialDirectory = folder;
                saveFileDialog1.FileName = Output.Text;
            }
            saveFileDialog1.Filter = "MKV |*.mkv |MP4 |*mp4";
            saveFileDialog1.FilterIndex = 1;
            bool? userClickedOK = saveFileDialog1.ShowDialog();
            if (userClickedOK == true)
            {
                if (saveFileDialog1.FilterIndex == 1)
                    Output.Text = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf(".")) + ".mkv";
                else
                {
                    Output.Text = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf(".")) + ".mp4";
                    FLAC.IsChecked = false;
                }
            }
        }

        private void EncodingStart(object sender, MouseButtonEventArgs e)
        {
            if (SubInput.Text != "Sub" && File.Exists(SubInput.Text) == true)
            {
                if (StyleCheck() == true)
                {
                    AVSCreate();
                    BatCreate();
                    start = new TimeSpan(DateTime.Now.Ticks);
                    encodework.RunWorkerAsync(Output.Text);
                    while (encodework.IsBusy)
                    {
                        if (((MainWindow)System.Windows.Application.Current.MainWindow).EncodingStatus.Text != "Exit")
                        {
                            DoEvents();
                            percent = Math.Round(percent, 2);
                            ((MainWindow)System.Windows.Application.Current.MainWindow).EncodingStatus.Text = string.Format("{0} Percent Complete", percent);
                            Thread.Sleep(15);
                        }
                    }
                    ((MainWindow)System.Windows.Application.Current.MainWindow).EncodingStatus.Text = string.Format("No Encoding In Progress");
                    MessageBox.Show("Completed.");
                }
            }
        }

        public static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private int IndexOfNth(string str, char c, int n)
        {
            int remaining = n;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == c)
                {
                    remaining--;
                    if (remaining == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private Boolean StyleCheck()
        {
            int x, line = 0, linedialog = 0, linestyle = 0;
            string location = SubInput.Text;
            string unknown = "";
            StreamReader streamReadera = new StreamReader(location);
            var lineCount = 0;
            using (var reader = File.OpenText(SubInput.Text))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }
            string[] c = new string[lineCount];
            for (x = 0; x < lineCount; x++)
            {
                c[x] = streamReadera.ReadLine();
            }
            streamReadera.Close();
            for (int g = 0; g < lineCount; g++)
            {
                if (c[g].Contains("Style:") && !c[g].Contains("WrapStyle:"))
                {
                    line++;
                }
            }
            string[] checkstyle = new string[line];
            for (int h = 0; h < lineCount; h++)
            {
                if (c[h].Contains("Style:") && !c[h].Contains("WrapStyle:"))
                {
                    int indexOfDot = IndexOfNth(c[h], ',', 1);
                    checkstyle[linestyle] = c[h].Substring(7, indexOfDot - 7);
                    linestyle++;
                }
            }
            line = 0;
            for (int j = 0; j < lineCount; j++)
            {
                if (c[j].Contains("Dialogue:"))
                {
                    line++;
                }
            }
            string[] checkdialog = new string[line];
            for (int m = 0; m < lineCount; m++)
            {
                if (c[m].Contains("Dialogue:"))
                {
                    int indexOfDotstart = IndexOfNth(c[m], ',', 3);
                    int indexOfDotstop = IndexOfNth(c[m], ',', 4);
                    checkdialog[linedialog] = c[m].Substring(indexOfDotstart + 1, indexOfDotstop - (indexOfDotstart + 1));
                    linedialog++;
                }
            }
            int check = 1;
            for (int z = 0; z < linedialog; z++)
            {
                for (int b = 0; b < linestyle; b++)
                {

                    if (checkdialog[z] == checkstyle[b])
                    {
                        check = 1;
                        break;
                    }
                    else
                    {
                        check = 0;
                    }
                }
                if (check == 0)
                {
                    unknown = checkdialog[z];
                    MessageBox.Show("   Unknown style found. : " + unknown);
                    break;
                }
            }
            if (check == 1)
                return true;
            else
                return false;
        }

        private void AVSCreate()
        {
            string contents = "";
            string currentDirectory = Environment.CurrentDirectory;
            double num1 = 190;
            double num2 = 191;
            string str1 = "LoadPlugin(" + DoubleColon + currentDirectory + "\\Resources\\vsfilter.dll" + DoubleColon + ")";
            string str2 = "DirectShowSource(" + DoubleColon + RawInput.Text + DoubleColon + ", fps=23.976 ,convertfps=true ) ";
            string str2c = "RAW=DirectShowSource(" + DoubleColon + RawInput.Text + DoubleColon + ", fps=23.976 ,convertfps=true ) ";
            string str3 = "Pic = (ImageSource(" + DoubleColon + CreditInput.Text + DoubleColon + ", start=0, end=" + num1 + ", fps=23.976).ConvertToYV12()).FadeOut(25, color=$000000)" + Environment.NewLine + "Blank + RAW" + Environment.NewLine + "trim(0," + num1 + ").overlay(Pic) + trim(" + num2 + ",0)";
            string str4 = "TextSub(" + DoubleColon + SubInput.Text + DoubleColon + ", 1)";
            string str4c = "trim(0," + num1 + ")+trim(" + num2 + ",0).TextSub(" + DoubleColon + SubInput.Text + DoubleColon + ", 1)";
            string str5 = "TextSub(" + DoubleColon + OPInput.Text + DoubleColon + ", 1)";
            string str5c = "trim(0," + num1 + ")+trim(" + num2 + ",0).TextSub(" + DoubleColon + OPInput.Text + DoubleColon + ", 1)";
            string str6 = "TextSub(" + DoubleColon + EDInput.Text + DoubleColon + ", 1)";
            string str6c = "trim(0," + num1 + ")+trim(" + num2 + ",0).TextSub(" + DoubleColon + EDInput.Text + DoubleColon + ", 1)";
            if (OPInput.Text == "OP")
            {
                if (EDInput.Text == "ED")
                {
                    if (CreditInput.Text == "Credit")
                    {
                        contents = str1 + Environment.NewLine + str2 + Environment.NewLine + str4;
                    }
                    else
                    {
                        contents = str1 + Environment.NewLine + str2c + Environment.NewLine + "Blank=BlankClip(RAW," + num2 + ")" + Environment.NewLine + str3 + Environment.NewLine + str4c;
                    }
                }
                else
                {
                    if (CreditInput.Text == "Credit")
                    {
                        contents = str1 + Environment.NewLine + str2 + Environment.NewLine + str4 + Environment.NewLine + str6;
                    }
                    else
                    {
                        contents = str1 + Environment.NewLine + str2c + Environment.NewLine + "Blank=BlankClip(RAW," + num2 + ")" + Environment.NewLine + str3 + Environment.NewLine + str4c + Environment.NewLine + str6c;
                    }
                }
            }
            else
            {
                if (EDInput.Text == "")
                {
                    if (CreditInput.Text == "")
                    {
                        contents = str1 + Environment.NewLine + str2 + Environment.NewLine + str4 + Environment.NewLine + str5;
                    }
                    else
                    {
                        contents = str1 + Environment.NewLine + str2c + Environment.NewLine + "Blank=BlankClip(RAW," + num2 + ")" + Environment.NewLine + str3 + Environment.NewLine + str4c + Environment.NewLine + str5c;
                    }
                }
                else
                {
                    if (CreditInput.Text == "")
                    {
                        contents = str1 + Environment.NewLine + str2 + Environment.NewLine + str4 + Environment.NewLine + Environment.NewLine + str5 + str6;
                    }
                    else
                    {
                        contents = str1 + Environment.NewLine + str2c + Environment.NewLine + "Blank=BlankClip(RAW," + num2 + ")" + Environment.NewLine + str3 + Environment.NewLine + str4c + Environment.NewLine + str5c + Environment.NewLine + str6c;
                    }
                }
            }
            if (AVS.IsChecked == true)
            {
                File.WriteAllText(Output.Text + "encode.avs", contents, Encoding.ASCII);
            }
            else
            {
                File.WriteAllText("encode.avs", contents, Encoding.ASCII);
            }
        }

        private void BatCreate()
        {
            if (ResolutionCombo.SelectedIndex == 0)
            {
                if (FLAC.IsChecked == false)
                {
                    string Bat = DoubleColon + "Resources\\ffmpeg.exe" + DoubleColon + " -y -i " + DoubleColon + "encode.avs" + DoubleColon + " -vcodec libx264 -crf 18.5 -preset slow -profile:v main -level 4.1 -vf scale=1280:720 -acodec libvo_aacenc " + DoubleColon + Output.Text + DoubleColon;
                    File.WriteAllText("encode.bat", Bat, Encoding.ASCII);
                }
                else
                {
                    string Bat = DoubleColon + "Resources\\ffmpeg.exe" + DoubleColon + " -y -i " + DoubleColon + "encode.avs" + DoubleColon + " -vcodec libx264 -crf 18.5 -preset slow -profile:v main -level 4.1 -vf scale=1280:720 -acodec flac " + DoubleColon + Output.Text + DoubleColon;
                    File.WriteAllText("encode.bat", Bat, Encoding.ASCII);
                }
            }
            if (ResolutionCombo.SelectedIndex == 1)
            {
                if (FLAC.IsChecked == false)
                {
                    string Bat = DoubleColon + "Resources\\ffmpeg.exe" + DoubleColon + " -y -i " + DoubleColon + "encode.avs" + DoubleColon + " -vcodec libx264 -crf 18.5 -bufsize 1500k -preset slow -profile:v main -level 4.1 -vf scale=1920:1080 -acodec libvo_aacenc " + DoubleColon + Output.Text + DoubleColon;
                    File.WriteAllText("encode.bat", Bat, Encoding.ASCII);
                }
                else
                {
                    string Bat = DoubleColon + "Resources\\ffmpeg.exe" + DoubleColon + " -y -i " + DoubleColon + "encode.avs" + DoubleColon + " -vcodec libx264 -crf 18.5 -bufsize 1500k -preset slow -profile:v main -level 4.1 -vf scale=1920:1080 -acodec flac " + DoubleColon + Output.Text + DoubleColon;
                    File.WriteAllText("encode.bat", Bat, Encoding.ASCII);
                }
            }
        }

        private void encodework_DoWork(object sender, DoWorkEventArgs e)
        {
            string currentDirectory = Environment.CurrentDirectory;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = currentDirectory + "\\encode.bat";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;
            Process processTemp = new Process();
            processTemp.StartInfo = startInfo;
            processTemp.EnableRaisingEvents = true;
            processTemp.Start();
            StreamReader reader = processTemp.StandardError;
            string line;
            int count = 0;
            while ((line = reader.ReadLine()) != null)
            {
                File.WriteAllText(currentDirectory + "\\log.txt", line, Encoding.ASCII);
                if (line.Contains("Duration"))
                {
                    duration = ParseDurationLine(line);
                }
                if (line.Contains("time"))
                {
                    time = ParseLine(line);
                    count = 1;
                }
                else
                {
                    count = 0;
                }
                if (count == 1)
                {
                    percent = (Divide(time, duration)) * 100;
                }
            }
            processTemp.WaitForExit();
            processTemp.Close();
            processTemp.Dispose();
        }

        public static double Divide(TimeSpan dividend, TimeSpan divisor)
        {
            return (double)dividend.Ticks / (double)divisor.Ticks;
        }

        private TimeSpan ParseDurationLine(string line)
        {
            var itemsOfData = line.Split(" "[0], "="[0]).Where(s => string.IsNullOrEmpty(s) == false).Select(s => s.Trim().Replace("=", string.Empty).Replace(",", string.Empty)).ToList();
            string duration = "";
            int count = 0;
            foreach (string word in itemsOfData)
            {
                if (count == 1)
                {
                    duration = word;
                    count = 0;
                }
                if (word.Contains("Duration"))
                {
                    count = 1;
                }
            }
            return TimeSpan.Parse(duration);
        }

        private TimeSpan ParseLine(string line)
        {
            string currentDirectory = Environment.CurrentDirectory;
            var itemsOfData = line.Split(" "[0], "="[0]).Where(s => string.IsNullOrEmpty(s) == false).Select(s => s.Trim().Replace("=", string.Empty).Replace(",", string.Empty)).ToList();
            string duration = "";
            int count = 0;
            foreach (string word in itemsOfData)
            {
                if (count == 1)
                {
                    duration = word;
                    count = 0;
                }
                if (word.Contains("time"))
                {
                    count = 1;
                }
            }
            return TimeSpan.Parse(duration);
        }

        private void FLAC_Checked(object sender, RoutedEventArgs e)
        {
            if (Output.Text.Substring(Output.Text.LastIndexOf(".") + 1) != "mkv")
            {
                MessageBoxResult MKVCheck = MessageBox.Show("FLAC Work Only With MKV Container. Change Output Container to MKV?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (MKVCheck == MessageBoxResult.Yes)
                {
                    Output.Text = Output.Text.Substring(0 , Output.Text.LastIndexOf(".") + 1) + "mkv";
                }
                else
                {
                    FLAC.IsChecked = false;
                }

            }
        }
    }
}
