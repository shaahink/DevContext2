# REPORT
**PowerToys**

Style: Unknown
_123 projects  ·  1 HostedService, 237 UiEntry, 4 CliCommand  ·  net10.0-windows10.0.26100.0, net8.0 + desktop-ui + nlog + cli-commands_

## Stats

| Metric | Value |
|--------|-------|
| Files | 3800 |
| Projects | 208 |
| Nodes | 6654 |
| Edges | 4399 |
| Entries | 242 |
| With target | 242/242 |
| Deep spine (>=2) | 242/242 (100%) |
| Verified edges | 80% |
| Analyzed in | 58.9s |

## Top Flows

1. **BaseCommand —settings object** → `BaseCommand` *(CliCommand)*
2. **FancyZonesBaseCommand —settings object** → `FancyZonesBaseCommand` *(CliCommand)*
3. **ImageResizerRootCommand —settings object** → `ImageResizerRootCommand` *(CliCommand)*
4. **PowerDisplayRootCommand —settings object** → `PowerDisplayRootCommand` *(CliCommand)*
5. **Worker** → `Worker` *(HostedService)*
6. **[RelayCommand] AppearanceSettingsViewModel.ResetAppearanceSettings** → `AppearanceSettingsViewModel` *(UiEntry)*
7. **[RelayCommand] AppearanceSettingsViewModel.ResetBackgroundImageProperties** → `AppearanceSettingsViewModel` *(UiEntry)*
8. **[RelayCommand] CommandParameterRunViewModel.Invoke** → `CommandParameterRunViewModel` *(UiEntry)*
9. **[RelayCommand] ContentPageViewModel.InvokePrimaryCommand** → `ContentPageViewModel` *(UiEntry)*
10. **[RelayCommand] ContentPageViewModel.InvokeSecondaryCommand** → `ContentPageViewModel` *(UiEntry)*

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
      ├─ call LogTelemetry  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:64) [verified]
      │      private void LogTelemetry(bool successful)
      │      try
      │      PowerToysTelemetry.Log.WriteEvent(new FancyZonesCLICommandEvent
      │  ├─ call PowerToysTelemetry  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:72) [verified]
      │  │      /// <summary>
      │  │      /// Telemetry helper class for PowerToys.
      │  │      /// </summary>
      │  ├─ call Logger.LogError  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:81) [verified]
      │  │      public static void LogError(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
      │  │      Log("ERROR", message, memberName, sourceFilePath, sourceLineNumber);
      │  │  └─ call Logger.Log  (src/modules/fancyzones/FancyZonesCLI/Logger.cs:75) [verified]
      │  │         private static void Log(string level, string message, string memberName, string sourceFilePath, int sourceLineNumber)
      │  │         if (!_isInitialized || string.IsNullOrEmpty(_logFilePath))
      │  │         return;
      │  └─ call WriteEvent  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:72) [verified]
      │         [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode", Justification = "We will ensure the public properties won't be trimmed by ourself.")]
      │         public void WriteEvent<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T telemetryEvent)
      │         where T : EventBase, IEvent
      │     ├─ call PowerToysTelemetry  (src/common/ManagedTelemetry/Telemetry/PowerToysTelemetry.cs:43) [verified]
      │     │      /// <summary>
      │     │      /// Telemetry helper class for PowerToys.
      │     │      /// </summary>
      │     ├─ call PowerToysTelemetry.Write  (src/common/ManagedTelemetry/Telemetry/PowerToysTelemetry.cs:43) [verified]
      │     └─ call DataDiagnosticsSettings.GetEnabledValue  (src/common/ManagedTelemetry/Telemetry/PowerToysTelemetry.cs:41) [verified]
      │            public static bool GetEnabledValue()
      │            object registryValue = null;
      │            try
      ├─ call Logger.LogError  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:57) [verified]
      │      public static void LogError(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
      │      Log("ERROR", message, memberName, sourceFilePath, sourceLineNumber);
      │  (stopped at depth 2; 1 branch omitted)
      ├─ call Logger.LogDebug  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:47) [verified]
      │      [System.Diagnostics.Conditional("DEBUG")]
      │      public static void LogDebug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
      │      Log("DEBUG", message, memberName, sourceFilePath, sourceLineNumber);
      │  └─ call Logger.Log  (src/modules/fancyzones/FancyZonesCLI/Logger.cs:100) [verified]
      │         private static void Log(string level, string message, string memberName, string sourceFilePath, int sourceLineNumber)
      │         if (!_isInitialized || string.IsNullOrEmpty(_logFilePath))
      │         return;
      ├─ call Logger.LogInfo  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:46) [verified]
      │      public static void LogInfo(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
      │      Log("INFO", message, memberName, sourceFilePath, sourceLineNumber);
      │  └─ call Logger.Log  (src/modules/fancyzones/FancyZonesCLI/Logger.cs:91) [verified]
      │         private static void Log(string level, string message, string memberName, string sourceFilePath, int sourceLineNumber)
      │         if (!_isInitialized || string.IsNullOrEmpty(_logFilePath))
      │         return;
      ├─ call FancyZonesBaseCommand.Execute  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:42) [verified]
      │      protected abstract string Execute(InvocationContext context);
      ├─ call Logger.LogWarning  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:33) [verified]
      │      public static void LogWarning(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
      │      Log("WARN", message, memberName, sourceFilePath, sourceLineNumber);
      │  └─ call Logger.Log  (src/modules/fancyzones/FancyZonesCLI/Logger.cs:83) [verified]
      │         private static void Log(string level, string message, string memberName, string sourceFilePath, int sourceLineNumber)
      │         if (!_isInitialized || string.IsNullOrEmpty(_logFilePath))
      │         return;
      └─ call FancyZonesCliGuards.IsFancyZonesRunning  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:31) [verified]
             public static bool IsFancyZonesRunning()
             try
             return Process.GetProcessesByName("PowerToys.FancyZones").Length != 0;

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

