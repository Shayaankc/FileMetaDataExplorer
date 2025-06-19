using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D.Converters;
using WinForms = System.Windows.Forms;

namespace FileMetaDataExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class FileMetadata
    {
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public double SizeKB { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Extension { get; set; }
    }

    public partial class MainWindow : Window
    {
        private bool dateCreated = true;
        public MainWindow()
        {
            InitializeComponent();
        }
        private List<FileMetadata> allFiles = new();
        private List<FileMetadata> filteredFiles = new();


        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog();
            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                string folderPath = dialog.SelectedPath;
                SelectedFolderText.Text = folderPath;

                allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                    .Select(path =>
                    {
                        var info = new FileInfo(path);
                        return new FileMetadata
                        {
                            FileName = info.Name,
                            FullPath = info.FullName,
                            SizeKB = Math.Round(info.Length / 1024.0, 2),
                            CreatedDate = info.CreationTime,
                            ModifiedDate = info.LastWriteTime,
                            Extension = info.Extension
                        };
                    }).ToList();

                ExtensionComboBox.ItemsSource = allFiles.Select(f => f.Extension)
                                                        .Distinct()
                                                        .OrderBy(e => e)
                                                        .ToList();

                FileDataGrid.ItemsSource = allFiles;
                filteredFiles = allFiles;
            }
        }


        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            List<FileMetadata> result = new List<FileMetadata>();

            string name = FilterNameTextBox.Text.ToLower();
            string ext = ExtensionComboBox.Text;
            DateTime? from = CreatedFromDatePicker.SelectedDate;
            DateTime? to = CreatedToDatePicker.SelectedDate;

            foreach (FileMetadata file in allFiles)
            {
                if (name != "" && !file.FileName.ToLower().Contains(name))
                {
                    continue;
                }

                if (ext != "" && file.Extension != ext)
                {
                    continue;
                }

                DateTime fileDate = dateCreated ? file.CreatedDate : file.ModifiedDate;

                if (from.HasValue && fileDate < from.Value)
                {
                    continue;
                }

                if (to.HasValue && fileDate > to.Value)
                {
                    continue;
                }

                result.Add(file);
            }

            filteredFiles = result;
            FileDataGrid.ItemsSource = filteredFiles;
        }


        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "FileMetadata",
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("FileName,FullPath,SizeKB,CreatedDate,ModifiedDate,Extension");

                foreach (var file in filteredFiles)
                {
                    sb.AppendLine($"\"{file.FileName}\",\"{file.FullPath}\",{file.SizeKB}," +
                                  $"{file.CreatedDate},{file.ModifiedDate},\"{file.Extension}\"");
                }

                File.WriteAllText(dialog.FileName, sb.ToString());
                System.Windows.MessageBox.Show("Exported to CSV successfully.");

            }
        }

        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            FileDataGrid.ItemsSource = allFiles;
            filteredFiles = allFiles;
            FilterNameTextBox.Clear();
            ExtensionComboBox.SelectedItem = null;
            CreatedFromDatePicker.SelectedDate = null;
            CreatedToDatePicker.SelectedDate = null;
            dateCreated = false;
            ToggleDate(null, null);

        }

        private void ToggleDate(object sender, RoutedEventArgs e)
        {
            dateCreated = !dateCreated;
            mdc.Content = dateCreated ? "Modified Date" : "Created Date";
            text1.Text = dateCreated ? "Created From:" : "Modified From:";
        }
    }
}