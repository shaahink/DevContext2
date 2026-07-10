# REPORT
**Avalonia.Samples**

Style: Unknown
_18 projects  ·  31 UiEntry  ·  net10.0, net10.0-android + dapper + desktop-ui + serilog_

## Stats

| Metric | Value |
|--------|-------|
| Files | 274 |
| Projects | 29 |
| Nodes | 197 |
| Edges | 36 |
| Entries | 31 |
| With target | 31/31 |
| Verified edges | 86% |
| Analyzed in | 2.8s |

## Top Flows

1. **[RelayCommand] App.ExitApplication** → `App` *(UiEntry)*
2. **[RelayCommand] App.ShowAboutWindow** → `App` *(UiEntry)*
3. **[RelayCommand] App.TrayIconClicked** → `App` *(UiEntry)*
4. **[RelayCommand] CommunityToolkitCommandsViewModel.OpenThePodBayDoorsAsync** → `CommunityToolkitCommandsViewModel` *(UiEntry)*
5. **[RelayCommand] CommunityToolkitCommandsViewModel.OpenThePodBayDoorsDirect** → `CommunityToolkitCommandsViewModel` *(UiEntry)*
6. **[RelayCommand] CommunityToolkitCommandsViewModel.OpenThePodBayDoorsFellowRobot** → `CommunityToolkitCommandsViewModel` *(UiEntry)*
7. **[RelayCommand] CustomInteractionViewModel.SelectFilesAsync** → `CustomInteractionViewModel` *(UiEntry)*
8. **[RelayCommand] InputDialogViewModel.Cancel** → `InputDialogViewModel` *(UiEntry)*
9. **[RelayCommand] InputDialogViewModel.ReturnResult** → `InputDialogViewModel` *(UiEntry)*
10. **[RelayCommand] MainViewModel.AddAlbumAsync** → `MainViewModel` *(UiEntry)*

### Trace 1: [RelayCommand] App.ExitApplication

TRACE  [RelayCommand] App.ExitApplication
       src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:51

▸ ENTRY  [RelayCommand] App.ExitApplication  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:51)
   └─ call App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:51)
          public partial class App : Application
          public override void Initialize()
          AvaloniaXamlLoader.Load(this);

---

### Trace 2: [RelayCommand] App.ShowAboutWindow

TRACE  [RelayCommand] App.ShowAboutWindow
       src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:41

▸ ENTRY  [RelayCommand] App.ShowAboutWindow  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:41)
   └─ call App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:41)
          public partial class App : Application
          public override void Initialize()
          AvaloniaXamlLoader.Load(this);

---

### Trace 3: [RelayCommand] App.TrayIconClicked

TRACE  [RelayCommand] App.TrayIconClicked
       src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:31

▸ ENTRY  [RelayCommand] App.TrayIconClicked  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:31)
   └─ call App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:31)
          public partial class App : Application
          public override void Initialize()
          AvaloniaXamlLoader.Load(this);

---

## Insights

_4 info · 3 notable_

### **NOTABLE**: ViewModel-View: 41 VMs + 37 Views (0 call edges)
*(Wiring)*

- 41 ViewModels
- 37 Views

### **NOTABLE**: Most depended-upon: AdvancedToDoList (5 dependents) · TestableApp (3 dependents) · SharedControls (2 dependents)
*(Topology)*

- AdvancedToDoList (5 dependents)
- TestableApp (3 dependents)
- SharedControls (2 dependents)

### **NOTABLE**: Event flow: 2 published, 0 consumed, 2 orphan
*(Wiring)*

- 2 orphan events (published, no internal consumer)
- Avalonia.MusicStore.Messages.PurchaseAlbumMessage ← MainWindow.MainWindow
- Avalonia.MusicStore.Messages.CheckAlbumAlreadyExistsMessage ← MusicStoreViewModel.BuyMusic
- 0/2 events consumed

### _INFO_: Entry targets resolved 31/31 (100%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Command inventory: 2 ICommand implementations
*(Wiring)*

- Interaction
- DialogCommand

