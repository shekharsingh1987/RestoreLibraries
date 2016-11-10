namespace Hexgen.CodeLibrary.FileReportsRestore
{
    public interface IFileRestore
    {
        event RestoreDelegates.OnFileRestoreHandler OnFileRestore;
        string DirectorySearchPattern { get; set; }
        string FileTypeSearchPattern { get; set; }

        void Restore(string sourceFilePath, string targetDirectory);
        void RestoreAll(string sourceDirectory, string targetDirectory);
    }
}
