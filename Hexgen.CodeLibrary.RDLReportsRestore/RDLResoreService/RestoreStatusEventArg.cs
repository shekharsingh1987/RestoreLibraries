using System;

namespace Hexgen.CodeLibrary.RDLReportsRestore
{
    public class RestoreStatus : EventArgs
    {
        public bool status { get; set; }
        public string errorInfo { get; set; }
    }
}