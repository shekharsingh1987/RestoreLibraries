using Hexgen.CodeLibrary.RDLReportsRestore.ReportService2010;
using System;
using System.Collections.Generic;

namespace Hexgen.CodeLibrary.RDLReportsRestore
{
    public enum DataSourceType
    {
        WindowsIntegrated = 0,
        CredentialsStored = 1
    }

    public class RDLDeploy
    {
        public static void SetDataSource(string itemPath, ReportingService2010 report)
        {
            if (report == null)
            {
                throw new ArgumentNullException($"report");
            }

            if (itemPath == null)
            {
                throw new ArgumentNullException($"itemPath");
            }

            List<DataSource> lstDs = new List<DataSource>();

            foreach (DataSource ds in report.GetItemDataSources(itemPath))
            {
                DataSourceDefinition dataDef = (DataSourceDefinition)ds.Item;
                DataSource dsNew = new DataSource();
                DataSourceDefinition dfNew = new DataSourceDefinition();
                dfNew.CredentialRetrieval = CredentialRetrievalEnum.Integrated;
                dfNew.WindowsCredentials = false;
                dfNew.UseOriginalConnectString = false;
                dfNew.Extension = dataDef.Extension;
                dsNew.Name = ds.Name;
                dsNew.Item = dataDef;

                lstDs.Add(dsNew);
            }

            report.SetItemDataSources(itemPath, lstDs.ToArray());
        }

        public static void SetDataSource(string itemPath, ReportingService2010 report, string userName, string password, int timeExpiry)
        {
            if (itemPath == null)
            {
                throw new ArgumentNullException($"itemPath");
            }

            if (report == null)
            {
                throw new ArgumentNullException($"report");
            }

            List<DataSource> lstDs = new List<DataSource>();

            foreach (DataSource ds in report.GetItemDataSources(itemPath))
            {
                DataSourceDefinition tempDef = (DataSourceDefinition)ds.Item;

                DataSource dsNew = new DataSource();

                DataSourceDefinition dfNew = new DataSourceDefinition();

                dfNew.CredentialRetrieval = CredentialRetrievalEnum.Store;
                dfNew.UserName = userName;
                dfNew.Password = password;
                dfNew.Enabled = true;
                dfNew.EnabledSpecified = true;
                dfNew.Extension = $"SQL";
                dfNew.ImpersonateUser = false;
                dfNew.ImpersonateUserSpecified = true;
                dfNew.OriginalConnectStringExpressionBased = true;
                dfNew.UseOriginalConnectString = true;
                dfNew.WindowsCredentials = true;

                dsNew.Name = ds.Name;
                dsNew.Item = dfNew;

                lstDs.Add(dsNew);
            }

            report.SetItemDataSources(itemPath, lstDs.ToArray());

            ExpirationDefinition exprDef = new TimeExpiration() { Minutes = timeExpiry };
            report.SetCacheOptions(itemPath, true, exprDef);
        }
    }
}
