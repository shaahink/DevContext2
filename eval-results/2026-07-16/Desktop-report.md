# REPORT
**AdvancedToDoList**

Style: Unknown
_6 projects  ·  25 UiEntry  ·  net10.0, net10.0-android + dapper + desktop-ui + serilog_

## Stats

| Metric | Value |
|--------|-------|
| Files | 274 |
| Projects | 29 |
| Nodes | 171 |
| Edges | 79 |
| Entries | 25 |
| With target | 25/25 |
| Deep spine (>=2) | 25/25 (100%) |
| Verified edges | 65% |
| Analyzed in | 6.6s |

## Top Flows

1. **[RelayCommand] EditCategoryViewModel.CancelAsync** → `EditCategoryViewModel` *(UiEntry)*
2. **[RelayCommand] EditCategoryViewModel.SaveAsync** → `EditCategoryViewModel` *(UiEntry)*
3. **[RelayCommand] EditToDoItemViewModel.AddNewCategoryAsync** → `EditToDoItemViewModel` *(UiEntry)*
4. **[RelayCommand] EditToDoItemViewModel.CancelAsync** → `EditToDoItemViewModel` *(UiEntry)*
5. **[RelayCommand] EditToDoItemViewModel.SaveAsync** → `EditToDoItemViewModel` *(UiEntry)*
6. **[RelayCommand] EditToDoItemViewModel.SetCategoryToEmpty** → `EditToDoItemViewModel` *(UiEntry)*
7. **[RelayCommand] ManageCategoriesViewModel.AddNewCategoryAsync** → `ManageCategoriesViewModel` *(UiEntry)*
8. **[RelayCommand] ManageCategoriesViewModel.DeleteCategoryAsync** → `ManageCategoriesViewModel` *(UiEntry)*
9. **[RelayCommand] ManageCategoriesViewModel.EditCategoryAsync** → `ManageCategoriesViewModel` *(UiEntry)*
10. **[RelayCommand] ManageCategoriesViewModel.RefreshAsync** → `ManageCategoriesViewModel` *(UiEntry)*

### Trace 1: [RelayCommand] EditCategoryViewModel.CancelAsync

TRACE  [RelayCommand] EditCategoryViewModel.CancelAsync
       src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:309
       AdvancedToDoList
▸ ENTRY  [RelayCommand] EditCategoryViewModel.CancelAsync  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:309)
   └─ call EditCategoryViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:309)
          /// <summary>
          /// ViewModel for editing category details in a dialog window.
          /// Handles form validation, save/cancel operations, and data persistence.

---

### Trace 2: [RelayCommand] EditCategoryViewModel.SaveAsync

TRACE  [RelayCommand] EditCategoryViewModel.SaveAsync
       src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:228
       AdvancedToDoList
▸ ENTRY  [RelayCommand] EditCategoryViewModel.SaveAsync  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:228)
   └─ call EditCategoryViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:228)
          /// <summary>
          /// ViewModel for editing category details in a dialog window.
          /// Handles form validation, save/cancel operations, and data persistence.

---

### Trace 3: [RelayCommand] EditToDoItemViewModel.AddNewCategoryAsync

TRACE  [RelayCommand] EditToDoItemViewModel.AddNewCategoryAsync
       src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditToDoItemViewModel.cs:82
       AdvancedToDoList
▸ ENTRY  [RelayCommand] EditToDoItemViewModel.AddNewCategoryAsync  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditToDoItemViewModel.cs:82)
   └─ call EditToDoItemViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditToDoItemViewModel.cs:82)
          /// <summary>
          /// ViewModel for editing ToDoItems in a dialog context.
          /// Provides UI interactions for modifying ToDoItem properties, managing categories,

---

## Insights

_5 info · 1 notable_

### **NOTABLE**: Most depended-upon: AdvancedToDoList (5 dependents) · TestableApp (3 dependents) · SharedControls (2 dependents)
*(Topology)*

- AdvancedToDoList (5 dependents)
- TestableApp (3 dependents)
- SharedControls (2 dependents)

### _INFO_: Command inventory: 2 ICommand implementations
*(Wiring)*

- Interaction
- DialogCommand

### _INFO_: Entry targets resolved 25/25 (100%) — trace any entry for its full path
*(Coverage)*

### _INFO_: Module map: 2 feature areas
*(Shape)*

- ViewModels (18 entries)
- Views (7 entries)

