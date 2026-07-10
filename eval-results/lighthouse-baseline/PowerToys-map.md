Overview map (no focus).
Analyzing project...

MAP  PowerToys     (120 projects)

STACK  net10.0-windows10.0.26100.0, net8.0, net9.0, netcoreapp3.1

STYLE  ModularMonolith  (confidence moderate)
       evidence: 4 module-like sub-projects: powertoys.modulecontracts, 
awake.moduleservices, colorpicker.moduleservices, workspaces.moduleservices

TOPOLOGY (depends-on)
   ManagedCommon ÄÄ ManagedTelemetry
   Common.UI ÄÄ ManagedCommon
   Settings.UI.Library ÄÄ ManagedCommon, ManagedTelemetry, MouseJump.Common, 
PowerDisplay.Models
   ManagedTelemetry
   Wox.Infrastructure ÄÄ Wox.Plugin
   Wox.Plugin ÄÄ Common.UI, ManagedCommon, Settings.UI.Library
   PreviewHandlerCommon
   FilePreviewCommon
   Microsoft.CmdPal.Common ÄÄ ManagedCommon, 
Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.CommandPalette.Extensions.Toolkit
   Common.UI.Controls ÄÄ ManagedCommon
   GPOWrapperProjection
   ManagedCsWin32
   PowerDisplay.Models
   FancyZonesEditorCommon
   Microsoft.CmdPal.Ext.Indexer ÄÄ ManagedCommon, ManagedCsWin32, 
Microsoft.CmdPal.Common
   MouseJump.Common
   PowerToys.ModuleContracts ÄÄ Common.UI
   WorkspacesCsharpLibrary
   Common.Search
   LanguageModelProvider ÄÄ ManagedCommon
   Microsoft.CmdPal.Ext.Apps ÄÄ ManagedCommon, ManagedCsWin32, 
Microsoft.CmdPal.Common
   Peek.Common ÄÄ ManagedCommon, ManagedTelemetry
   PowerAccent.Common
   Settings.UI.Controls ÄÄ ManagedCommon, Settings.UI.Library
   Awake.ModuleServices ÄÄ Common.UI, ManagedCommon, PowerToys.ModuleContracts, 
Settings.UI.Library
   ColorPicker.ModuleServices ÄÄ Common.UI, ManagedCommon, 
PowerToys.ModuleContracts, Settings.UI.Library
   EnvironmentVariablesUILib
   HostsUILib
   ImageResizerUI ÄÄ Common.UI, ManagedCommon
   Microsoft.CmdPal.Ext.Actions ÄÄ Microsoft.CmdPal.Ext.Indexer, 
Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.CmdPal.Ext.Bookmarks ÄÄ Microsoft.CmdPal.Common, 
Microsoft.CmdPal.Ext.Indexer, Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.CmdPal.Ext.Calc ÄÄ ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.ClipboardHistory ÄÄ ManagedCommon, 
Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.PerformanceMonitor ÄÄ ManagedCommon, 
Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.Registry ÄÄ ManagedCommon
   Microsoft.CmdPal.Ext.RemoteDesktop ÄÄ ManagedCommon
   Microsoft.CmdPal.Ext.Shell ÄÄ Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.System
   Microsoft.CmdPal.Ext.TimeDate ÄÄ ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.WebSearch ÄÄ ManagedCommon
   Microsoft.CmdPal.Ext.WindowsServices ÄÄ ManagedCommon
   Microsoft.CmdPal.Ext.WindowsSettings ÄÄ ManagedCommon
   Microsoft.CmdPal.Ext.WindowsTerminal ÄÄ ManagedCommon, ManagedCsWin32
   Microsoft.CmdPal.Ext.WindowWalker ÄÄ ManagedCommon, ManagedCsWin32
   Microsoft.CmdPal.Ext.WinGet ÄÄ ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.UI.ViewModels ÄÄ ManagedCommon, Microsoft.CmdPal.Ext.Apps, 
Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.Plugin.Folder ÄÄ Settings.UI.Library, Wox.Infrastructure, 
Wox.Plugin
   Peek.FilePreviewer ÄÄ Common.UI, FilePreviewCommon, Peek.Common, 