### _INFO_: Module map: 3 feature areas
*(Shape)*

- ViewModels (15 entries)
- Views (12 entries)
- TrayIcon (4 entries)

### _INFO_: Public surface: 8 interfaces, 226 classes (238 total public types)
*(Shape)*

- 8 interfaces
- 226 classes

MAP  Avalonia.Samples     (18 projects)

STACK  net10.0, net10.0-android, net10.0-browser, net10.0-ios

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         Avalonia.MusicStore: Unknown
         SimpleToDoList: Unknown
         RatingControlSample: Unknown
         SnowflakesControlSample: Unknown
         BasicDataTemplateSample: Unknown
         FuncDataTemplateSample: Unknown
         IDataTemplateSample: Unknown
         TrayIcon: Unknown
         BattleCity: Unknown
         RectPainter: Unknown
         BasicMvvmSample: Unknown
         CommandSample: Unknown
         ValidationSample: Unknown
         ValueConversionSample: Unknown
         BasicViewLocatorSample: Unknown
         TestableApp: Unknown
         DialogManagerSample: Unknown
         MvvmDialogSample: Unknown
         AdvancedToDoList.Android: Unknown
         AdvancedToDoList.Browser: Unknown
         AdvancedToDoList.Desktop: Unknown
         AdvancedToDoList.iOS: Unknown

TOPOLOGY (depends-on)
   Avalonia.MusicStore
   BasicDataTemplateSample
   BasicMvvmSample
   BasicViewLocatorSample
   BattleCity
   CommandSample
   DialogManagerSample
   FuncDataTemplateSample
   IDataTemplateSample
   MvvmDialogSample
   RatingControlSample
   RectPainter
   SimpleToDoList
   SnowflakesControlSample
   TestableApp
   TrayIcon
   ValidationSample
   ValueConversionSample

ENTRY POINTS
   UI (31)
      [RelayCommand] App.ExitApplication  → App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:51)
      [RelayCommand] App.ShowAboutWindow  → App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:41)
      [RelayCommand] App.TrayIconClicked  → App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:31)
      [RelayCommand] CommunityToolkitCommandsViewModel.OpenThePodBayDoorsAsync  → CommunityToolkitCommandsViewModel  (src/Avalonia.Samples/MVVM/CommandSample/ViewModels/CommunityToolkitCommandsViewModel.cs:66)
      [RelayCommand] CommunityToolkitCommandsViewModel.OpenThePodBayDoorsDirect  → CommunityToolkitCommandsViewModel  (src/Avalonia.Samples/MVVM/CommandSample/ViewModels/CommunityToolkitCommandsViewModel.cs:23)
      [RelayCommand] CommunityToolkitCommandsViewModel.OpenThePodBayDoorsFellowRobot  → CommunityToolkitCommandsViewModel  (src/Avalonia.Samples/MVVM/CommandSample/ViewModels/CommunityToolkitCommandsViewModel.cs:39)
      [RelayCommand] CustomInteractionViewModel.SelectFilesAsync  → CustomInteractionViewModel  (src/Avalonia.Samples/ViewInteraction/MvvmDialogSample/ViewModels/CustomInteractionViewModel.cs:27)
      [RelayCommand] InputDialogViewModel.Cancel  → InputDialogViewModel  (src/Avalonia.Samples/ViewInteraction/DialogManagerSample/ViewModels/InputDialogViewModel.cs:49)
      [RelayCommand] InputDialogViewModel.ReturnResult  → InputDialogViewModel  (src/Avalonia.Samples/ViewInteraction/DialogManagerSample/ViewModels/InputDialogViewModel.cs:38)
      [RelayCommand] MainViewModel.AddAlbumAsync  → MainViewModel  (src/Avalonia.Samples/CompleteApps/Avalonia.MusicStore/ViewModels/MainViewModel.cs:32)
      [RelayCommand] MainViewModel.AddItem  → MainViewModel  (src/Avalonia.Samples/CompleteApps/SimpleToDoList/ViewModels/MainViewModel.cs:39)
      [RelayCommand] MainViewModel.RemoveItem  → MainViewModel  (src/Avalonia.Samples/CompleteApps/SimpleToDoList/ViewModels/MainViewModel.cs:67)
      [RelayCommand] MainWindowViewModel.AskForUsernameAsync  → MainWindowViewModel  (src/Avalonia.Samples/ViewInteraction/DialogManagerSample/ViewModels/MainWindowViewModel.cs:41)
      [RelayCommand] MainWindowViewModel.SelectFilesAsync  → MainWindowViewModel  (src/Avalonia.Samples/ViewInteraction/DialogManagerSample/ViewModels/MainWindowViewModel.cs:21)
      [RelayCommand] MainWindowViewModel.ShowError  → MainWindowViewModel  (src/Avalonia.Samples/ViewInteraction/DialogManagerSample/ViewModels/MainWindowViewModel.cs:71)
      [RelayCommand] MainWindowViewModel.ShowInformation  → MainWindowViewModel  (src/Avalonia.Samples/ViewInteraction/DialogManagerSample/ViewModels/MainWindowViewModel.cs:62)
      [RelayCommand] MusicStoreViewModel.BuyMusic  → MusicStoreViewModel  (src/Avalonia.Samples/CompleteApps/Avalonia.MusicStore/ViewModels/MusicStoreViewModel.cs:32)
      [RelayCommand] SnowflakeGameViewModel.StartGame  → SnowflakeGameViewModel  (src/Avalonia.Samples/CustomControls/SnowflakesControlSample/ViewModels/SnowflakeGameViewModel.cs:77)
      AboutWindow  → AboutWindow  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/AboutWindow.axaml.cs:6)
      AlbumView  → AlbumView  (src/Avalonia.Samples/CompleteApps/Avalonia.MusicStore/Views/AlbumView.axaml.cs:5)
      … and 11 more (ui entries — use --focus for a drill-in)

