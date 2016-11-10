namespace Hexgen.CodeLibrary.CubeReportsRestore
{
    public interface ICubeRestore
    {
        event RestoreDelegates.OnCubeRestoreHandler OnCubeRestore;        
        void RestoreCube(string connectionString, string abfFilePath);        
    }
}
