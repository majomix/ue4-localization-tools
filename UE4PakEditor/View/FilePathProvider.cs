using Microsoft.Win32;
using UE4PakEditor.ViewModel;
using WinForms = System.Windows.Forms;

namespace UE4PakEditor.View
{
    internal class FilePathProvider : IFilePathProvider
    {
        public string GetOpenFilePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Unreal Engine 4 PAK files (*.pak)|*.pak";
            string filePath = null;
            bool? dialogResult = openFileDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                filePath = openFileDialog.FileName;
            }

            return filePath;
        }

        public string GetDirectoryPath()
        {
            using(WinForms.FolderBrowserDialog fileBrowserDialog = new WinForms.FolderBrowserDialog())
            {
                string filePath = null;
                WinForms.DialogResult dialogResult = fileBrowserDialog.ShowDialog();
                if (dialogResult == WinForms.DialogResult.OK)
                {
                    filePath = fileBrowserDialog.SelectedPath;
                }
                return filePath;
            }
        }


        public string GetSaveFilePath()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Unreal Engine 4 PAK files (*.pak)|*.pak";
            string filePath = null;
            bool? dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                filePath = saveFileDialog.FileName;
            }

            return filePath;
        }
    }
}
