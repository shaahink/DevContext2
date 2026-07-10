# REPORT
**PowerToys**

Style: Unknown
_120 projects  ·  1 HostedService, 237 UiEntry, 3 CliCommand  ·  net10.0-windows10.0.26100.0, net8.0 + desktop-ui + nlog + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 3637 |
| Projects | 197 |
| Nodes | 5141 |
| Edges | 2878 |
| Entries | 241 |
| With target | 241/241 |
| Verified edges | 83% |
| Analyzed in | 30.0s |

## Top Flows

1. **BaseCommand —settings object** → `BaseCommand` *(CliCommand)*
2. **FancyZonesBaseCommand —settings object** → `FancyZonesBaseCommand` *(CliCommand)*
3. **ImageResizerRootCommand —settings object** → `ImageResizerRootCommand` *(CliCommand)*
4. **Worker** → `Worker` *(HostedService)*
5. **[RelayCommand] AppearanceSettingsViewModel.ResetAppearanceSettings** → `AppearanceSettingsViewModel` *(UiEntry)*
6. **[RelayCommand] AppearanceSettingsViewModel.ResetBackgroundImageProperties** → `AppearanceSettingsViewModel` *(UiEntry)*
7. **[RelayCommand] CommandParameterRunViewModel.Invoke** → `CommandParameterRunViewModel` *(UiEntry)*
8. **[RelayCommand] ContentPageViewModel.InvokePrimaryCommand** → `ContentPageViewModel` *(UiEntry)*
9. **[RelayCommand] ContentPageViewModel.InvokeSecondaryCommand** → `ContentPageViewModel` *(UiEntry)*
10. **[RelayCommand] DevRibbonViewModel.OpenInternalTools** → `DevRibbonViewModel` *(UiEntry)*

### Trace 1: BaseCommand —settings object

TRACE  BaseCommand —settings object
       src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22

▸ ENTRY  BaseCommand —settings object  (src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22)
   └─ call BaseCommand  (src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22)
          /// <summary>
          /// Base class for all DSC commands.
          /// </summary>

---

### Trace 2: FancyZonesBaseCommand —settings object

TRACE  FancyZonesBaseCommand —settings object
       src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16

▸ ENTRY  FancyZonesBaseCommand —settings object  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16)
   └─ call FancyZonesBaseCommand  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16)
          internal abstract class FancyZonesBaseCommand : Command
          protected FancyZonesBaseCommand(string name, string description)
          : base(name, description)

---

### Trace 3: ImageResizerRootCommand —settings object

TRACE  ImageResizerRootCommand —settings object
       src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14

▸ ENTRY  ImageResizerRootCommand —settings object  (src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14)
   └─ call ImageResizerRootCommand  (src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14)
          /// <summary>
          /// Root command for the ImageResizer CLI.
          /// </summary>

---

## Insights

_4 info · 5 notable_

### **NOTABLE**: Internal hubs: 5 heavily-referenced internal types
*(Topology)*

- Logger (679 refs)
- Common (330 refs)
- AllSizesCollection (213 refs)
- NativeMethods (194 refs)
- MachineStuff (117 refs)

### **NOTABLE**: Extension seats: ICommandProvider (14 impls) · AddKeyedSingleton (6 impls) · IUserSettings (2 impls)
*(Wiring)*

- ICommandProvider (14 impls)
- AddKeyedSingleton (6 impls)
- IUserSettings (2 impls)

### **NOTABLE**: Most depended-upon: Common.UI (33 dependents) · PowerToys.Interop (28 dependents) · ManagedCommon (26 dependents)
*(Topology)*

- Common.UI (33 dependents)
- PowerToys.Interop (28 dependents)
- ManagedCommon (26 dependents)

### **NOTABLE**: Event flow: 74 published, 0 consumed, 74 orphan
*(Wiring)*

