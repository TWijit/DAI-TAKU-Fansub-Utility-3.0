using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for AQS.xaml
    /// </summary>
    public partial class AQS : Page
    {
        public AQS()
        {
            InitializeComponent();
        }
        private void QuoteRun(object sender, RoutedEventArgs e) /// "Run" Button Click
        {
            if (InputText.Text != "Input" && OutputText.Text != "Output" && File.Exists(InputText.Text) == true && File.Exists(OutputText.Text) == false) /// Check for Input and Output exists and Output not exists
            {
                QuoteStart(); /// Start Quote Operation
            }
            else if (InputText.Text != "Input" && OutputText.Text != "Output" && File.Exists(InputText.Text) == true && File.Exists(OutputText.Text) == true) /// Check for Input and Output exists and Output exists
            {
                MessageBoxResult OutputFileExits = MessageBox.Show("Output File Exits. Overwrite?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (OutputFileExits == MessageBoxResult.Yes)
                {
                    QuoteStart();
                }
            }
            else if (InputText.Text == "Input") /// Check for Input and input file is not specified 
            {
                MessageBox.Show("Input File Is Not Specified.");
            }
            else if (InputText.Text != "Input" && File.Exists(InputText.Text) == false) /// Check for Input and input file is not exists 
            {
                MessageBox.Show("Input File Is Not Exists.");
            }
        }
        private void QuoteStart() /// Quote Operation method
        {
            int LineCounter; /// For Counting how many line in file
            string InputTextStream = InputText.Text; /// Assign location of file to object
            StreamReader streamReader = new StreamReader(InputTextStream); /// Create stream reader
            var lineCount = 0; /// For Check what line currently work with
            using (var reader = File.OpenText(InputText.Text)) /// Counting Line
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }
            string[] Line = new string[lineCount]; /// Create Array for store file information
            for (LineCounter = 0; LineCounter < lineCount; LineCounter++) /// Store file information into array
            {
                Line[LineCounter] = streamReader.ReadLine();
            }
            streamReader.Close(); /// Close steam reader
            for (LineCounter = 0; LineCounter < lineCount; LineCounter++) /// Quote Operation
            {
                int indexOfDot = IndexOfNth(Line[LineCounter], ',', 9); /// Find index of 9th "," 
                if (Line[LineCounter].Contains("Dialogue:")) /// Check for Dialogue Line
                {
                    if (Line[LineCounter].Contains("{")) /// Check if Dialogue Line has been Quote before
                    {
                        int LastIndex = Line[LineCounter].Length - 1; /// Fine last index for line
                        if (Line[LineCounter][LastIndex] != '}') /// Check If last index was not a "}"
                        {
                            Line[LineCounter] = Line[LineCounter] + '}'; /// Add "}" to the end of line
                        }
                        for (int LineIndex = Line[LineCounter].Length - 1; LineIndex >= 0; LineIndex--) /// Loop from the end of line when last index of line is "}"
                        {
                            if (Line[LineCounter][LineIndex] == '{') /// Check for "{"
                            {
                                if (Line[LineCounter][LineIndex - 1] != '}' && LineIndex - 1 > indexOfDot) /// Check if infront of { is not "}" and infront of "{" is not 9th ","
                                {
                                    Line[LineCounter] = Line[LineCounter].Substring(0, LineIndex) + '}' + Line[LineCounter].Substring(LineIndex); /// Add "}" infront of "{"
                                }
                            }
                        }
                        for (int LineIndex = Line[LineCounter].Length - 1; LineIndex >= 0; LineIndex--) /// Loop from the end of line when last index of line
                        {
                            if (Line[LineCounter][LineIndex] == '}') /// Check for "}"
                            {
                                if (LineIndex + 1 < Line[LineCounter].Length && Line[LineCounter][LineIndex + 1] != '{') /// check if after "}" is not "{"
                                {
                                    Line[LineCounter] = Line[LineCounter].Substring(0, LineIndex + 1) + '{' + Line[LineCounter].Substring(LineIndex + 1); /// Add "{" after "}"
                                }
                            }
                        }
                        if (Line[LineCounter][indexOfDot + 1] != '{') /// Check if after 9th "," is not "{"
                        {
                            Line[LineCounter] = Line[LineCounter].Substring(0, indexOfDot + 1) + "{" + Line[LineCounter].Substring(indexOfDot + 1); /// Add "{" after 9th ","
                        }
                    }
                    else /// If Dialogue Line has not been Quote before
                    {
                        if (indexOfDot > 0)
                            Line[LineCounter] = Line[LineCounter].Substring(0, indexOfDot + 1) + "{" + Line[LineCounter].Substring(indexOfDot + 1) + "}"; /// Add "{" after 9th "," and "}" at the end of the Line
                    }
                }
            }
            string result = string.Join(Environment.NewLine, Line); /// Join Array into string without merge into single line
            File.WriteAllText(@OutputText.Text, result, Encoding.Unicode); /// Write file at Output location with Unicode encoding
            MessageBox.Show("Completed");
        }
        private int IndexOfNth(string str, char c, int n) /// Method for finding index of 9 ","
        {
            int remaining = n; /// Create object for count the 9th ","
            for (int i = 0; i < str.Length; i++) /// Line Loop
            {
                if (str[i] == c) /// Check if charactor is ","
                {
                    remaining--; /// Minus remaining by 1
                    if (remaining == 0) /// Check If remaining is 0
                    {
                        return i; /// Return index of 9th ","
                    }
                }
            }
            return -1; /// For complete method mush has something to send back even no condition is met
        }
        private void AutoQuoteInputBrowse(object sender, RoutedEventArgs e) /// Method For Input browse browser
        {
            OpenFileDialog QuoteInputFileDialog = new OpenFileDialog(); /// Create browser
            if (InputText.Text != "") /// Check If input text is not "Input"
            {
                string Inputfolder = System.IO.Path.GetDirectoryName(InputText.Text); /// Get last Input location
                QuoteInputFileDialog.InitialDirectory = Inputfolder; /// Open Browser at last Input location
            }
            QuoteInputFileDialog.Filter = "ASS (.ass)|*.ass"; /// Filter file for ".ass"
            QuoteInputFileDialog.FilterIndex = 1; /// Select ".ass" for filter
            bool? userClickedOKOpen = QuoteInputFileDialog.ShowDialog(); /// Show browser and "Open" button check
            if (userClickedOKOpen == true) /// Check if user Click "Open" Button
            {
                string InputName = ""; /// Create Inputname object
                int InputExtensionIndex = QuoteInputFileDialog.FileName.LastIndexOf("."); /// check for last index of "."
                if (InputExtensionIndex > 0) /// Check if file has extesnsion
                    InputName = QuoteInputFileDialog.FileName.Substring(0, InputExtensionIndex); /// Delete file extension
                string QuoteName = "_Quote.ass"; /// Create QuoteName object
                InputText.Text = QuoteInputFileDialog.FileName; /// Display file loaction with File Name an Extension at Input Textbox 
                OutputText.Text = InputName + QuoteName; /// Display file location without original Extension and add "_Quote.ass" at the end of the line at Output Textbox 
            }
        }
        private void AutoQuoteOutputBrowse(object sender, RoutedEventArgs e) /// Method For Output browse browser
        {
            SaveFileDialog QuoteOutputFileDialog = new SaveFileDialog(); /// Create browser
            if (OutputText.Text != "") /// Check If output text is not "Output"
            {
                string Outputfolder = System.IO.Path.GetDirectoryName(OutputText.Text); /// Get last Output location
                QuoteOutputFileDialog.InitialDirectory = Outputfolder; /// Open Browser at last Output location
                QuoteOutputFileDialog.FileName = OutputText.Text; /// Set file location from Output Textbox at Output browser
            }
            QuoteOutputFileDialog.Filter = "ASS (.ass)|*.ass"; /// Filter file for ".ass"
            QuoteOutputFileDialog.FilterIndex = 1; /// Select ".ass" for filter
            bool? userClickedOK = QuoteOutputFileDialog.ShowDialog(); /// Show browser and "OK" button check
            if (userClickedOK == true) /// Check if user Click "OK" Button
            {
                OutputText.Text = QuoteOutputFileDialog.FileName; /// Display file loaction at Output Textbox 
            }
        }
        private void OpenQuoteFolder(object sender, RoutedEventArgs e) /// Method for Open Output folder
        {
            if (OutputText.Text != "") /// Check If output text is not "Output"
            {
                string OutputTextFolder = OutputText.Text; /// Create Output location object
                int index = OutputTextFolder.LastIndexOf("\\"); /// Check for last "\"
                if (index > 0) /// If index of "|" is not 0
                    OutputTextFolder = OutputTextFolder.Substring(0, index); /// Delete file name
                Process.Start(OutputTextFolder); /// Open Windows Explorer at Output folder location
            }
        }
        private void OpenQuoteFile(object sender, RoutedEventArgs e) /// Method for Open Output file
        {
            if (OutputText.Text != "" && File.Exists(OutputText.Text) == true) /// Check If output text is not "Output" and Output file is exists
            {
                Process.Start(OutputText.Text); /// Open Output File
            }
            else if (OutputText.Text != "" && File.Exists(OutputText.Text) == false) /// Check If output text is not "Output" and Output file is not exists
            {
                MessageBox.Show("Output File Not Exists.");
            }
        }
    }
}
