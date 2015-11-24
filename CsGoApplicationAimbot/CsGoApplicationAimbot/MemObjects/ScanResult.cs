using System;

namespace CsGoApplicationAimbot.MemObjects
{        
    /// <summary>
    /// Struct that holds basic data about the outcome of a signature-scan
    /// </summary>
    public struct ScanResult
    {
        public bool Success;
        public IntPtr Address;
        public IntPtr Base;
        public IntPtr Offset;
    }
}