- 74 orphan events (published, no internal consumer)
- Microsoft.CommandPalette.Extensions.Toolkit.StatusMessage ← CreateNoteCommand.Invoke
- Microsoft.CmdPal.UI.Events.CmdPalDismissedOnEsc ← ShellPage.Receive
- Microsoft.CmdPal.UI.Events.OpenPage ← ShellPage.Receive
- 0/74 events consumed

### **NOTABLE**: Multi-implementation interfaces: ICommandProvider (14 impls) · IUserSettings (2 impls) · IExtensionService (2 impls)
*(Wiring)*

- ICommandProvider (14 impls)
- IUserSettings (2 impls)
- IExtensionService (2 impls)

### _INFO_: Entry targets resolved 241/241 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Module map: 8 feature areas
*(Shape)*

- Controls (34 entries)
- Microsoft/PowerToys/Settings/UI/OOBE/Views (32 entries)
- ViewModels (24 entries)
- Microsoft/PowerToys/Settings/UI/Controls (15 entries)
- MouseWithoutBorders (13 entries)

### _INFO_: Public surface: 231 interfaces, 2306 classes (2806 total public types)
*(Shape)*

- 231 interfaces
- 2306 classes

### _INFO_: Entry surface: 237 UI · 1 hosted
*(Shape)*

- 237 UI
- 1 hosted

MAP  PowerToys     (120 projects)

STACK  net10.0-windows10.0.26100.0, net8.0, netcoreapp3.1

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         PowerToys.Settings.DSC.Schema.Generator: Unknown
         PowerToys.QuickAccess: Unknown
         PowerToys.Settings: Unknown
         Settings.UI.XamlIndexBuilder: Unknown
         PowerToys.DSC: Unknown
         AdvancedPaste: Unknown
         Awake: Unknown
         Microsoft.CmdPal.UI: Unknown
         ColorPickerUI: Unknown
         EnvironmentVariables: Unknown
         FancyZonesCLI: Unknown
         FileLocksmithUI: Unknown
         Hosts: Unknown
         ImageResizerCLI: Unknown
         ImageResizerUI: Unknown
         KeyboardManagerEditorUI: Unknown
         PowerLauncher: Unknown
         MeasureToolUI: Unknown
         MouseJumpUI: Unknown
         MouseWithoutBorders: Unknown
         Peek.UI: Unknown
         PowerAccent.UI: Unknown
         PowerDisplay: Unknown
         PowerOCR: Unknown
         BgcodePreviewHandler: Unknown
         BgcodeThumbnailProvider: Unknown
         GcodePreviewHandler: Unknown
         GcodeThumbnailProvider: Unknown
         MarkdownPreviewHandler: Unknown
         MonacoPreviewHandler: Unknown
         PdfPreviewHandler: Unknown
         PdfThumbnailProvider: Unknown
         QoiPreviewHandler: Unknown
         QoiThumbnailProvider: Unknown
         StlThumbnailProvider: Unknown
         SvgPreviewHandler: Unknown
         SvgThumbnailProvider: Unknown
         RegistryPreview: Unknown
         ShortcutGuide.IndexYmlGenerator: Unknown
         ShortcutGuide.Ui: Unknown
         WorkspacesEditor: Unknown
         WorkspacesLauncherUI: Unknown
         CacheGenerator: Unknown
         Microsoft.CmdPal.Ext.PowerToys: Unknown
         ProcessMonitorExtension: Unknown
         SamplePagesExtension: Unknown
         FancyZonesEditor: Unknown
         MouseWithoutBordersHelper: Unknown
         MouseWithoutBordersService: Unknown
         TemplateCmdPalExtension: Unknown

