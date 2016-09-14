using System;
using System.Runtime.InteropServices;

namespace ManagedDumpAssembliesFetcher
{
  [StructLayout(LayoutKind.Explicit)]
  public struct IMAGE_SECTION_HEADER
  {
    [FieldOffset(0)]
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public char[] Name;

    [FieldOffset(8)]
    public UInt32 VirtualSize;

    [FieldOffset(12)]
    public UInt32 VirtualAddress;

    [FieldOffset(16)]
    public UInt32 SizeOfRawData;

    [FieldOffset(20)]
    public UInt32 PointerToRawData;

    [FieldOffset(24)]
    public UInt32 PointerToRelocations;

    [FieldOffset(28)]
    public UInt32 PointerToLinenumbers;

    [FieldOffset(32)]
    public UInt16 NumberOfRelocations;

    [FieldOffset(34)]
    public UInt16 NumberOfLinenumbers;

    [FieldOffset(36)]
    public uint Characteristics;

    public string Section => new string(this.Name);
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct IMAGE_DOS_HEADER
  {
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public char[] e_magic;       // Magic number
    public UInt16 e_cblp;    // Bytes on last page of file
    public UInt16 e_cp;      // Pages in file
    public UInt16 e_crlc;    // Relocations
    public UInt16 e_cparhdr;     // Size of header in paragraphs
    public UInt16 e_minalloc;    // Minimum extra paragraphs needed
    public UInt16 e_maxalloc;    // Maximum extra paragraphs needed
    public UInt16 e_ss;      // Initial (relative) SS value
    public UInt16 e_sp;      // Initial SP value
    public UInt16 e_csum;    // Checksum
    public UInt16 e_ip;      // Initial IP value
    public UInt16 e_cs;      // Initial (relative) CS value
    public UInt16 e_lfarlc;      // File address of relocation table
    public UInt16 e_ovno;    // Overlay number
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public UInt16[] e_res1;    // Reserved words
    public UInt16 e_oemid;       // OEM identifier (for e_oeminfo)
    public UInt16 e_oeminfo;     // OEM information; e_oemid specific
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public UInt16[] e_res2;    // Reserved words
    public Int32 e_lfanew;      // File address of new exe header

    private string _e_magic => new string(this.e_magic);

    public bool isValid => this._e_magic == "MZ";
  }

}