_5 info · 2 notable_

### **NOTABLE**: Most depended-upon: Common.UI (33 dependents) · PowerToys.Interop (28 dependents) · ManagedCommon (27 dependents)
*(Topology)*

- Common.UI (33 dependents)
- PowerToys.Interop (28 dependents)
- ManagedCommon (27 dependents)

### **NOTABLE**: Multi-implementation interfaces: ICommandProvider (14 impls) · IUserSettings (2 impls) · IExtensionService (2 impls)
*(Wiring)*

- ICommandProvider (14 impls)
- IUserSettings (2 impls)
- IExtensionService (2 impls)

### _INFO_: Command inventory: 171 ICommand implementations
*(Wiring)*

- CopyTextCommand
- ToggleFindMyMouseCommand
- CropAndLockThumbnailCommand
- OpenWorkspaceEditorCommand
- CommandResult

### _INFO_: Entry targets resolved 242/242 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: Module map: 8 feature areas
*(Shape)*

- Controls (34 entries)
- Microsoft/PowerToys/Settings/UI/OOBE/Views (32 entries)
- ViewModels (24 entries)
- Microsoft/PowerToys/Settings/UI/Controls (15 entries)
- MouseWithoutBorders (13 entries)

### _INFO_: ViewModel-View: 147 VMs + 220 Views (3 call edges)
*(Wiring)*

- 147 ViewModels
- 220 Views

### _INFO_: Entry surface: 237 UI · 1 hosted
*(Shape)*

- 237 UI
- 1 hosted

DESKTOP APP  PowerToys     (123 projects)

