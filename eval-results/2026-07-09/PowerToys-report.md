# REPORT
**PowerToys**

Style: Unknown
_120 projects  ·  1 HostedService, 237 UiEntry, 3 CliCommand  ·  net10.0-windows10.0.26100.0, net8.0 + desktop-ui + nlog + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 3637 |
| Projects | 197 |
| Nodes | 6488 |
| Edges | 4288 |
| Entries | 241 |
| With target | 241/241 |
| Verified edges | 80% |
| Analyzed in | 36.0s |

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
       PowerToys.DSC
▸ ENTRY  BaseCommand —settings object  (src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22)
   └─ call BaseCommand  (src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22)
          /// <summary>
          /// Base class for all DSC commands.
          /// </summary>

---

### Trace 2: FancyZonesBaseCommand —settings object

TRACE  FancyZonesBaseCommand —settings object
       src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16
       FancyZonesCLI
▸ ENTRY  FancyZonesBaseCommand —settings object  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16)
   └─ call FancyZonesBaseCommand  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16)
          internal abstract class FancyZonesBaseCommand : Command
          protected FancyZonesBaseCommand(string name, string description)
          : base(name, description)

---

### Trace 3: ImageResizerRootCommand —settings object

TRACE  ImageResizerRootCommand —settings object
       src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14
       ImageResizerUI
▸ ENTRY  ImageResizerRootCommand —settings object  (src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14)
   └─ call ImageResizerRootCommand  (src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14)
          /// <summary>
          /// Root command for the ImageResizer CLI.
          /// </summary>

---

## Insights

_5 info · 4 notable_

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

### **NOTABLE**: Multi-implementation interfaces: ICommandProvider (14 impls) · IUserSettings (2 impls) · IExtensionService (2 impls)
*(Wiring)*

- ICommandProvider (14 impls)
- IUserSettings (2 impls)
- IExtensionService (2 impls)

### _INFO_: Entry targets resolved 241/241 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Command inventory: 169 ICommand implementations
*(Wiring)*

- CopyTextCommand
- ToggleFindMyMouseCommand
- CropAndLockThumbnailCommand
- OpenWorkspaceEditorCommand
- CommandResult

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

DESKTOP APP  PowerToys     (120 projects)

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

