using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Flow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly byte[] WAD_ECDSA_PATCH = { 0xB8, 0x01, 0x00, 0x00, 0x00 };
        private static readonly List<byte[]> WAD_ECDSA_PATTERNS = new List<byte[]>()
        {
            new byte[] { 0x83, 0xE6, 0xE0, 0x56, 0x53, 0x57, 0xE8 },
            new byte[] { 0x93, 0x3C, 0x26, 0x00, 0x56, 0x53, 0x57, 0xE8 }
        };
        private static readonly byte WAD_ECDSA_REPLACE = 0xE8;

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
                long ecdsaOffset = 0;
                for (int i = 0; i < WAD_ECDSA_PATTERNS.Count; i++)
                {
                    FindPattern(fs, WAD_ECDSA_PATTERNS[i]);
                    ecdsaOffset = FindByte(fs, WAD_ECDSA_REPLACE);

                    if (ecdsaOffset != 0)
                    {
                        break;
                    }
                    else
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                    }
                }

                if (ecdsaOffset != 0)
                {
                    fs.Seek(ecdsaOffset, SeekOrigin.Begin);
                    fs.Write(WAD_ECDSA_PATCH, 0, WAD_ECDSA_PATCH.Length);

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
