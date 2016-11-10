using System;
using System.IO;

namespace Hexgen.CodeLibrary.FileReportsRestore
{
    public class FileRestoreManager : IFileRestore
    {
        private string _directorySearchPattern = $"*";
        private string _fileTypeSearchPattern = $"*.*";
        public string DirectorySearchPattern
        {
            get
            {
                return _directorySearchPattern;
            }

            set
            {
                _directorySearchPattern = value;
            }
        }

        public string FileTypeSearchPattern
        {
            get
            {
                return _fileTypeSearchPattern;
            }

            set
            {
                _fileTypeSearchPattern = value;
            }
        }

        public event RestoreDelegates.OnFileRestoreHandler OnFileRestore;

        public void Restore(string sourceFilePath, string targetDirectory)
        {
            if (string.IsNullOrEmpty(targetDirectory))
            {
                FailedToRestore($"The target path is empty of null.");
                //throw new ArgumentNullException($"targetDirectory is empty or null.");
            }

            if (string.IsNullOrEmpty(sourceFilePath))
            {
                FailedToRestore($"The source path is empty of null.");
                //throw new ArgumentNullException($"sourceDirectory is empty or null.");
            }
            if (File.Exists(sourceFilePath))
            {
                string sourcePath = sourceFilePath;
                string targetFileName = $"{ targetDirectory}\\{Path.GetFileName(sourcePath)}";
                File.Copy(sourcePath, targetFileName, true);
                FileRestored(new RestoreStatus() { status = true, errorInfo = string.Empty });
            }
            else
            {
                FailedToRestore($"The source path doesn't exist.");
            }
        }

        public void RestoreAll(string sourceDirectory, string targetDirectory)
        {
            if (string.IsNullOrEmpty(targetDirectory))
            {
                FailedToRestore($"The target path is empty of null.");
                //throw new ArgumentNullException($"targetDirectory is empty or null.");
            }

            if (string.IsNullOrEmpty(sourceDirectory))
            {
                FailedToRestore($"The source path is empty of null.");
                //throw new ArgumentNullException($"sourceDirectory is empty or null.");
            }

            if(!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            if (Directory.Exists(sourceDirectory) && Directory.Exists(targetDirectory))
            {
                try
                {
                    foreach (string dirPath in Directory.GetDirectories(sourceDirectory, DirectorySearchPattern, SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourceDirectory, targetDirectory));
                    }

                    foreach (string newPath in Directory.GetFiles(sourceDirectory, FileTypeSearchPattern, SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(sourceDirectory, targetDirectory), true);
                    }
                    RemoveFileReadOnly(targetDirectory);

                    RestoredSuccessfully();
                }
                catch (FileNotFoundException fex)
                {
                    FailedToRestore(fex.Message);
                }
                catch (Exception ex)
                {
                    FailedToRestore(ex.Message);
                }
            }
            else
            {
                FileRestored(new RestoreStatus() { status = false, errorInfo = $"The source path doesn't exist." });
            }
        }
        private void RemoveFileReadOnly(string filePath)
        {
            FileAttributes attributes = File.GetAttributes(filePath);

            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                // Make the file RW
                attributes = RemoveAttribute(attributes, FileAttributes.ReadOnly);
                File.SetAttributes(filePath, attributes);
            }
        }
        private FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        private void FileRestored(RestoreStatus status)
        {
            if (OnFileRestore != null)
            {
                OnFileRestore(this, status);
            }
        }

        private void RestoredSuccessfully()
        {
            FileRestored(new RestoreStatus() { status = true, errorInfo = string.Empty });
        }
        private void FailedToRestore(string message)
        {
            FileRestored(new RestoreStatus() { status = false, errorInfo = message });
        }
    }
}
