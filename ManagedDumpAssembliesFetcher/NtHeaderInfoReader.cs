using System;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime.Interop;

namespace ManagedDumpAssembliesFetcher
{
  public static class NtHeaderInfoReader
  {
    public static NT_HEADER_INFO_BITNESS_INDEPENDENT FetchNtHeaderInfo(ulong address, IStructureReader structureReader)
    {
      if (Environment.Is64BitProcess) return FetchNtHeaderInfoX64(address, structureReader);

      return FetchNtHeaderInfoX86(address, structureReader);
    }

    private static NT_HEADER_INFO_BITNESS_INDEPENDENT FetchNtHeaderInfoX86(ulong address,
      IStructureReader structureReader)
    {
      var optionalHeaderOffset = Marshal.OffsetOf<IMAGE_NT_HEADERS32>(nameof(IMAGE_NT_HEADERS32.OptionalHeader)).ToInt32();

      var header = structureReader.ReadStructure<IMAGE_NT_HEADERS32>(address);

      return new NT_HEADER_INFO_BITNESS_INDEPENDENT(
        optionalHeaderOffset,
        header.FileHeader.SizeOfOptionalHeader,
        header.FileHeader.NumberOfSections,
        (int)header.OptionalHeader.SizeOfHeaders);
    }

    private static NT_HEADER_INFO_BITNESS_INDEPENDENT FetchNtHeaderInfoX64(ulong address,
      IStructureReader structureReader)
    {
      var optionalHeaderOffset = Marshal.OffsetOf<IMAGE_NT_HEADERS64>(nameof(IMAGE_NT_HEADERS64.OptionalHeader)).ToInt32();

      var header = structureReader.ReadStructure<IMAGE_NT_HEADERS64>(address);

      return new NT_HEADER_INFO_BITNESS_INDEPENDENT(
        optionalHeaderOffset,
        header.FileHeader.SizeOfOptionalHeader,
        header.FileHeader.NumberOfSections,
        (int)header.OptionalHeader.SizeOfHeaders);
    }
  }

  public class NT_HEADER_INFO_BITNESS_INDEPENDENT
  {
    public NT_HEADER_INFO_BITNESS_INDEPENDENT(int optionalHeaderOffset, int sizeOfOptionalHeader, int numberOfSections, int sizeOfHeaders)
    {
      this.OptionalHeaderOffset = optionalHeaderOffset;
      this.SizeOfOptionalHeader = sizeOfOptionalHeader;
      this.NumberOfSections = numberOfSections;
      this.SizeOfHeaders = sizeOfHeaders;
    }

    public int OptionalHeaderOffset { get; }

    public int SizeOfOptionalHeader { get; }

    public int NumberOfSections { get; }

    public int SizeOfHeaders { get; }
  }
}