### _INFO_: DI: 11 Singleton (11 total)
*(Wiring)*

### _INFO_: Wiring hubs: IDialogService (6) · ManageToDoItemsViewModel (5)
*(Wiring)*

- IDialogService (6)
- ManageToDoItemsViewModel (5)

DESKTOP APP  AdvancedToDoList     (6 projects)

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

DESKTOP VIEW
   AdvancedToDoList [Infrastructure] (25)
      SettingsView  → SettingsView
      ManageToDoItemsView  → ManageToDoItemsView
      ManageCategoriesView  → ManageCategoriesView
      MainWindow  → MainWindow
      MainView  → MainView
      EditToDoItemView  → EditToDoItemView
      EditCategoryView  → EditCategoryView
      [RelayCommand] ToDoItemViewModel.SetProgressAsync  → ToDoItemViewModel
      [RelayCommand] SettingsViewModel.ClearDatabaseAsync  → SettingsViewModel
      [RelayCommand] SettingsViewModel.ImportDataAsync  → SettingsViewModel
      [RelayCommand] SettingsViewModel.ExportDataAsync  → SettingsViewModel
      [RelayCommand] ManageToDoItemsViewModel.RefreshAsync  → ManageToDoItemsViewModel
      [RelayCommand] ManageToDoItemsViewModel.EditToDoItemAsync  → ManageToDoItemsViewModel
      [RelayCommand] ManageToDoItemsViewModel.DeleteToDoItemAsync  → ManageToDoItemsViewModel
      [RelayCommand] ManageToDoItemsViewModel.AddNewToDoItem  → ManageToDoItemsViewModel
      [RelayCommand] ManageCategoriesViewModel.RefreshAsync  → ManageCategoriesViewModel
      [RelayCommand] ManageCategoriesViewModel.EditCategoryAsync  → ManageCategoriesViewModel
      [RelayCommand] ManageCategoriesViewModel.DeleteCategoryAsync  → ManageCategoriesViewModel
      [RelayCommand] ManageCategoriesViewModel.AddNewCategoryAsync  → ManageCategoriesViewModel
      [RelayCommand] EditToDoItemViewModel.CancelAsync  → EditToDoItemViewModel
      [RelayCommand] EditToDoItemViewModel.SaveAsync  → EditToDoItemViewModel
      [RelayCommand] EditToDoItemViewModel.SetCategoryToEmpty  → EditToDoItemViewModel
      [RelayCommand] EditToDoItemViewModel.AddNewCategoryAsync  → EditToDoItemViewModel
      [RelayCommand] EditCategoryViewModel.CancelAsync  → EditCategoryViewModel
      [RelayCommand] EditCategoryViewModel.SaveAsync  → EditCategoryViewModel

TOPOLOGY (depends-on)
   AdvancedToDoList ── SharedControls
   SharedControls
   AdvancedToDoList.Android ── AdvancedToDoList
   AdvancedToDoList.Browser ── AdvancedToDoList
   AdvancedToDoList.Desktop ── AdvancedToDoList
   AdvancedToDoList.iOS ── AdvancedToDoList