STACK  net10.0-windows10.0.26100.0, net8.0, netcoreapp3.1

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         PowerToys.Settings.DSC.Schema.Generator: Unknown
         PowerToys.QuickAccess: Unknown
         PowerToys.Settings: Unknown
         Settings.UI.XamlIndexBuilder: CLI [CLI]
         PowerToys.DSC: CLI [CLI]
         AdvancedPaste: Unknown
         Awake: CLI [CLI]
         Microsoft.CmdPal.UI: Unknown
         ColorPickerUI: Unknown
         EnvironmentVariables: Unknown
         FancyZonesCLI: CLI [CLI]
         FileLocksmithUI: Unknown
         Hosts: Unknown
         ImageResizerCLI: CLI [CLI]
         ImageResizerUI: CLI [CLI]
         KeyboardManagerEditorUI: Unknown
         PowerLauncher: Unknown
         MeasureToolUI: Unknown
         MouseJumpUI: Unknown
         MouseWithoutBorders: Unknown
         Peek.UI: Unknown
         PowerAccent.UI: Unknown
         PowerDisplay: Unknown
         PowerDisplay.Cli: CLI [CLI]
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
         MouseWithoutBordersService: Worker Service [Worker]
         TemplateCmdPalExtension: Unknown

DESKTOP VIEW
   PowerToys.Settings [Presentation] (51)
      ScoobeReleaseNotesPage  → ScoobeReleaseNotesPage
      OobeZoomIt  → OobeZoomIt
      OobeWorkspaces  → OobeWorkspaces
      OobeShortcutGuide  → OobeShortcutGuide
      OobeRun  → OobeRun
      OobeRegistryPreview  → OobeRegistryPreview
      OobePowerRename  → OobePowerRename
      OobePowerOCR  → OobePowerOCR
      OobePowerDisplay  → OobePowerDisplay
      OobePowerAccent  → OobePowerAccent
      OobePeek  → OobePeek
      OobeOverview  → OobeOverview
      OobeNewPlus  → OobeNewPlus
      OobeMouseWithoutBorders  → OobeMouseWithoutBorders
      OobeMouseUtils  → OobeMouseUtils
      OobeMeasureTool  → OobeMeasureTool
      OobeLightSwitch  → OobeLightSwitch
      OobeKBM  → OobeKBM
      OobeImageResizer  → OobeImageResizer
      OobeHosts  → OobeHosts
      OobeGrabAndMove  → OobeGrabAndMove
      OobeFileLocksmith  → OobeFileLocksmith
      OobeFileExplorer  → OobeFileExplorer
      OobeFancyZones  → OobeFancyZones
      OobeEnvironmentVariables  → OobeEnvironmentVariables
      OobeCropAndLock  → OobeCropAndLock
      OobeColorPicker  → OobeColorPicker
      OobeCmdPal  → OobeCmdPal
      OobeCmdNotFound  → OobeCmdNotFound
      OobeAwake  → OobeAwake
      OobeAlwaysOnTop  → OobeAlwaysOnTop
      OobeAdvancedPaste  → OobeAdvancedPaste
      Timeline  → Timeline
      ShortcutDialogContentControl  → ShortcutDialogContentControl
      ShortcutControl  → ShortcutControl
      SettingsPageControl  → SettingsPageControl
      FoundryLocalModelPicker  → FoundryLocalModelPicker
      ShortcutConflictControl  → ShortcutConflictControl
      CheckUpdateControl  → CheckUpdateControl
      ShellPage  → ShellPage
      SearchResultsPage  → SearchResultsPage
      ProfileEditorDialog  → ProfileEditorDialog
      PowerDisplayWarningDialog  → PowerDisplayWarningDialog
      CustomVcpMappingEditorDialog  → CustomVcpMappingEditorDialog
      MouseJumpPanel  → MouseJumpPanel
      PowerAccentShortcutControl  → PowerAccentShortcutControl
      OOBEPageControl  → OOBEPageControl
      FancyZonesPreviewControl  → FancyZonesPreviewControl
      ColorPickerButton  → ColorPickerButton
      ColorFormatEditor  → ColorFormatEditor
      NavigablePage  → NavigablePage
   Microsoft.CmdPal.UI [Presentation] (38)
      PlainTextContentViewer  → PlainTextContentViewer
      ImageContentViewer  → ImageContentViewer
      [RelayCommand] DevRibbonViewModel.ToggleDevRibbonVisibility  → DevRibbonViewModel
      [RelayCommand] DevRibbonViewModel.OpenInternalTools  → DevRibbonViewModel
      [RelayCommand] DevRibbonViewModel.ResetErrorCounters  → DevRibbonViewModel
      [RelayCommand] DevRibbonViewModel.OpenLogFolderAsync  → DevRibbonViewModel
      [RelayCommand] DevRibbonViewModel.OpenLogFileAsync  → DevRibbonViewModel
      InternalPage  → InternalPage
      GeneralPage  → GeneralPage
      ExtensionsPage  → ExtensionsPage
      ExtensionPage  → ExtensionPage
      ExtensionGalleryPage  → ExtensionGalleryPage
      ExtensionGalleryItemPage  → ExtensionGalleryItemPage
      DockSettingsPage  → DockSettingsPage
      AppearancePage  → AppearancePage
      LoadingPage  → LoadingPage
      ListPage  → ListPage
      ListItemsView  → ListItemsView
      ContentPage  → ContentPage
      PinToDockDialogContent  → PinToDockDialogContent
      DockControl  → DockControl
      DockContentControl  → DockContentControl
      WinGetOperationsButton  → WinGetOperationsButton
      SearchBar  → SearchBar
      ScrollContainer  → ScrollContainer
      ScreenPreview  → ScreenPreview
      ImageViewer  → ImageViewer
      IconCarouselControl  → IconCarouselControl
      FiltersDropDown  → FiltersDropDown
      FallbackRankerDialog  → FallbackRankerDialog
      FallbackRanker  → FallbackRanker
      DevRibbon  → DevRibbon
      ContextMenu  → ContextMenu
      ContentFormControl  → ContentFormControl
      CommandPalettePreview  → CommandPalettePreview
      CommandBar  → CommandBar
      ColorPalette  → ColorPalette
      CmdPalMainControl  → CmdPalMainControl
   Microsoft.CmdPal.UI.ViewModels [Presentation] (27)
      [RelayCommand] WinGetOperationViewModel.Cancel  → WinGetOperationViewModel
      [RelayCommand] ExtensionGalleryViewModel.SortByInstallationStatus  → ExtensionGalleryViewModel
      [RelayCommand] ExtensionGalleryViewModel.SortByAuthor  → ExtensionGalleryViewModel
      [RelayCommand] ExtensionGalleryViewModel.SortByName  → ExtensionGalleryViewModel
      [RelayCommand] ExtensionGalleryViewModel.SortByFeatured  → ExtensionGalleryViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.CancelWinGetAction  → ExtensionGalleryItemViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaWinGetAsync  → ExtensionGalleryItemViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.CopyWinGetInstall  → ExtensionGalleryItemViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstalledApps  → ExtensionGalleryItemViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.OpenInstallUrl  → ExtensionGalleryItemViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.InstallViaStore  → ExtensionGalleryItemViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.OpenAuthorPage  → ExtensionGalleryItemViewModel
      [RelayCommand] ExtensionGalleryItemViewModel.OpenHomepage  → ExtensionGalleryItemViewModel
      NewExtensionPage  → NewExtensionPage
      [RelayCommand] TopLevelCommandManager.LoadExternalProvidersAsync  → TopLevelCommandManager
      [RelayCommand] ShellViewModel.LoadAsync  → ShellViewModel
      [RelayCommand] SettingsExtensionsViewModel.OpenStoreWithExtension  → SettingsExtensionsViewModel
      [RelayCommand] CommandParameterRunViewModel.Invoke  → CommandParameterRunViewModel
      [RelayCommand] PageViewModel.InitializeAsync  → PageViewModel
      [RelayCommand] ListViewModel.UpdateSelectedItem  → ListViewModel
      [RelayCommand] ListViewModel.InvokeSecondaryCommand  → ListViewModel
      [RelayCommand] ListViewModel.InvokeItem  → ListViewModel
      [RelayCommand] DockAppearanceSettingsViewModel.ResetBackgroundImageProperties  → DockAppearanceSettingsViewModel
      [RelayCommand] ContentPageViewModel.InvokeSecondaryCommand  → ContentPageViewModel
      [RelayCommand] ContentPageViewModel.InvokePrimaryCommand  → ContentPageViewModel
      [RelayCommand] AppearanceSettingsViewModel.ResetAppearanceSettings  → AppearanceSettingsViewModel
      [RelayCommand] AppearanceSettingsViewModel.ResetBackgroundImageProperties  → AppearanceSettingsViewModel
   ImageResizerUI [Infrastructure] (12)
      ResultsPage  → ResultsPage
      ProgressPage  → ProgressPage
      InputPage  → InputPage
      [RelayCommand] ResultsViewModel.Close  → ResultsViewModel
      [RelayCommand] ProgressViewModel.Stop  → ProgressViewModel
      [RelayCommand] ProgressViewModel.StartAsync  → ProgressViewModel
      [RelayCommand] MainViewModel.LoadAsync  → MainViewModel
      [RelayCommand] InputViewModel.DownloadModelAsync  → InputViewModel
      [RelayCommand] InputViewModel.OpenSettings  → InputViewModel
      [RelayCommand] InputViewModel.Cancel  → InputViewModel
      [RelayCommand] InputViewModel.EnterKeyPressed  → InputViewModel
      [RelayCommand] InputViewModel.Resize  → InputViewModel
   Peek.FilePreviewer [Shared] (11)
      UnsupportedFilePreview  → UnsupportedFilePreview
      InformationalPreviewControl  → InformationalPreviewControl
      FailedFallbackPreviewControl  → FailedFallbackPreviewControl
      SpecialFolderPreview  → SpecialFolderPreview
      SpecialFolderInformationalPreviewControl  → SpecialFolderInformationalPreviewControl
      ShellPreviewHandlerControl  → ShellPreviewHandlerControl
      DriveControl  → DriveControl
      BrowserControl  → BrowserControl
      AudioControl  → AudioControl
      ArchiveControl  → ArchiveControl
      FilePreview  → FilePreview
   MouseWithoutBorders [Application] (11)
      SettingsFormPage  → SettingsFormPage
      SettingsForm  → SettingsForm
      FrmScreen  → FrmScreen
      FrmMouseCursor  → FrmMouseCursor
      FrmMessage  → FrmMessage
      FrmMatrix  → FrmMatrix
      frmLogon  → frmLogon
      FrmInputCallback  → FrmInputCallback
      FrmAbout  → FrmAbout
      Machine  → Machine
      ColorBorderField  → ColorBorderField
   SamplePagesExtension [Presentation] (10)
      SampleSettingsPage  → SampleSettingsPage
      SampleMarkdownPage  → SampleMarkdownPage
      SampleMarkdownManyBodies  → SampleMarkdownManyBodies
      SampleMarkdownImagesPage  → SampleMarkdownImagesPage
      SampleMarkdownDetails  → SampleMarkdownDetails
      SampleTreeContentPage  → SampleTreeContentPage
      SampleImageContentPage  → SampleImageContentPage
      SamplePlainTextContentPage  → SamplePlainTextContentPage
      SampleContentPage  → SampleContentPage
      SampleCommentsPage  → SampleCommentsPage
   AdvancedPaste [Domain] (9)
      [RelayCommand] PromptBox.CancelPasteActionAsync  → PromptBox
      [RelayCommand] PromptBox.GenerateCustomAIAsync  → PromptBox
      PromptBox  → PromptBox
      ClipboardHistoryItemPreviewControl  → ClipboardHistoryItemPreviewControl
      [RelayCommand] OptionsViewModel.SetActiveProviderAsync  → OptionsViewModel
      [RelayCommand] OptionsViewModel.OpenSettings  → OptionsViewModel
      [RelayCommand] OptionsViewModel.NextCustomFormat  → OptionsViewModel
      [RelayCommand] OptionsViewModel.PreviousCustomFormat  → OptionsViewModel
      [RelayCommand] OptionsViewModel.PasteCustomAsync  → OptionsViewModel
   PowerDisplay [Infrastructure] (8)
      [RelayCommand] MonitorViewModel.SetVolume  → MonitorViewModel
      [RelayCommand] MonitorViewModel.SetContrast  → MonitorViewModel
      [RelayCommand] MonitorViewModel.SetBrightness  → MonitorViewModel
      [RelayCommand] MonitorViewModel.SetInputSource  → MonitorViewModel
      [RelayCommand] MainViewModel.ApplyProfile  → MainViewModel
      [RelayCommand] MainViewModel.IdentifyMonitors  → MainViewModel
      [RelayCommand] MainViewModel.RefreshAsync  → MainViewModel
      MonitorIcon  → MonitorIcon
   HostsUILib [Presentation] (8)
      [RelayCommand] MainViewModel.OverwriteHosts  → MainViewModel
      [RelayCommand] MainViewModel.OpenHostsFile  → MainViewModel
      [RelayCommand] MainViewModel.OpenSettings  → MainViewModel
      [RelayCommand] MainViewModel.ClearFilters  → MainViewModel
      [RelayCommand] MainViewModel.ApplyFilters  → MainViewModel
      [RelayCommand] MainViewModel.ReadHosts  → MainViewModel
      [RelayCommand] MainViewModel.DeleteEntry  → MainViewModel
      HostsMainPage  → HostsMainPage
   FancyZonesEditor [Domain] (8)
      MainWindow  → MainWindow
      LayoutPreview  → LayoutPreview
      LayoutOverlayWindow  → LayoutOverlayWindow
      GridZone  → GridZone
      GridEditor  → GridEditor
      EditorWindow  → EditorWindow
      CanvasZone  → CanvasZone
      CanvasEditor  → CanvasEditor
   ColorPickerUI [Application] (7)
      ZoomView  → ZoomView
      MainView  → MainView
      ColorEditorView  → ColorEditorView
      ColorPickerControl  → ColorPickerControl
      ColorFormatControl  → ColorFormatControl
      ZoomWindow  → ZoomWindow
      ColorEditorWindow  → ColorEditorWindow
   ShortcutGuide.Ui [Presentation] (4)
      ShortcutsPage  → ShortcutsPage
      TaskbarIndicator  → TaskbarIndicator
      ShortcutItemView  → ShortcutItemView
      App  → App
   Peek.UI [Presentation] (3)
      [RelayCommand] TitleBar.Pin  → TitleBar
      [RelayCommand] TitleBar.LaunchDefaultAppButtonAsync  → TitleBar
      TitleBar  → TitleBar
   FileLocksmithUI [Presentation] (3)
      MainPage  → MainPage
      [RelayCommand] MainViewModel.RestartElevated  → MainViewModel
      [RelayCommand] MainViewModel.EndTask  → MainViewModel
   Settings.UI.Controls [Presentation] (3)
      QuickAccessList  → QuickAccessList
      Card  → Card
      ModuleList  → ModuleList
   WorkspacesEditor [Infrastructure] (3)
      ProjectEditor  → ProjectEditor
      SnapshotWindow  → SnapshotWindow
      OverlayWindow  → OverlayWindow
   RegistryPreviewUILib [Presentation] (2)
      MonacoEditorControl  → MonacoEditorControl
      RegistryPreviewMainPage  → RegistryPreviewMainPage
   Microsoft.CommandPalette.Extensions.Toolkit [Infrastructure] (2)
      ParametersPage  → ParametersPage
      SettingsContentPage  → SettingsContentPage
   PowerToys.QuickAccess [Presentation] (2)
      LaunchPage  → LaunchPage
      AppsListPage  → AppsListPage

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
   PowerDisplay.Contracts
   PowerToys.ModuleContracts ── Common.UI
   WorkspacesCsharpLibrary
   Common.Search
   LanguageModelProvider ── ManagedCommon
   Microsoft.CmdPal.Ext.Apps ── ManagedCommon, ManagedCsWin32, Microsoft.CmdPal.Common
   Peek.Common ── ManagedCommon, ManagedTelemetry
   PowerAccent.Common
   PowerDisplay.Lib ── ManagedCommon, ManagedCsWin32, PowerDisplay.Models
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
   … and 73 more projects (use --focus for a scoped slice)

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
   CLI (4)
      BaseCommand —settings object  → BaseCommand  (src/dsc/v3/PowerToys.DSC/Commands/BaseCommand.cs:22)
      FancyZonesBaseCommand —settings object  → FancyZonesBaseCommand  (src/modules/fancyzones/FancyZonesCLI/CommandLine/Commands/FancyZonesBaseCommand.cs:16)
      ImageResizerRootCommand —settings object  → ImageResizerRootCommand  (src/modules/imageresizer/ui/Cli/Commands/ImageResizerRootCommand.cs:14)
      PowerDisplayRootCommand —settings object  → PowerDisplayRootCommand  (src/modules/powerdisplay/PowerDisplay.Cli/Commands/PowerDisplayRootCommand.cs:18)

