using System;
using System.IO;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace ManagedDumpAssembliesFetcher
{
  public static class Program
  {
    private static void Main(string[] args)
    {
      Console.WriteLine();

      var taskInfo = ReadTaskInfo(args);
      if (taskInfo == null) return;

      WriteConsoleColored(ConsoleColor.DarkCyan, "Managed Dump Assemblies Fetcher\nWritten by Alex Povar.\n");
      Console.WriteLine();

      try
      {
        Console.WriteLine("Initializing runtime... ");
        using (var dt = DataTarget.LoadCrashDump(taskInfo.DumpFilePath))
        {
          var version = dt.ClrVersions.Single();
          var dacPath = taskInfo.KnownDacFilePath ?? dt.SymbolLocator.FindBinary(version.DacInfo);

          var runtime = dt.ClrVersions.Single().CreateRuntime(dacPath);

          Console.WriteLine("Runtime initialized.");
          Console.WriteLine("Started to fetch modules...");
          Console.WriteLine();
          Console.WriteLine("*****************************");
          Console.WriteLine();

          var counterSuccessfully = 0;
          var counterFailed = 0;
          var counterSkippedNet = 0;

          foreach (var module in runtime.Modules)
          {
            if (!module.IsFile) continue;

            var fileName = Path.GetFileName(module.FileName);

            if (!taskInfo.DoNotSkipNetAssemblies && module.Name.StartsWith("C:\\Windows\\Microsoft.Net", StringComparison.OrdinalIgnoreCase))
            {
              WriteConsoleColored(ConsoleColor.DarkGray, $"Skipped: {fileName}");

              counterSkippedNet++;
              continue;
            }

            try
            {
              new ModuleFetcher(module).FetchToFile(taskInfo.OutputDirPath);
              counterSuccessfully++;

              WriteConsoleColored(ConsoleColor.DarkGreen, $"Fetched: {fileName}");
            }
            catch (Exception ex)
            {
              WriteConsoleColored(ConsoleColor.DarkRed, $"Failed to fetch module: {module.Name}. Exception message: {ex.Message}.");
              counterFailed++;
            }
          }

          Console.WriteLine();
          Console.WriteLine("*****************************");
          Console.WriteLine();
          Console.WriteLine($"Finished! Fetched: {counterSuccessfully}, Failed: {counterFailed}, Skipped .NET modules: {counterSkippedNet}");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Failed to fetch modules.");
        Console.WriteLine(ex);
      }
    }

    private static TaskInfo ReadTaskInfo(string[] args)
    {
      var argsWithoutFlags = args.Where(a => !a.Trim().StartsWith("-")).ToArray();
      if (argsWithoutFlags.Length < 2 || argsWithoutFlags.Length > 3)
      {
        WriteConsoleColored(
          ConsoleColor.Red,
          @"
Wrong parameters.

Tool syntax: 
tool.exe DumpFilePath OutputDirPath [DAC file path] [-noskip]

   DAC Path - Optional. Use custom DAC file.
   noskip - Optional. Do not skip .NET assemblies".TrimStart());
        return null;
      }

      var result = new TaskInfo
      {
        DoNotSkipNetAssemblies = args.Any(a => a.Equals("-noskip", StringComparison.OrdinalIgnoreCase)),
        DumpFilePath = argsWithoutFlags[0],
        OutputDirPath = argsWithoutFlags[1]
      };

      if (argsWithoutFlags.Length > 2)
      {
        result.KnownDacFilePath = argsWithoutFlags[2];
      }

      return result;
    }

    private class TaskInfo
    {
      public bool DoNotSkipNetAssemblies { get; set; }

      public string DumpFilePath { get; set; }

      public string OutputDirPath { get; set; }

      public string KnownDacFilePath { get; set; }
    }

    private static void WriteConsoleColored(ConsoleColor color, string text)
    {
      var originalColor = Console.ForegroundColor;
      Console.ForegroundColor = color;

      Console.WriteLine(text);

      Console.ForegroundColor = originalColor;
    }
  }
}