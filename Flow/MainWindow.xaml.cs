using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace Flow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly byte[] WAD_PATCH = { 0xB8, 0x01, 0x00, 0x00, 0x00 };
        private static readonly byte[] WAD_PATTERN = { 0x83, 0xE6, 0xE0, 0x56, 0x53, 0x57, 0xE8 };
        private static readonly byte WAD_REPLACE = 0xE8;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonSelectLoL_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select your League of Legends executable";
            dialog.Filter = "Executable file (*.exe)|*.exe";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true)
            {
                this.textboxLoLPath.Text = dialog.FileName;
                this.buttonPatch.IsEnabled = true;
            }
        }

        private void buttonPatch_Click(object sender, RoutedEventArgs e)
        {
            using (FileStream fs = new FileStream(this.textboxLoLPath.Text, FileMode.Open))
            {
                FindPattern(fs, WAD_PATTERN);
                long offset = FindByte(fs, WAD_REPLACE);

                if (offset != 0)
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Write(WAD_PATCH, 0, WAD_PATCH.Length);

                    MessageBox.Show("Your League of Legends executable was succesfully patched!", "Flow Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Flow was not able to patch your League of Legends executable", "Flow Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private long FindPattern(Stream stream, byte[] pattern)
        {
            byte[] found = new byte[1];
            int matchCount = 0;
            long currentPosition = stream.Position;

            while (stream.Read(found, 0, 1) == 1)
            {
                if (found[0] == pattern[matchCount])
                {
                    matchCount++;

                    if (matchCount == pattern.Length)
                    {
                        return currentPosition;
                    }
                }
                else
                {
                    matchCount = 0;
                }

                currentPosition = stream.Position;
            }

            return 0;
        }

        private long FindByte(Stream stream, byte toFind)
        {
            byte[] found = new byte[1];
            long currentPosition = stream.Position;

            while (stream.Read(found, 0, 1) == 1)
            {
                if (found[0] == toFind)
                {
                    return currentPosition;
                }

                currentPosition = stream.Position;
            }

            return 0;
        }
    }
}