ENTRY POINTS
   UI (25)
      [RelayCommand] EditCategoryViewModel.CancelAsync  → EditCategoryViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:309)
      [RelayCommand] EditCategoryViewModel.SaveAsync  → EditCategoryViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditCategoryViewModel.cs:228)
      [RelayCommand] EditToDoItemViewModel.AddNewCategoryAsync  → EditToDoItemViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditToDoItemViewModel.cs:82)
      [RelayCommand] EditToDoItemViewModel.CancelAsync  → EditToDoItemViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditToDoItemViewModel.cs:150)
      [RelayCommand] EditToDoItemViewModel.SaveAsync  → EditToDoItemViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditToDoItemViewModel.cs:111)
      [RelayCommand] EditToDoItemViewModel.SetCategoryToEmpty  → EditToDoItemViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/EditToDoItemViewModel.cs:102)
      [RelayCommand] ManageCategoriesViewModel.AddNewCategoryAsync  → ManageCategoriesViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageCategoriesViewModel.cs:93)
      [RelayCommand] ManageCategoriesViewModel.DeleteCategoryAsync  → ManageCategoriesViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageCategoriesViewModel.cs:113)
      [RelayCommand] ManageCategoriesViewModel.EditCategoryAsync  → ManageCategoriesViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageCategoriesViewModel.cs:136)
      [RelayCommand] ManageCategoriesViewModel.RefreshAsync  → ManageCategoriesViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageCategoriesViewModel.cs:170)
      [RelayCommand] ManageToDoItemsViewModel.AddNewToDoItem  → ManageToDoItemsViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageToDoItemsViewModel.cs:161)
      [RelayCommand] ManageToDoItemsViewModel.DeleteToDoItemAsync  → ManageToDoItemsViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageToDoItemsViewModel.cs:186)
      [RelayCommand] ManageToDoItemsViewModel.EditToDoItemAsync  → ManageToDoItemsViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageToDoItemsViewModel.cs:210)
      [RelayCommand] ManageToDoItemsViewModel.RefreshAsync  → ManageToDoItemsViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ManageToDoItemsViewModel.cs:239)
      [RelayCommand] SettingsViewModel.ClearDatabaseAsync  → SettingsViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/SettingsViewModel.cs:147)
      [RelayCommand] SettingsViewModel.ExportDataAsync  → SettingsViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/SettingsViewModel.cs:44)
      [RelayCommand] SettingsViewModel.ImportDataAsync  → SettingsViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/SettingsViewModel.cs:91)
      [RelayCommand] ToDoItemViewModel.SetProgressAsync  → ToDoItemViewModel  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/ViewModels/ToDoItemViewModel.cs:159)
      EditCategoryView  → EditCategoryView  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/Views/EditCategoryView.axaml.cs:11)
      EditToDoItemView  → EditToDoItemView  (src/Avalonia.Samples/CompleteApps/AdvancedToDoList/AdvancedToDoList/Views/EditToDoItemView.axaml.cs:11)
      … and 5 more (ui entries — use --focus for a drill-in)

PACKAGES
   ORM/Data:  Dapper 2.1.79, Dapper.AOT 1.0.52, Microsoft.Data.Sqlite 10.0.9
   Logging:  Serilog 4.3.1, Serilog.Sinks.File 7.0.0, SerilogTraceListener 3.2.0
   Testing:  Avalonia.Headless.NUnit 12.1.0, Avalonia.Headless.XUnit 12.1.0, coverlet.collector 10.0.1, NUnit 4.6.1, NUnit.Analyzers 4.14.0, NUnit3TestAdapter 6.2.0, xunit.runner.visualstudio 3.1.5, xunit.v3 3.2.2
   Other:  Appium.WebDriver 8.3.1, Avalonia 12.1.0, Avalonia.Android 12.1.0, Avalonia.Browser 12.1.0, Avalonia.Controls.ColorPicker 12.1.0, Avalonia.Desktop 12.1.0, Avalonia.Fonts.Inter 12.1.0, Avalonia.iOS 12.1.0 … (18 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "SettingsView")
## Run Report

### Stages

| Stage | Time |
|-------|------|
| DiscoveryAndCacheWarmup | 908ms |
| GenericExtraction | 1702ms |
| SignalSealing | 0ms |
| SpecificExtraction | 256ms |
| Compression | 71ms |
| **Total** | **6556ms** |

### Extractors

| Name | Time | +Types | +Dets |
|------|------|--------|-------|
| ProgramCsFlowExtractor | 1684ms | 267 | 11 |
| SyntaxStructureExtractor | 1683ms | 267 | 11 |
| DiRegistrationExtractor | 1663ms | 0 | 11 |
| ProjectStructure | 685ms | 0 | 0 |
| CallGraphExtractor | 183ms | 0 | 0 |
| SourceBodyExtractor | 181ms | 0 | 0 |
| FileTreeExtractor | 137ms | 0 | 0 |
| SolutionDiscovery | 81ms | 0 | 0 |
| DesktopEntryExtractor | 65ms | 0 | 79 |
| BodyFactsExtractor | 62ms | 0 | 0 |
| InMemoryEventBusExtractor | 58ms | 0 | 79 |
| DependencyExtractor | 26ms | 0 | 0 |
| LayerClassifier | 26ms | 0 | 0 |
| IndirectWiringDetector | 22ms | 0 | 18 |
| AntiPatternDetector | 0ms | 0 | 0 |

### Graph Seams

| Seam | Edges | Approx |
|------|-------|--------|
| Calls | 68 | 26 |
| Resolves | 11 | 2 |

_274 files · 29 projects_