PACKAGES
   ORM/Data:  Dapper 2.1.79, Dapper.AOT 1.0.52, Microsoft.Data.Sqlite 10.0.9
   Logging:  Serilog 4.3.1, Serilog.Sinks.File 7.0.0, SerilogTraceListener 3.2.0
   Testing:  Avalonia.Headless.NUnit 12.0.5, Avalonia.Headless.XUnit 12.0.5, coverlet.collector 10.0.1, NUnit 4.6.1, NUnit.Analyzers 4.14.0, NUnit3TestAdapter 6.2.0, xunit.runner.visualstudio 3.1.5, xunit.v3 3.2.2
   Other:  Appium.WebDriver 8.3.0, Avalonia 12.0.5, Avalonia.Android 12.0.5, Avalonia.Browser 12.0.5, Avalonia.Controls.ColorPicker 12.0.5, Avalonia.Desktop 12.0.5, Avalonia.Fonts.Inter 12.0.5, Avalonia.iOS 12.0.5 … (18 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "POST /api/orders/" or --focus <TypeName>)
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 647ms |
| GenericExtraction | 1399ms |
| SignalSealing | 0ms |
| SpecificExtraction | 206ms |
| Compression | 45ms |
| **Total** | **2811ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 1395ms | 267 | 11 |
| DiRegistrationExtractor | 1393ms | 1 | 11 |
| ProgramCsFlowExtractor | 487ms | 0 | 0 |
| ProjectStructure | 469ms | 0 | 0 |
| CallGraphExtractor | 146ms | 0 | 0 |
| SourceBodyExtractor | 136ms | 0 | 0 |
| FileTreeExtractor | 119ms | 0 | 0 |
| SolutionDiscovery | 54ms | 0 | 0 |
| DesktopEntryExtractor | 53ms | 0 | 79 |
| InMemoryEventBusExtractor | 46ms | 0 | 79 |
| DependencyExtractor | 27ms | 0 | 0 |
| LayerClassifier | 27ms | 0 | 0 |
| IndirectWiringDetector | 13ms | 0 | 2 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 31 | 0 |
| Sends | 2 | 2 |
| Raises | 3 | 3 |

_274 files · 29 projects_
