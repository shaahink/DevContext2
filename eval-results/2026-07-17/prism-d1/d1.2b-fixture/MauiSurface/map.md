MAP  unknown     (1 project)

STACK  net10.0-android;net10.0-ios;net10.0-maccatalyst

STYLE  Unknown  (confidence low)
       evidence: ArchitectureStyleDetector

       per service:
         MauiSurface: MAUI App [.NET MAUI]

TOPOLOGY (depends-on)
   MauiSurface

ENTRY POINTS
   UI (4)
      [RelayCommand] DiscoverViewModel.OpenShow  → DiscoverViewModel  (App/ViewModels/DiscoverViewModel.cs:11)
      AppShell  → AppShell  (App/AppShell.xaml.cs:5)
      DiscoverPage  → DiscoverPage  (App/Pages/DiscoverPage.xaml.cs:5)
      PlayerPage  → PlayerPage  (App/Pages/PlayerPage.xaml.cs:4)

→ drill in:  --focus "<entry>"   (e.g. --focus "[RelayCommand] DiscoverViewModel.OpenShow")
