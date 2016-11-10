using Hexgen.CodeLibrary.FileReportsRestore;
using System;

namespace Implementation
{
    class Program
    {
        private const string testQuery = @"IF NOT EXISTS(SELECT * FROM [dbo].[MediaUploadViewModels] WHERE Name = @userName )
                                            Begin
                                            INSERT INTO [dbo].[MediaUploadViewModels]
                                                       ([Name],[Title],[Description],[IsBannerVideo],[Department],[status])
                                                 VALUES(@userName,'Test',@descriptions,0,@descriptions,2)
                                            End                                            
                                            INSERT INTO [dbo].[MediaUploadViewModels]
                                                       ([Name],[Title],[Description],[IsBannerVideo],[Department],[status])
                                                 VALUES(@userName,'Test',@descriptions,0,@descriptions,2)
                                            ";
        static void Main(string[] args)
        {
            const string sourcePath = @"D:\Documents\Visual Studio 2013\Projects";
            const string destinationPath = @"E:\Design Patterns";
            //File restore
            FileRestoreManager fileManager = new FileRestoreManager();
            fileManager.DirectorySearchPattern = $"*DesignPattern";
            fileManager.OnFileRestore += FileManager_OnFileRestore;
            fileManager.RestoreAll(sourcePath, destinationPath);

            //const string connString = @"data source=SHEKHARK\EAUDIT;initial catalog=Hexgenmedialibv2-sqldb_2;persist security info=True;Integrated Security=SSPI;";
            //using (SqlConnection connection = new SqlConnection(connString))
            //{
            //    SqlCommand cmd = new SqlCommand(testQuery);
            //    cmd.CommandType = CommandType.Text;
            //    cmd.Connection = connection;
            //    cmd.Parameters.AddWithValue($"@userName", $"Amit");
            //    cmd.Parameters.AddWithValue($"@descriptions", $"Descriotion");
            //    connection.Open();
            //    cmd.ExecuteNonQuery();

            //}
            Console.ReadKey();
        }

        private static void FileManager_OnFileRestore(object sender, RestoreStatus eventArg)
        {
            Console.WriteLine(eventArg.status.ToString());
            Console.WriteLine(eventArg.errorInfo);
            Console.ReadKey();
        }
    }
}
