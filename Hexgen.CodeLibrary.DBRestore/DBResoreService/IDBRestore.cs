using Microsoft.SqlServer.Management.Smo;

namespace Hexgen.CodeLibrary.DBReportsRestore
{
    public interface IDBRestore
    {
        event RestoreDelegates.OnDBRestoreHandler OnDBRestore;
        int ConnectionStatementTimeout { get; set; }
        void RestoreDB(string connectionString, string backupFile);
    }
}
