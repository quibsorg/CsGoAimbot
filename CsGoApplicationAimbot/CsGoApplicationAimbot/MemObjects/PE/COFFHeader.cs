using System.Runtime.InteropServices;

namespace CsGoApplicationAimbot.MemObjects.PE
{
    /// <summary>
    ///     Source: https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files#Code_Sections
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct COFFHeader
    {
        private readonly short Machine;
        private readonly short NumberOfSections;
        private readonly int TimeDateStamp;
        private readonly int PointerToSymbolTable;
        private readonly int NumberOfSymbols;
        private readonly short SizeOfOptionalHeader;
        private readonly short Characteristics;
    }
}