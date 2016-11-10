using System;

namespace Hexgen.CodeLibrary.DBReportsRestore
{
    public class RestoreStatus : EventArgs
    {
        public bool status { get; set; }
        public string errorInfo { get; set; }
    }
}