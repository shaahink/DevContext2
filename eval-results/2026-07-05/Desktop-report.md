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
| Edges | 35 |
| Entries | 31 |
| With target | 30/31 |
| Verified edges | 86% |
| Analyzed in | 2.6s |

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
          public class App : Application
          public override void Initialize()
          AvaloniaXamlLoader.Load(this);

---

### Trace 2: [RelayCommand] App.ShowAboutWindow

TRACE  [RelayCommand] App.ShowAboutWindow
       src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:41

▸ ENTRY  [RelayCommand] App.ShowAboutWindow  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:41)
   └─ call App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:41)
          public class App : Application
          public override void Initialize()
          AvaloniaXamlLoader.Load(this);

---

### Trace 3: [RelayCommand] App.TrayIconClicked

TRACE  [RelayCommand] App.TrayIconClicked
       src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:31

▸ ENTRY  [RelayCommand] App.TrayIconClicked  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:31)
   └─ call App  (src/Avalonia.Samples/DesktopIntegration/TrayIcon/App.axaml.cs:31)
          public class App : Application
          public override void Initialize()
          AvaloniaXamlLoader.Load(this);

---

## Insights

_4 info · 3 notable_

### **NOTABLE**: ViewModel-View: 41 VMs + 37 Views (0 call edges)
*(Wiring)*

- 41 ViewModels
- 37 Views

### **NOTABLE**: Possible dead code: 5 public types with zero inbound references
*(Wiring)*

- App
- Snowflake
- Teacher
- MainWindow
- MainWindow

### **NOTABLE**: Most depended-upon: AdvancedToDoList (5 dependents) · TestableApp (3 dependents) · SharedControls (2 dependents)
*(Topology)*

- AdvancedToDoList (5 dependents)
- TestableApp (3 dependents)
- SharedControls (2 dependents)

### _INFO_: Entry targets resolved 30/31 (96%) — use --focus for deeper traces
*(Coverage)*

### _INFO_: Command inventory: 2 ICommand implementations
*(Wiring)*

- Interaction
- DialogCommand

### _INFO_: Module map: 8 feature areas
*(Shape)*

- Views (10 entries)
- ViewModels (8 entries)
- ValidationSample/ViewModels (4 entries)
- CommandSample/ViewModels (3 entries)
- BattleCity (3 entries)

### _INFO_: Public surface: 8 interfaces, 226 classes (238 total public types)
*(Shape)*

- 8 interfaces
- 226 classes

MAP  Avalonia.Samples     (18 projects)

STACK  net10.0, net10.0-android, net10.0-browser, net10.0-ios

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

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
| DiscoveryAndCacheWarmup | 584ms |
| GenericExtraction | 1397ms |
| SignalSealing | 0ms |
| SpecificExtraction | 158ms |
| Compression | 47ms |
| **Total** | **2564ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| SyntaxStructureExtractor | 1392ms | 267 | 11 |
| DiRegistrationExtractor | 1389ms | 0 | 11 |
| ProgramCsFlowExtractor | 497ms | 0 | 0 |
| ProjectStructure | 470ms | 0 | 0 |
| CallGraphExtractor | 116ms | 0 | 0 |
| SourceBodyExtractor | 101ms | 0 | 0 |
| FileTreeExtractor | 71ms | 0 | 0 |
| InMemoryEventBusExtractor | 39ms | 0 | 79 |
| SolutionDiscovery | 39ms | 0 | 0 |
| DesktopEntryExtractor | 39ms | 0 | 79 |
| DependencyExtractor | 22ms | 0 | 0 |
| LayerClassifier | 20ms | 0 | 0 |
| IndirectWiringDetector | 12ms | 0 | 9 |
| AntiPatternDetector | 0ms | 0 | 0 |
| AspireExtractor | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 30 | 0 |
| Sends | 2 | 2 |
| Raises | 3 | 3 |

_274 files · 0 projects_