PACKAGES
   ORM/Data:  Microsoft.Data.Sqlite 10.0.9
   Logging:  NLog 5.2.8, NLog.Extensions.Logging 5.3.8, NLog.Schema 5.2.8
   Testing:  Moq 4.18.4, MSTest, MSTest.TestFramework
   Other:  AdaptiveCards.ObjectModel.WinUI3 2.0.0-beta, AdaptiveCards.Rendering.WinUI3 2.1.0-beta, AdaptiveCards.Templating 2.0.5, Appium.WebDriver 4.4.5, CoenM.ImageSharp.ImageHash 1.3.6, CommunityToolkit.Common 8.4.0, CommunityToolkit.Labs.WinUI.Controls.MarkdownTextBlock 0.1.260116-build.2514, CommunityToolkit.Labs.WinUI.Controls.OpacityMaskView 0.1.251101-build.2372 … (93 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "Worker")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 3230ms |
| GenericExtraction | 23442ms |
| SignalSealing | 0ms |
| SpecificExtraction | 5149ms |
| Compression | 622ms |
| **Total** | **58857ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| ProgramCsFlowExtractor | 23439ms | 4155 | 103 |
| SyntaxStructureExtractor | 23430ms | 4155 | 102 |
| DiRegistrationExtractor | 23424ms | 49 | 102 |
| CallGraphExtractor | 4235ms | 0 | 0 |
| ProjectStructure | 2592ms | 0 | 0 |
| SourceBodyExtractor | 1717ms | 0 | 0 |
| BodyFactsExtractor | 875ms | 0 | 0 |
| CliCommandExtractor | 867ms | 0 | 300 |
| InMemoryEventBusExtractor | 594ms | 0 | 297 |
| DesktopEntryExtractor | 593ms | 0 | 297 |
| FileTreeExtractor | 439ms | 0 | 0 |
| IndirectWiringDetector | 257ms | 0 | 89 |
| SolutionDiscovery | 195ms | 0 | 0 |
| DependencyExtractor | 97ms | 0 | 0 |
| LayerClassifier | 21ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 4234 | 780 |
| Resolves | 165 | 93 |

_3800 files · 208 projects_
