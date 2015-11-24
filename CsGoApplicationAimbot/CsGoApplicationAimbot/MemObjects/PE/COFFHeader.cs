using System.Runtime.InteropServices;

namespace CsGoApplicationAimbot.MemObjects.PE
{
    /// <summary>
    /// Source: https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files#Code_Sections
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct COFFHeader
    {
        short Machine;
        short NumberOfSections;
        int TimeDateStamp;
        int PointerToSymbolTable;
        int NumberOfSymbols;
        short SizeOfOptionalHeader;
        short Characteristics;
    }
}