DESKTOP VIEW
   PowerToys.Settings [Presentation] (51)
      ScoobeReleaseNotesPage
      OobeZoomIt
      OobeWorkspaces
      OobeShortcutGuide
      OobeRun
      OobeRegistryPreview
      OobePowerRename
      OobePowerOCR
      OobePowerDisplay
      OobePowerAccent
      OobePeek
      OobeOverview
      OobeNewPlus
      OobeMouseWithoutBorders
      OobeMouseUtils
      OobeMeasureTool
      OobeLightSwitch
      OobeKBM
      OobeImageResizer
      OobeHosts
      OobeGrabAndMove
      OobeFileLocksmith
      OobeFileExplorer
      OobeFancyZones
      OobeEnvironmentVariables
      OobeCropAndLock
      OobeColorPicker
      OobeCmdPal
      OobeCmdNotFound
      OobeAwake
      OobeAlwaysOnTop
      OobeAdvancedPaste
      Timeline
      ShortcutDialogContentControl
      ShortcutControl
      SettingsPageControl
      FoundryLocalModelPicker
      ShortcutConflictControl
      CheckUpdateControl
      ShellPage
      SearchResultsPage
      ProfileEditorDialog
      PowerDisplayWarningDialog
      CustomVcpMappingEditorDialog
      MouseJumpPanel
      PowerAccentShortcutControl
      OOBEPageControl
      FancyZonesPreviewControl
      ColorPickerButton
      ColorFormatEditor
      NavigablePage
   Microsoft.CmdPal.UI [Presentation] (38)
      PlainTextContentViewer
      ImageContentViewer
      [RelayCommand] DevRibbonViewModel.ToggleDevRibbonVisibility
      [RelayCommand] DevRibbonViewModel.OpenInternalTools
      [RelayCommand] DevRibbonViewModel.ResetErrorCounters
      [RelayCommand] DevRibbonViewModel.OpenLogFolderAsync
      [RelayCommand] DevRibbonViewModel.OpenLogFileAsync
      InternalPage
      GeneralPage
      ExtensionsPage
      ExtensionPage
      ExtensionGalleryPage
      ExtensionGalleryItemPage
      DockSettingsPage
      AppearancePage
      LoadingPage
      ListPage
      ListItemsView
      ContentPage
      PinToDockDialogContent
      DockControl
      DockContentControl
      WinGetOperationsButton
      SearchBar
      ScrollContainer
      ScreenPreview
      ImageViewer
      IconCarouselControl
      FiltersDropDown
      FallbackRankerDialog
      FallbackRanker
      DevRibbon
      ContextMenu
      ContentFormControl
      CommandPalettePreview
      CommandBar
      ColorPalette
      CmdPalMainControl
   Microsoft.CmdPal.UI.ViewModels [Presentation] (27)
      [RelayCommand] WinGetOperationViewModel.Cancel
      [RelayCommand] ExtensionGalleryViewModel.SortByInstallationStatus
      [RelayCommand] ExtensionGalleryViewModel.SortByAuthor
      [RelayCommand] ExtensionGalleryViewModel.SortByName
      [RelayCommand] ExtensionGalleryViewModel.SortByFeatured
      [RelayCommand] ExtensionGalleryItemViewModel.CancelWinGetAction
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaWinGetAsync
      [RelayCommand] ExtensionGalleryItemViewModel.CopyWinGetInstall
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstalledApps
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstallUrl
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaStore
      [RelayCommand] ExtensionGalleryItemViewModel.OpenAuthorPage
      [RelayCommand] ExtensionGalleryItemViewModel.OpenHomepage
      NewExtensionPage
      [RelayCommand] TopLevelCommandManager.LoadExternalProvidersAsync
      [RelayCommand] ShellViewModel.LoadAsync
      [RelayCommand] SettingsExtensionsViewModel.OpenStoreWithExtension
      [RelayCommand] CommandParameterRunViewModel.Invoke
      [RelayCommand] PageViewModel.InitializeAsync
      [RelayCommand] ListViewModel.UpdateSelectedItem
      [RelayCommand] ListViewModel.InvokeSecondaryCommand
      [RelayCommand] ListViewModel.InvokeItem
      [RelayCommand] DockAppearanceSettingsViewModel.ResetBackgroundImageProperties
      [RelayCommand] ContentPageViewModel.InvokeSecondaryCommand
      [RelayCommand] ContentPageViewModel.InvokePrimaryCommand
      [RelayCommand] AppearanceSettingsViewModel.ResetAppearanceSettings
      [RelayCommand] AppearanceSettingsViewModel.ResetBackgroundImageProperties
   ImageResizerUI [Domain] (12)
      ResultsPage
      ProgressPage
      InputPage
      [RelayCommand] ResultsViewModel.Close
      [RelayCommand] ProgressViewModel.Stop
      [RelayCommand] ProgressViewModel.StartAsync
      [RelayCommand] MainViewModel.LoadAsync
      [RelayCommand] InputViewModel.DownloadModelAsync
      [RelayCommand] InputViewModel.OpenSettings
      [RelayCommand] InputViewModel.Cancel
      [RelayCommand] InputViewModel.EnterKeyPressed
      [RelayCommand] InputViewModel.Resize
   Peek.FilePreviewer [Domain] (11)
      UnsupportedFilePreview
      InformationalPreviewControl
      FailedFallbackPreviewControl
      SpecialFolderPreview
      SpecialFolderInformationalPreviewControl
      ShellPreviewHandlerControl
      DriveControl
      BrowserControl
      AudioControl
      ArchiveControl
      FilePreview
   MouseWithoutBorders [Presentation] (11)
      SettingsFormPage
      SettingsForm
      FrmScreen
      FrmMouseCursor
      FrmMessage
      FrmMatrix
      frmLogon
      FrmInputCallback
      FrmAbout
      Machine
      ColorBorderField
   SamplePagesExtension [Contracts] (10)
      SampleSettingsPage
      SampleMarkdownPage
      SampleMarkdownManyBodies
      SampleMarkdownImagesPage
      SampleMarkdownDetails
      SampleTreeContentPage
      SampleImageContentPage
      SamplePlainTextContentPage
      SampleContentPage
      SampleCommentsPage
   AdvancedPaste [Domain] (9)
      [RelayCommand] PromptBox.CancelPasteActionAsync
      [RelayCommand] PromptBox.GenerateCustomAIAsync
      PromptBox
      ClipboardHistoryItemPreviewControl
      [RelayCommand] OptionsViewModel.SetActiveProviderAsync
      [RelayCommand] OptionsViewModel.OpenSettings
      [RelayCommand] OptionsViewModel.NextCustomFormat
      [RelayCommand] OptionsViewModel.PreviousCustomFormat
      [RelayCommand] OptionsViewModel.PasteCustomAsync
   PowerDisplay [Infrastructure] (8)
      [RelayCommand] MonitorViewModel.SetVolume
      [RelayCommand] MonitorViewModel.SetContrast
      [RelayCommand] MonitorViewModel.SetBrightness
      [RelayCommand] MonitorViewModel.SetInputSource
      [RelayCommand] MainViewModel.ApplyProfile
      [RelayCommand] MainViewModel.IdentifyMonitors
      [RelayCommand] MainViewModel.RefreshAsync
      MonitorIcon
   HostsUILib [Presentation] (8)
      [RelayCommand] MainViewModel.OverwriteHosts
      [RelayCommand] MainViewModel.OpenHostsFile
      [RelayCommand] MainViewModel.OpenSettings
      [RelayCommand] MainViewModel.ClearFilters
      [RelayCommand] MainViewModel.ApplyFilters
      [RelayCommand] MainViewModel.ReadHosts
      [RelayCommand] MainViewModel.DeleteEntry
      HostsMainPage
   FancyZonesEditor [Infrastructure] (8)
      MainWindow
      LayoutPreview
      LayoutOverlayWindow
      GridZone
      GridEditor
      EditorWindow
      CanvasZone
      CanvasEditor
   ColorPickerUI [Infrastructure] (7)
      ZoomView
      MainView
      ColorEditorView
      ColorPickerControl
      ColorFormatControl
      ZoomWindow
      ColorEditorWindow
   ShortcutGuide.Ui [Presentation] (4)
      ShortcutsPage
      TaskbarIndicator
      ShortcutItemView
      App
   Peek.UI [Presentation] (3)
      [RelayCommand] TitleBar.Pin
      [RelayCommand] TitleBar.LaunchDefaultAppButtonAsync
      TitleBar
   FileLocksmithUI [Presentation] (3)
      MainPage
      [RelayCommand] MainViewModel.RestartElevated
      [RelayCommand] MainViewModel.EndTask
   Settings.UI.Controls [Presentation] (3)
      QuickAccessList
      Card
      ModuleList
   WorkspacesEditor [Infrastructure] (3)
      ProjectEditor
      SnapshotWindow
      OverlayWindow
   RegistryPreviewUILib [Presentation] (2)
      MonacoEditorControl
      RegistryPreviewMainPage
   Microsoft.CommandPalette.Extensions.Toolkit [Infrastructure] (2)
      ParametersPage
      SettingsContentPage
   PowerToys.QuickAccess [Presentation] (2)
      LaunchPage
      AppsListPage

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
| DiscoveryAndCacheWarmup | 1068ms |
| GenericExtraction | 4030ms |
| SignalSealing | 0ms |
| SpecificExtraction | 6512ms |
| Compression | 724ms |
| **Total** | **35955ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 4026ms | 3966 | 102 |
| DiRegistrationExtractor | 4007ms | 0 | 102 |
| CallGraphExtractor | 3993ms | 0 | 0 |
| CliCommandExtractor | 2509ms | 0 | 298 |
| InMemoryEventBusExtractor | 2214ms | 0 | 296 |
| DesktopEntryExtractor | 2213ms | 0 | 296 |
| SourceBodyExtractor | 1226ms | 0 | 0 |
| BodyFactsExtractor | 629ms | 0 | 0 |
| ProgramCsFlowExtractor | 606ms | 0 | 1 |
| IndirectWiringDetector | 538ms | 0 | 41 |
| ProjectStructure | 452ms | 0 | 0 |
| FileTreeExtractor | 450ms | 0 | 0 |
| SolutionDiscovery | 163ms | 0 | 0 |
| DependencyExtractor | 68ms | 0 | 0 |
| LayerClassifier | 20ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 4125 | 767 |
| Resolves | 163 | 91 |

_3637 files · 197 projects_
