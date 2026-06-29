dotnet : Unhandled exception. System.IO.DirectoryNotFoundException: Path not found: 
C:\Users\shahi\AppData\Local\DevContext\repos\VahidN-DntSite-default
At line:1 char:237
+ ... /News ==="; dotnet $cli analyze $dnt --focus "GET /Feed/News" --depth ...
+                 ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : NotSpecified: (Unhandled excep...DntSite-default:String) [], RemoteException
    + FullyQualifiedErrorId : NativeCommandError
 
   at DevContext.Core.Resolvers.ProjectRootResolver.ResolveBaseAsync(String inputPath, IFileSystem fs, 
CancellationToken ct) in C:\Code\DevContext2\src\DevContext.Core\Resolvers\ProjectRootResolver.cs:line 37
   at DevContext.Core.Resolvers.ProjectRootResolver.ResolveAsync(String inputPath, IFileSystem fs, CancellationToken 
ct) in C:\Code\DevContext2\src\DevContext.Core\Resolvers\ProjectRootResolver.cs:line 11
   at DevContext.Cli.Commands.AnalyzeCommand.ExecuteAsync(CommandContext context, AnalyzeSettings settings, 
CancellationToken ct) in C:\Code\DevContext2\src\DevContext.Cli\Commands\AnalyzeCommand.cs:line 70
   at Spectre.Console.Cli.CommandExecutor.ExecuteAsync(CommandTree leaf, CommandTree tree, CommandContext context, 
ITypeResolver resolver, IConfiguration configuration, CancellationToken cancellationToken) in 
/_/src/Spectre.Console.Cli/Internal/CommandExecutor.cs:line 257
   at Spectre.Console.Cli.CommandExecutor.ExecuteAsync(IConfiguration configuration, IEnumerable`1 args, 
CancellationToken cancellationToken) in /_/src/Spectre.Console.Cli/Internal/CommandExecutor.cs:line 128
   at Spectre.Console.Cli.CommandApp.RunAsync(IEnumerable`1 args, CancellationToken cancellationToken) in 
/_/src/Spectre.Console.Cli/CommandApp.cs:line 77
   at Program.<Main>$(String[] args) in C:\Code\DevContext2\src\DevContext.Cli\Program.cs:line 34
   at Program.<Main>(String[] args)
