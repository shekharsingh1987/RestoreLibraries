namespace Hexgen.CodeLibrary.RDLReportsRestore
{
    public interface IReportsRestore
    {
        event RestoreDelegates.OnReportsRestoreHandler OnRDLRestore;
        bool IsWindowsAuthenticated { get; set; }
        string ReportingServiceUserName { get; set; }
        string ReportingServicePassword { get; set; }
        void RestoreRDL(string reportingServerUrl, string rdlFilePath, string analysisFolderName, string parentFolderName);
    }
}
