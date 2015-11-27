using System.Runtime.InteropServices;

namespace CsGoApplicationAimbot.MemObjects.PE
{
    [StructLayout(LayoutKind.Sequential)]
    public struct _IMAGE_EXPORT_DIRECTORY
    {
        public int Characteristics; //offset 0x0
        public int TimeDateStamp; //offset 0x4
        public short MajorVersion; //offset 0x8
        public short MinorVersion; //offset 0xa
        public int Name; //offset 0xc
        public int Base; //offset 0x10
        public int NumberOfFunctions; //offset 0x14
        public int NumberOfNames; //offset 0x18
        public int AddressOfFunctions; //offset 0x1c
        public int AddressOfNames; //offset 0x20
        public int AddressOfNameOrdinals; //offset 0x24
    }
}