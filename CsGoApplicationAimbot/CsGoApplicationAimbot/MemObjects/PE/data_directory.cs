using System.Runtime.InteropServices;

namespace CsGoApplicationAimbot.MemObjects.PE
{
    [StructLayout(LayoutKind.Sequential)]
    public struct data_directory
    {
        public int VirtualAddress;
        public int Size;
    }
}