Settings.UI.Library
   PowerAccent.Core ÄÄ PowerAccent.Common, Settings.UI.Library
   . and 70 more projects (use --focus for a scoped slice)

ENTRY POINTS
   Background (1)
      Worker   Worker  
(src/modules/MouseWithoutBorders/App/Service/Program.cs:50)
   UI (237)
      [RelayCommand] AppearanceSettingsViewModel.ResetAppearanceSettings   
AppearanceSettingsViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/AppearanceSettingsViewModel.c
s:508)
      [RelayCommand] AppearanceSettingsViewModel.ResetBackgroundImageProperties
 AppearanceSettingsViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/AppearanceSettingsViewModel.c
s:498)
      [RelayCommand] CommandParameterRunViewModel.Invoke   
CommandParameterRunViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/ParametersViewModels.cs:508)
      [RelayCommand] ContentPageViewModel.InvokePrimaryCommand   
ContentPageViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/ContentPageViewModel.cs:296)
      [RelayCommand] ContentPageViewModel.InvokeSecondaryCommand   
ContentPageViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/ContentPageViewModel.cs:306)
      [RelayCommand] DevRibbonViewModel.OpenInternalTools   DevRibbonViewModel
(src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:104)
      [RelayCommand] DevRibbonViewModel.OpenLogFileAsync   DevRibbonViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:76)
      [RelayCommand] DevRibbonViewModel.OpenLogFolderAsync   DevRibbonViewModel
(src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:86)
      [RelayCommand] DevRibbonViewModel.ResetErrorCounters   DevRibbonViewModel
(src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:96)
      [RelayCommand] DevRibbonViewModel.ToggleDevRibbonVisibility   
DevRibbonViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:110)
      [RelayCommand] 
DockAppearanceSettingsViewModel.ResetBackgroundImageProperties   
DockAppearanceSettingsViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/DockAppearanceSettingsViewMod
el.cs:323)
      [RelayCommand] ExtensionGalleryItemViewModel.CancelWinGetAction   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:420)
      [RelayCommand] ExtensionGalleryItemViewModel.CopyWinGetInstall   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:360)
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaStore   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:336)
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaWinGetAsync   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:371)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenAuthorPage   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:327)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenHomepage   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:318)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstalledApps   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:354)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstallUrl   
ExtensionGalleryItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemV
iewModel.cs:345)
      [RelayCommand] ExtensionGalleryViewModel.SortByAuthor   
ExtensionGalleryViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryViewM
odel.cs:207)
      . and 217 more (ui entries - use --focus for a drill-in)
   CLI (20)
      AsyncCommand -settings object   AsyncCommand  
(src/settings-ui/Settings.UI/Helpers/AsyncCommand.cs:14)
      BaseCommand -settings object   BaseCommand  
(src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22)
      ButtonClickCommand -settings object   ButtonClickCommand  
(src/settings-ui/Settings.UI.Library/ViewModels/Commands/ButtonClickCommand.cs:1
0)
      Command -settings object   Command  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Com
mand.cs:7)
      CommandContextItem -settings object   CommandContextItem  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Com
mandContextItem.cs:7)
      CommandItem -settings object   CommandItem  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Com
mandItem.cs:11)
      CommandItemViewModel -settings object   CommandItemViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/CommandItemViewModel.cs:17)
      CommandParameterRun -settings object   CommandParameterRun  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Par
ameters/CommandParameterRun.cs:7)
      CommandProvider -settings object   CommandProvider  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Com
mandProvider.cs:9)
      CommandProviderWrapper -settings object   CommandProviderWrapper  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/CommandProviderWrapper.cs:17)
      CommandResult -settings object   CommandResult  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Com
mandResult.cs:7)
      ContentPageViewModel -settings object   ContentPageViewModel  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/ContentPageViewModel.cs:17)
      DefaultCommandProviderCache -settings object   