TOPOLOGY (depends-on)
   ManagedCommon ── ManagedTelemetry
   Common.UI ── ManagedCommon
   Settings.UI.Library ── ManagedCommon, ManagedTelemetry, MouseJump.Common, PowerDisplay.Models
   ManagedTelemetry
   Wox.Infrastructure ── Wox.Plugin
   Wox.Plugin ── Common.UI, ManagedCommon, Settings.UI.Library
   PreviewHandlerCommon
   FilePreviewCommon
   Microsoft.CmdPal.Common ── ManagedCommon, Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.CommandPalette.Extensions.Toolkit
   Common.UI.Controls ── ManagedCommon
   GPOWrapperProjection
   ManagedCsWin32
   PowerDisplay.Models
   FancyZonesEditorCommon
   Microsoft.CmdPal.Ext.Indexer ── ManagedCommon, ManagedCsWin32, Microsoft.CmdPal.Common
   MouseJump.Common
   PowerToys.ModuleContracts ── Common.UI
   WorkspacesCsharpLibrary
   Common.Search
   LanguageModelProvider ── ManagedCommon
   Microsoft.CmdPal.Ext.Apps ── ManagedCommon, ManagedCsWin32, Microsoft.CmdPal.Common
   Peek.Common ── ManagedCommon, ManagedTelemetry
   PowerAccent.Common
   Settings.UI.Controls ── ManagedCommon, Settings.UI.Library
   Awake.ModuleServices ── Common.UI, ManagedCommon, PowerToys.ModuleContracts, Settings.UI.Library
   ColorPicker.ModuleServices ── Common.UI, ManagedCommon, PowerToys.ModuleContracts, Settings.UI.Library
   EnvironmentVariablesUILib
   HostsUILib
   ImageResizerUI ── Common.UI, ManagedCommon
   Microsoft.CmdPal.Ext.Actions ── Microsoft.CmdPal.Ext.Indexer, Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.CmdPal.Ext.Bookmarks ── Microsoft.CmdPal.Common, Microsoft.CmdPal.Ext.Indexer, Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.CmdPal.Ext.Calc ── ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.ClipboardHistory ── ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.PerformanceMonitor ── ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.Registry ── ManagedCommon
   Microsoft.CmdPal.Ext.RemoteDesktop ── ManagedCommon
   Microsoft.CmdPal.Ext.Shell ── Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.System
   Microsoft.CmdPal.Ext.TimeDate ── ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.Ext.WebSearch ── ManagedCommon
   Microsoft.CmdPal.Ext.WindowsServices ── ManagedCommon
   Microsoft.CmdPal.Ext.WindowsSettings ── ManagedCommon
   Microsoft.CmdPal.Ext.WindowsTerminal ── ManagedCommon, ManagedCsWin32
   Microsoft.CmdPal.Ext.WindowWalker ── ManagedCommon, ManagedCsWin32
   Microsoft.CmdPal.Ext.WinGet ── ManagedCommon, Microsoft.CmdPal.Common
   Microsoft.CmdPal.UI.ViewModels ── ManagedCommon, Microsoft.CmdPal.Ext.Apps, Microsoft.CommandPalette.Extensions.Toolkit
   Microsoft.Plugin.Folder ── Settings.UI.Library, Wox.Infrastructure, Wox.Plugin
   Peek.FilePreviewer ── Common.UI, FilePreviewCommon, Peek.Common, Settings.UI.Library
   PowerAccent.Core ── PowerAccent.Common, Settings.UI.Library
   … and 70 more projects (use --focus for a scoped slice)

