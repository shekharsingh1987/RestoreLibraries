using System;
using System.IO;
using AMO = Microsoft.AnalysisServices;
namespace Hexgen.CodeLibrary.CubeReportsRestore
{
    public class CubeRestoreManager : ICubeRestore
    {
        AMO.Server ssasServer = null;
        public event RestoreDelegates.OnCubeRestoreHandler OnCubeRestore;
        public void RestoreCube(string connectionString, string abfFilePath)
        {
            if (string.IsNullOrEmpty(abfFilePath))
            {
                FailedToRestore($"abfFilePath is null or empty.");
                //throw new ArgumentNullException($"abfFilePath is null or empty.");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                FailedToRestore($"connectionString is null or empty.");
                //throw new ArgumentNullException($"connectionString is null or empty.");
            }

            try
            {
                if (ConnectSSASServer(connectionString))
                {
                    string SSASDBName = Path.GetFileNameWithoutExtension(abfFilePath);
                    ssasServer.Restore(abfFilePath, SSASDBName, true);
                    ssasServer.Restore(abfFilePath, SSASDBName, true);
                    ssasServer.Refresh();

                    AMO.Database ssasDataBase = ssasServer.Databases[SSASDBName];
                    ssasDataBase.Cubes[0].Update();
                    ssasDataBase.Cubes[0].Refresh();

                    RestoredSuccessfully();
                }
                else
                {
                    FailedToRestore($"Failed to connect to the server.");
                }                
            }
            catch (NullReferenceException nex)
            {
                FailedToRestore(nex.Message);
                //throw new Exception(nex.Message);
            }
            catch (Exception ex)
            {
                FailedToRestore(ex.Message);
                //throw new Exception(ex.Message);
            }
            finally
            {
                DisConnectSSASServer();
            }
        }
        private void OnCubeRestored(RestoreStatus status)
        {
            if (OnCubeRestore != null)
            {
                OnCubeRestore(this, status);
            }
        }
        private void RestoredSuccessfully()
        {
            OnCubeRestored(new RestoreStatus() { status = true, errorInfo = string.Empty });
        }
        private void FailedToRestore(string message)
        {
            OnCubeRestored(new RestoreStatus() { status = false, errorInfo = message });
        }
        private bool ConnectSSASServer(string connectionString)
        {
            try
            {
                if (ssasServer == null)
                {
                    ssasServer = new AMO.Server();
                }

                if (ssasServer.GetConnectionState(true) != System.Data.ConnectionState.Open)
                {
                    ssasServer.Connect(connectionString);
                }

                return true;
            }
            catch
            {
                return false;
            }

        }
        private void DisConnectSSASServer()
        {
            if (ssasServer != null)
            {
                if (ssasServer.GetConnectionState(true) == System.Data.ConnectionState.Open)
                {
                    ssasServer.Disconnect(true);
                }
                ssasServer.Dispose();
            }
            ssasServer = null;
        }
    }
}
