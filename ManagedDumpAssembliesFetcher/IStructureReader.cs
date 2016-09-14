namespace ManagedDumpAssembliesFetcher
{
  public interface IStructureReader
  {
    T ReadStructure<T>(ulong address) where T : struct;
  }
}