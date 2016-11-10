using Hexgen.CodeLibrary.RDLReportsRestore.ReportService2010;
using System;
using System.IO;
using System.Net;

namespace Hexgen.CodeLibrary.RDLReportsRestore
{
    public class RDLReportsRestoreManager : IReportsRestore
    {
        private const string itemTypeName = "Report";
        private const string parentFolderType = "Folder";

        private bool _isWindowsAuthenticated = true;
        private string _reportingServiceUserName = string.Empty;
        private string _reportingServicePassword = string.Empty;
        private int _reportingServiceExpireTime = 1800;
        public bool IsWindowsAuthenticated
        {
            get
            {
                return _isWindowsAuthenticated;
            }

            set
            {
                _isWindowsAuthenticated = value;
            }
        }
        public string ReportingServiceUserName
        {
            get
            {
                return _reportingServiceUserName;
            }

            set
            {
                _reportingServiceUserName = value;
            }
        }
        public string ReportingServicePassword
        {
            get
            {
                return _reportingServicePassword;
            }

            set
            {
                _reportingServicePassword = value;
            }
        }
        public int ReportingServiceExpireTime
        {
            get { return _reportingServiceExpireTime; }
            set { _reportingServiceExpireTime = value; }
        }

        public event RestoreDelegates.OnReportsRestoreHandler OnRDLRestore;

        public void RestoreRDL(string reportingServerUrl, string rdlFilePath, string analysisFolderName, string parentFolderName)
        {
            if (string.IsNullOrEmpty(parentFolderName))
            {
                FailedToRestore($"parentFolderName is null or empty.");
                //throw new ArgumentNullException($"parentFolderName is null or empty.");
            }

            if (string.IsNullOrEmpty(analysisFolderName))
            {
                FailedToRestore($"analysisFolderName is null or empty.");
                //throw new ArgumentNullException($"analysisFolderName is null or empty.");
            }

            if (string.IsNullOrEmpty(rdlFilePath))
            {
                FailedToRestore($"rdlFilePath is null or empty.");
                //throw new ArgumentNullException($"rdlFilePath is null or empty.");
            }

            if (string.IsNullOrEmpty(reportingServerUrl))
            {
                FailedToRestore($"reportingServerUrl is null or empty.");
                //throw new ArgumentNullException("reportingServerUrl");
            }

            if (File.Exists(rdlFilePath))
            {
                RestoreRdlFile(rdlFilePath, reportingServerUrl, analysisFolderName, parentFolderName);
                RestoredSuccessfully();
            }
            else if (Directory.Exists(rdlFilePath))
            {
                RestoreRdlFiles(rdlFilePath, reportingServerUrl, analysisFolderName, parentFolderName);
                RestoredSuccessfully();
            }
            else
            {
                FailedToRestore($"Could not identify path or directory mentioned.");
            }

        }

        private void RestoreRdlFiles(string rdlFilePath, string reportingServerUrl, string analysisFolderName, string parentFolderName)
        {
            foreach (string filePath in Directory.GetFiles(rdlFilePath, $"*.rdl", SearchOption.AllDirectories))
            {
                RestoreRdlFile(filePath, reportingServerUrl, analysisFolderName, parentFolderName);
            }
        }

        private void RestoreRdlFile(string rdlFilePath, string reportingServerUrl, string analysisFolderName, string parentFolderName)
        {
            try
            {
                CreateFoldersIfNotExists(analysisFolderName, parentFolderName, reportingServerUrl);

                string rdlName = Path.GetFileNameWithoutExtension(rdlFilePath);
                string rdlFile = string.Format("/{0}/{1}/{2}", parentFolderName, analysisFolderName, rdlName);

                using (ReportingService2010 rs = new ReportingService2010())
                {
                    rs.Credentials = CredentialCache.DefaultCredentials;
                    rs.Url = string.Format("{0}/ReportService2010.asmx?wsdl", reportingServerUrl);

                    string fileExists = rs.GetItemType(rdlFile);
                    if (fileExists.Equals(parentFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        rs.DeleteItem(rdlFile);
                    }

                    FileInfo fileInfo = new FileInfo(rdlFilePath);
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);

                    Warning[] warnings = null;
                    byte[] fileContents = File.ReadAllBytes(fileInfo.FullName);


                    string parentPath = string.Format("/{0}/{1}", parentFolderName, analysisFolderName);

                    rs.CreateCatalogItem(itemTypeName, fileNameWithoutExtension, parentPath, true, fileContents, null, out warnings);

                    if (IsWindowsAuthenticated)
                    {
                        RDLDeploy.SetDataSource($"{parentPath}/{fileNameWithoutExtension}", rs);
                    }
                    else
                    {
                        RDLDeploy.SetDataSource($"{parentPath}/{fileNameWithoutExtension}", rs, ReportingServiceUserName, ReportingServicePassword, ReportingServiceExpireTime);
                    }
                }

            }
            catch (NullReferenceException nex)
            {
                FailedToRestore(nex.Message);
            }
            catch (Exception ex)
            {
                FailedToRestore(ex.Message);
            }
        }

        private void OnRDLReportsRestored(RestoreStatus status)
        {
            if (OnRDLRestore != null)
            {
                OnRDLRestore(this, status);
            }
        }
        private void RestoredSuccessfully()
        {
            OnRDLReportsRestored(new RestoreStatus() { status = true, errorInfo = string.Empty });
        }
        private void FailedToRestore(string message)
        {
            OnRDLReportsRestored(new RestoreStatus() { status = false, errorInfo = message });
        }

        private bool ValidateConnection(string reportingServiceUrl)
        {
            bool connectionStatus = false;
            using (ReportingService2010 rs = new ReportingService2010())
            {

                rs.Credentials = CredentialCache.DefaultCredentials;
                rs.Url = string.Format("{0}/ReportService2010.asmx?wsdl", reportingServiceUrl);

                var serviceRequest = (HttpWebRequest)WebRequest.Create(rs.Url);
                serviceRequest.Credentials = CredentialCache.DefaultCredentials;

                try
                {
                    var response = (HttpWebResponse)serviceRequest.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        connectionStatus = true;
                    }
                }
                catch
                {
                    connectionStatus = false;
                }
            }
            return connectionStatus;
        }
        private void CreateFoldersIfNotExists(string analysisFolderName, string parentFolderName, string reportingServerUrl)
        {
            using (ReportingService2010 rs = new ReportingService2010())
            {
                rs.Credentials = CredentialCache.DefaultCredentials;
                rs.Url = string.Format("{0}/ReportService2010.asmx?wsdl", reportingServerUrl);

                string parentFolderExist = rs.GetItemType(string.Format(@"/{0}", parentFolderName));

                if (!parentFolderExist.Equals(parentFolderType, StringComparison.OrdinalIgnoreCase))
                {
                    rs.CreateFolder(parentFolderName, $"/", null);
                }

                string analysisFolderExist = rs.GetItemType(string.Format(@"/{0}/{1}", parentFolderName, analysisFolderName));
                if (!analysisFolderExist.Equals(parentFolderType, StringComparison.OrdinalIgnoreCase))
                {
                    rs.CreateFolder(analysisFolderName, $"/{parentFolderName}", null);
                }
            }
        }
    }

}