ENTRY POINTS
   Background (1)
      Worker  → Worker  (src/modules/MouseWithoutBorders/App/Service/Program.cs:50)
   UI (237)
      [RelayCommand] AppearanceSettingsViewModel.ResetAppearanceSettings  → AppearanceSettingsViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/AppearanceSettingsViewModel.cs:508)
      [RelayCommand] AppearanceSettingsViewModel.ResetBackgroundImageProperties  → AppearanceSettingsViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/AppearanceSettingsViewModel.cs:498)
      [RelayCommand] CommandParameterRunViewModel.Invoke  → CommandParameterRunViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/ParametersViewModels.cs:508)
      [RelayCommand] ContentPageViewModel.InvokePrimaryCommand  → ContentPageViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/ContentPageViewModel.cs:296)
      [RelayCommand] ContentPageViewModel.InvokeSecondaryCommand  → ContentPageViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/ContentPageViewModel.cs:306)
      [RelayCommand] DevRibbonViewModel.OpenInternalTools  → DevRibbonViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:104)
      [RelayCommand] DevRibbonViewModel.OpenLogFileAsync  → DevRibbonViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:76)
      [RelayCommand] DevRibbonViewModel.OpenLogFolderAsync  → DevRibbonViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:86)
      [RelayCommand] DevRibbonViewModel.ResetErrorCounters  → DevRibbonViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:96)
      [RelayCommand] DevRibbonViewModel.ToggleDevRibbonVisibility  → DevRibbonViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI/ViewModels/DevRibbonViewModel.cs:110)
      [RelayCommand] DockAppearanceSettingsViewModel.ResetBackgroundImageProperties  → DockAppearanceSettingsViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/DockAppearanceSettingsViewModel.cs:323)
      [RelayCommand] ExtensionGalleryItemViewModel.CancelWinGetAction  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:420)
      [RelayCommand] ExtensionGalleryItemViewModel.CopyWinGetInstall  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:360)
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaStore  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:336)
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaWinGetAsync  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:371)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenAuthorPage  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:327)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenHomepage  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:318)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstalledApps  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:354)
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstallUrl  → ExtensionGalleryItemViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryItemViewModel.cs:345)
      [RelayCommand] ExtensionGalleryViewModel.SortByAuthor  → ExtensionGalleryViewModel  (src/modules/cmdpal/Microsoft.CmdPal.UI.ViewModels/Gallery/ExtensionGalleryViewModel.cs:207)
      … and 217 more (ui entries — use --focus for a drill-in)
   CLI (3)
      BaseCommand —settings object  → BaseCommand  (src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22)
      FancyZonesBaseCommand —settings object  → FancyZonesBaseCommand  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16)
      ImageResizerRootCommand —settings object  → ImageResizerRootCommand  (src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14)

PACKAGES
   ORM/Data:  Microsoft.Data.Sqlite 10.0.8
   Logging:  NLog 5.2.8, NLog.Extensions.Logging 5.3.8, NLog.Schema 5.2.8
   Testing:  Moq 4.18.4, MSTest, MSTest.TestFramework
   Other:  AdaptiveCards.ObjectModel.WinUI3 2.0.0-beta, AdaptiveCards.Rendering.WinUI3 2.1.0-beta, AdaptiveCards.Templating 2.0.5, Appium.WebDriver 4.4.5, CoenM.ImageSharp.ImageHash 1.3.6, CommunityToolkit.Common 8.4.0, CommunityToolkit.Labs.WinUI.Controls.MarkdownTextBlock 0.1.260116-build.2514, CommunityToolkit.Labs.WinUI.Controls.OpacityMaskView 0.1.251101-build.2372 … (92 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 4048ms |
| GenericExtraction | 18009ms |
| SignalSealing | 0ms |
| SpecificExtraction | 3424ms |
| Compression | 383ms |
| **Total** | **30041ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 18006ms | 3966 | 102 |
| DiRegistrationExtractor | 18002ms | 1 | 102 |
| CallGraphExtractor | 2869ms | 0 | 0 |
| ProjectStructure | 2806ms | 0 | 0 |
| ProgramCsFlowExtractor | 1858ms | 0 | 1 |
| FileTreeExtractor | 1022ms | 0 | 0 |
| SourceBodyExtractor | 667ms | 0 | 0 |
| CliCommandExtractor | 546ms | 0 | 298 |
| InMemoryEventBusExtractor | 381ms | 0 | 296 |
| DesktopEntryExtractor | 381ms | 0 | 296 |
| SolutionDiscovery | 216ms | 0 | 0 |
| IndirectWiringDetector | 156ms | 0 | 66 |
| DependencyExtractor | 79ms | 0 | 0 |
| LayerClassifier | 27ms | 0 | 0 |
| AntiPatternDetector | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 2601 | 285 |
| Raises | 114 | 114 |
| Resolves | 163 | 91 |

_3637 files · 197 projects_
