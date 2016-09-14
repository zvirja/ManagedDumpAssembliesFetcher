# ManagedDumpAssembliesFetcher

Tool to fetch managed assemblies from the memory dump file

## Usage
Ensure to use x86 or x64 depending on the bitness of the memory dump file.

```
>tool.exe "PathToDumpFile" "PathToOutputDirectory" ["Path to custom DAC"] [-noskip]
```

Path to custom DAC is optional and should be used if tool is unable to resolve DAC automatically.  
`-noskip` - set flag if you don't want to skip .NET assemblies.
