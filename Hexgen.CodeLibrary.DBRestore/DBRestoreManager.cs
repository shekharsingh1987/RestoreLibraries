using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Data.SqlClient;
using System.IO;

namespace Hexgen.CodeLibrary.DBReportsRestore
{
    public class DBRestoreManager : IDBRestore
    {
        private ServerConnection _connection;
        private Server _server;
        private int _connectionStatementTimeout = 3600;
        private Restore _restore = default(Restore);
        private BackupDeviceItem _device = default(BackupDeviceItem);

        public Restore Restore { get { return _restore; } set { _restore = value; } }
        public BackupDeviceItem Device { get { return _device; } set { _device = value; } }
        public int ConnectionStatementTimeout
        {
            get
            {
                return _connectionStatementTimeout;
            }

            set
            {
                _connectionStatementTimeout = value;
            }
        }

        public event RestoreDelegates.OnDBRestoreHandler OnDBRestore;

        public void RestoreDB(string connectionString, string backupFile)
        {
            if (connectionString == null)
            {
                FailedToRestore($"ConnectionString is null.");
               // throw new ArgumentNullException($"ConnectionString  is null.");
            }

            if (backupFile == null)
            {
                FailedToRestore($"BackupFile is null.");
               // throw new ArgumentNullException($"BackupFile is null.");
            }

            if(string.IsNullOrEmpty(connectionString.Trim()) || string.IsNullOrEmpty(backupFile.Trim()))
            {
                FailedToRestore($"Either of the parameter is empty or having whitespace.");
                //throw new ArgumentNullException($"Either of the parameter is empty or whitespace.");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    //polish the connection string                    
                    connection.ConnectionString = connectionString;
                    _connection = new ServerConnection(connection);
                    _connection.StatementTimeout = ConnectionStatementTimeout;

                    _server = new Server(_connection);

                    string databaName = Path.GetFileNameWithoutExtension(backupFile);
                    Restore = new Restore();
                    Restore.Action = RestoreActionType.Database;
                    Restore.Database = databaName;
                    Restore.ReplaceDatabase = true;
                    Device = new BackupDeviceItem(backupFile, DeviceType.File);
                    Restore.Devices.Add(Device);

                    Database currentDb = _server.Databases[databaName];
                    if (currentDb != null)
                    {
                        KillDatabaseProcess(databaName);
                    }

                    //Restore DB
                    Restore.SqlRestore(_server);
                    RestoredSuccessfully();
                }
            }
            catch (InvalidOperationException ioex)
            {
                FailedToRestore(ioex.Message);
            }
            catch (Exception ex)
            {
                FailedToRestore(ex.Message);
            }
            finally
            {
                Device = null;
                Restore = null;
            }
        }

        private void FailedToRestore(string message)
        {
            DBRestored(new RestoreStatus() { status = false, errorInfo = message });
        }

        private void RestoredSuccessfully()
        {
            DBRestored(new RestoreStatus() { status = true, errorInfo = string.Empty });
        }

        private void KillDatabaseProcess(string DatabaseName)
        {
            _server.KillAllProcesses(DatabaseName);
            SqlConnection.ClearAllPools();
        }
        private void DBRestored(RestoreStatus status)
        {
            if (OnDBRestore != null)
            {
                OnDBRestore(this, status);
            }
        }
    }
}
