using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Diagnostics.Runtime;

namespace ManagedDumpAssembliesFetcher
{
  public class ModuleFetcher : IStructureReader
  {
    /*
     * This logic was copied from SaveModule implementation, strike.cpp, https://github.com/dotnet/coreclr
     */

    private readonly DataTarget _dataTarget;
    private readonly ClrModule _module;

    public ModuleFetcher(ClrModule module)
    {
      if (module.IsDynamic) throw new InvalidOperationException("Module is dynamic, while file modules could be stored only.");

      this._module = module;
      this._dataTarget = module.Runtime.DataTarget;
    }

    T IStructureReader.ReadStructure<T>(ulong address) => this.ReadStructure<T>(address);

    public void FetchToFile(string directoryPath, string fileName = null)
    {
      if (fileName == null) fileName = Path.GetFileName(this._module.FileName);

      var outputFilePath = Path.Combine(directoryPath, fileName);
      this.DoFetch(this._module.ImageBase, outputFilePath);
    }

    private void DoFetch(ulong dllBase, string filePath)
    {
      var dosHeader = this.ReadStructure<IMAGE_DOS_HEADER>(dllBase);
      if (!dosHeader.isValid)
      {
        throw new InvalidOperationException($"Unable to find MZ mark at the beginning. The specied address is not a module. Address: 0x{dllBase:x}, Module name: {this._module.Name}.");
      }

      var ntHeaderInfo = NtHeaderInfoReader.FetchNtHeaderInfo(dllBase + (ulong)dosHeader.e_lfanew, this);

      var sectionAddr = dllBase + (ulong)dosHeader.e_lfanew + (ulong)ntHeaderInfo.OptionalHeaderOffset + (ulong)ntHeaderInfo.SizeOfOptionalHeader;

      var memLocations = new List<MemLocation>();

      var nextSectionAddress = sectionAddr;
      for (var i = 0; i < ntHeaderInfo.NumberOfSections; i++)
      {
        var sectionHeader = this.ReadStructure<IMAGE_SECTION_HEADER>(nextSectionAddress);

        memLocations.Add(new MemLocation(sectionHeader.VirtualAddress, sectionHeader.VirtualSize, sectionHeader.PointerToRawData, (int)sectionHeader.SizeOfRawData));

        nextSectionAddress = nextSectionAddress + (ulong)Marshal.SizeOf<IMAGE_SECTION_HEADER>();
      }

      //Ensure that sections are ordered by their file address.
      var orderedLocations = memLocations.OrderBy(m => m.FileAddr);
      this.WriteModuleToFile(dllBase, orderedLocations, ntHeaderInfo, filePath);
    }

    private void WriteModuleToFile(ulong dllBase, IOrderedEnumerable<MemLocation> locations, NT_HEADER_INFO_BITNESS_INDEPENDENT ntHeaderInfo, string filePath)
    {
      using (var stream = new FileStream(filePath, FileMode.Create))
      {
        this.WriteMemoryRegToFile(stream, dllBase, ntHeaderInfo.SizeOfHeaders);

        foreach (var memLocation in locations)
        {
          //dwAddr = dllBase + (bIsImage ? memLoc[slot].VAAddr : memLoc[slot].FileAddr);
          var address = dllBase + memLocation.VAAddr;

          this.WriteMemoryRegToFile(stream, address, memLocation.FileSize);
        }
      }
    }

    private void WriteMemoryRegToFile(FileStream stream, ulong address, int size)
    {
      var buffer = this.ReadBytesSafe(address, size);
      stream.Write(buffer, 0, buffer.Length);
    }


    private T ReadStructure<T>(ulong address) where T : struct
    {
      var structSize = Marshal.SizeOf<T>();
      var buffer = this.ReadBytesSafe(address, structSize);

      var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
      var result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));

      handle.Free();
      return result;
    }

    private byte[] ReadBytesSafe(ulong address, int size)
    {
      var buffer = new byte[size];
      int actualRead;
      if (!this._dataTarget.ReadProcessMemory(address, buffer, size, out actualRead) || (actualRead != size))
      {
        throw new InvalidOperationException($"Unable to read memory. Address: 0x{address:x}, Buffer size: {address}.");
      }

      return buffer;
    }

    private class MemLocation
    {
      public MemLocation(ulong vaAddr, ulong vaSize, ulong fileAddr, int fileSize)
      {
        this.VAAddr = vaAddr;
        this.VASize = vaSize;
        this.FileAddr = fileAddr;
        this.FileSize = fileSize;
      }

      public ulong VAAddr { get; }
      public ulong VASize { get; }
      public ulong FileAddr { get; }
      public int FileSize { get; }
    }
  }
}