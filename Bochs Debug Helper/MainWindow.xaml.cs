using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace Bochs_Debug_Helper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] mapFileValues;
        private int[] mapFileID;
        private string mapFilePath = "";
        private bool readFile = false;
        private bool parsedFile = false;
        private string MapFileOutputString = "";

        public MainWindow()
        {
            InitializeComponent();
            UpdateState();
        }

        private void UpdateState()
        {
            MapFileRead.IsChecked = readFile;
            MapFileParsed.IsChecked = parsedFile;
            MapFileAdress.IsEnabled = parsedFile;
            MapFileOutput.Text = MapFileOutputString;
            if (MapFileOutputString == "")
            {
                MapFileAdress.BorderBrush = Brushes.Gray;
            }
            else
            {
                MapFileAdress.BorderBrush = Brushes.Green;
            }
        }

        private void MapFileOpenFile_click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                DefaultExt = ".map",
                Title = "Map file path",
                Multiselect = false
            };
            readFile = false;
            parsedFile = false;
            MapFileOutputString = "";
            UpdateState();
            if (dialog.ShowDialog() ?? false)
            {
                mapFilePath = dialog.FileName;
                Task GetFileData = Task.Run(() =>
               {
                   string path = mapFilePath;
                   string[] data = File.ReadAllLines(path);

                   //Cut the first 4 lines
                   data = data.Skip(4).ToArray();

                   readFile = true;
                   Application.Current.Dispatcher.Invoke(() => UpdateState());
                   return data;
               }).ContinueWith((lines) =>
               {
                   List<int> rowID = new List<int>(lines.Result.Length);
                   List<string> rowValues = new List<string>(lines.Result.Length);

                   Regex regex = new Regex("[ ]{2,}");

                   foreach (string line in lines.Result)
                   {
                       string temp = regex.Replace(line, " ");
                       string[] parts = temp.Split(new char[] { ' ', '\t' });
                       if (parts.Length < 4) continue;
                       rowID.Add(int.Parse(parts[0], NumberStyles.HexNumber));
                       rowValues.Add(parts.Last());
                   }

                   parsedFile = true;
                   mapFileID = rowID.ToArray();
                   mapFileValues = rowValues.ToArray();
                   Application.Current.Dispatcher.Invoke(() => UpdateState());
               });
            }
        }

        private void MapFileAdress_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(MapFileAdress.Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int result))
            {
                Task.Run(() =>
                {
                    MapFileOutputString = "";
                    Application.Current.Dispatcher.Invoke(() => UpdateState());
                    for (int i = 0; i < mapFileID.Length; i++)
                    {
                        int id = mapFileID[i];
                        if (id == result)
                        {
                            MapFileOutputString += mapFileValues[i] + "\n";
                            Application.Current.Dispatcher.Invoke(() => UpdateState());
                        }
                    }
                });
            }
            else
            {
                MapFileOutputString = "";
                UpdateState();
            }
        }
    }
}