DefaultCommandProviderCache  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Services/DefaultCommandProvid
erCache.cs:12)
      EmptyCommandProviderContext -settings object   
EmptyCommandProviderContext  
(src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/CommandProviderContext.cs:11)
      FancyZonesBaseCommand -settings object   FancyZonesBaseCommand  
(src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand
.cs:16)
      ImageResizerRootCommand -settings object   ImageResizerRootCommand  
(src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14)
      InvokableCommand -settings object   InvokableCommand  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Inv
okableCommand.cs:7)
      Page -settings object   Page  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Pag
e.cs:7)
      RelayCommand -settings object   RelayCommand  
(src/modules/registrypreview/RegistryPreviewUILib/Controls/HexBox/CanvasCommands
.cs:15)
      Settings -settings object   Settings  
(src/modules/cmdpal/extensionsdk/Microsoft.CommandPalette.Extensions.Toolkit/Set
tings.cs:11)

PACKAGES
   ORM/Data:  Microsoft.Data.Sqlite 10.0.8
   Logging:  NLog 5.2.8, NLog.Extensions.Logging 5.3.8, NLog.Schema 5.2.8
   Testing:  Moq 4.18.4, MSTest, MSTest.TestFramework
   Other:  AdaptiveCards.ObjectModel.WinUI3 2.0.0-beta, 
AdaptiveCards.Rendering.WinUI3 2.1.0-beta, AdaptiveCards.Templating 2.0.5, 
Appium.WebDriver 4.4.5, CoenM.ImageSharp.ImageHash 1.3.6, 
CommunityToolkit.Common 8.4.0, 
CommunityToolkit.Labs.WinUI.Controls.MarkdownTextBlock 0.1.260116-build.2514, 
CommunityToolkit.Labs.WinUI.Controls.OpacityMaskView 0.1.251101-build.2372 . (92
total)

 drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus 
<TypeName>)

analyzed 3637 files ú 5206 nodes ú 2924 edges ú 258 entries ú 258/258 target ú 
~2710 tokens ú 22.6s stage2 x2.1 stage3 x2.2

                                    Insights                                    
ÚÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Sev  ³ Category ³ Title                        ³ Evidence                    ³
ÃÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ NOTE ³ Wiring   ³ Possible dead code: 5 public ³ ImageButton,                ³
³      ³          ³ types with zero inbound      ³ PromptTemplate,             ³
³      ³          ³ references                   ³ ImageDecodeOptions          ³
³ NOTE ³ Topology ³ Most depended-upon:          ³ Common.UI (33 dependents),  ³
³      ³          ³ Common.UI (33 dependents) ú  ³ PowerToys.Interop (28       ³
³      ³          ³ PowerToys.Interop (28        ³ dependents), ManagedCommon  ³
³      ³          ³ dependents) ú ManagedCommon  ³ (26 dependents)             ³
³      ³          ³ (26 dependents)              ³                             ³
³ NOTE ³ Wiring   ³ Multi-implementation         ³ ElevationHelper (3 impls),  ³
³      ³          ³ interfaces: ElevationHelper  ³ FileSystem (3 impls), ? (3  ³
³      ³          ³ (3 impls) ú FileSystem (3    ³ impls)                      ³
³      ³          ³ impls) ú ? (3 impls)         ³                             ³
³ INFO ³ Coverage ³ Entry targets resolved       ³                             ³
³      ³          ³ 258/258 (100%) - use --focus ³                             ³
³      ³          ³ for deeper traces            ³                             ³
³ INFO ³ Wiring   ³ DI: 83 Singleton ú 12        ³                             ³
³      ³          ³ Extension ú 6 Transient (101 ³                             ³
³      ³          ³ total)                       ³                             ³
³ INFO ³ Shape    ³ Entry surface: 237 UI ú 20   ³ 237 UI, 20 CLI, 1 hosted    ³
³      ³          ³ CLI ú 1 hosted               ³                             ³
ÀÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
                     Stage Timing                     
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Stage                   ³    Time ³ Bar            ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ DiscoveryAndCacheWarmup ³  2286ms ³ ÛÛÛÛ           ³
³ GenericExtraction       ³  5501ms ³ ÛÛÛÛÛÛÛÛÛ      ³
³ SignalSealing           ³     2ms ³                ³
³ SpecificExtraction      ³  8446ms ³ ÛÛÛÛÛÛÛÛÛÛÛÛÛÛ ³
³ Compression             ³   568ms ³ Û              ³
³ Total                   ³ 22633ms ³                ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

                                   Extractors                                   
ÚÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³ Name                     ³   Time ³ +Types ³ +Dets ³ Status                  ³
ÃÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ CallGraphExtractor       ³ 5903ms ³      0 ³     0 ³ ran                     ³
³ SyntaxStructureExtractor ³ 5498ms ³   3966 ³   102 ³ ran                     ³
³ DiRegistrationExtractor  ³ 5491ms ³     49 ³   102 ³ ran                     ³
³ SourceBodyExtractor      ³ 5168ms ³      0 ³     0 ³ ran                     ³
³ DesktopEntryExtractor    ³ 2530ms ³      0 ³   325 ³ ran                     ³
³ InMemoryEventBusExtracto ³ 2467ms ³      0 ³   291 ³ ran                     ³
³ r                        ³        ³        ³       ³                         ³
³ CliCommandExtractor      ³ 2152ms ³      0 ³   264 ³ ran                     ³
³ FileTreeExtractor        ³ 1064ms ³      0 ³     0 ³ ran                     ³
³ ProjectStructure         ³  907ms ³      0 ³     0 ³ ran                     ³
³ ProgramCsFlowExtractor   ³  707ms ³      0 ³     1 ³ ran                     ³
³ IndirectWiringDetector   ³  695ms ³      0 ³    69 ³ ran                     ³
³ SolutionDiscovery        ³  293ms ³      0 ³     0 ³ ran                     ³
³ DependencyExtractor      ³   67ms ³      0 ³     0 ³ ran                     ³
³ LayerClassifier          ³   25ms ³      0 ³     0 ³ ran                     ³
³ AntiPatternDetector      ³    0ms ³      0 ³     0 ³ skipped: gated by       ³
³                          ³        ³        ³       ³ ShouldRun               ³
³ AspireExtractor          ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs aspire            ³
³ AwsLambdaExtractor       ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs aws-lambda        ³
³ AzureFunctionsExtractor  ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs azure-functions   ³
³ BlazorEntryExtractor     ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs blazor or         ³
³                          ³        ³        ³       ³ controllers             ³
³ ControllerActionExtracto ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³ r                        ³        ³        ³       ³ needs controllers       ³
³ EfCoreExtractor          ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs efcore            ³
³ EndpointExtractor        ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs minimal-apis or   ³
³                          ³        ³        ³       ³ fast-endpoints or       ³
³                          ³        ³        ³       ³ controllers             ³
³ EventBusExtractor        ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs masstransit or    ³
³                          ³        ³        ³       ³ nservicebus             ³
³ GraphQlResolverExtractor ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs graphql           ³
³ GrpcServiceExtractor     ³    0ms ³      0 ³     0 ³ skipped: signal gate:   ³
³                          ³        ³        ³       ³ needs grpc              ³
ÀÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ

         Graph Seams         
ÚÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄ¿
³ Seam     ³ Edges ³ Approx ³
ÃÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄ´
³ Calls    ³  2618 ³    285 ³
³ Raises   ³   146 ³    146 ³
³ Resolves ³   160 ³     91 ³
ÀÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÙ
5206 nodes ú 2924 edges ú 258/258 entries  target
cache 0% hit ú 3637 files ú 0 projects
ÚÄÄÄÄÄÄÄÄÄÄÂÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ¿
³  Metric  ³        Value         ³
ÃÄÄÄÄÄÄÄÄÄÄÅÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄ´
³ Solution ³    PowerToys.slnx    ³
³   Time   ³       23180ms        ³
³  Tokens  ³ ~2710 (budget 8000)  ³
³ Version  ³ v1.0.5-preview.0.244 ³
ÀÄÄÄÄÄÄÄÄÄÄÁÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÙ
