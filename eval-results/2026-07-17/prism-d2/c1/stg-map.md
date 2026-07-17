DESKTOP APP  GifRecorder     (6 projects)

STACK  net8.0-windows, net9.0-windows7.0

STYLE  DesktopMvvm  (confidence moderate)
       evidence: 5 WPF/WinForms project(s) + 20 ViewModels

       per service:
         ScreenToGif: Desktop (MVVM) [WPF/WinForms]
         Translator: Desktop [WPF/WinForms]

DESKTOP VIEW
   ScreenToGif [Presentation] (32)
      ExportPanel  → ExportPanel
      PluginSettings  → PluginSettingsViewModel
      StorageSettings  → StorageSettings
      TasksSettings  → TasksSettings
      DonateSettings  → DonateSettings
      UploadSettings  → UploadSettings
      EditorSettings  → EditorSettings
      ApplicationSettings  → ApplicationSettings
      ImageViewer  → ImageViewer
      KGySoftGifOptionsPanel  → KGySoftGifOptionsPanel
      RegionSelection  → RegionSelection
      LanguageSettings  → LanguageSettings
      TestField  → TestField
      RegionSelector  → RegionSelector
      ShortcutsSettings  → ShortcutsSettings
      ImgurPanel  → ImgurPanel
      AboutSettings  → AboutSettings
      YandexPanel  → YandexPanel
      WebcamControl  → WebcamControl
      ShadowPanel  → ShadowPanel
      ProgressPanel  → ProgressPanel
      Splash  → Splash
      RegionMagnifier  → RegionMagnifier
      RecorderSettings  → RecorderSettings
      ResizePanel  → ResizePanel
      MouseEventsPanel  → MouseEventsPanel
      KeyStrokesPanel  → KeyStrokesPanel
      DelayPanel  → DelayPanel
      BorderPanel  → BorderPanel
      ExWindow  → ExWindow
      BaseWindow  → BaseWindow
      BaseRecorder  → BaseRecorder
   Translator (3)
      TranslatorWindow  → TranslatorWindow
      ExceptionDialog  → ExceptionDialog
      Dialog  → Dialog

TOPOLOGY (depends-on)
   ScreenToGif.Domain
   ScreenToGif.Native ── ScreenToGif.Domain
   ScreenToGif.Util ── ScreenToGif.Domain, ScreenToGif.Native
   ScreenToGif.ViewModel ── ScreenToGif.Domain, ScreenToGif.Util
   ScreenToGif ── ScreenToGif.Native, ScreenToGif.ViewModel
   Translator

ENTRY POINTS
   UI (35)
      AboutSettings  → AboutSettings  (ScreenToGif/Views/Settings/AboutSettings.xaml.cs:9)
      ApplicationSettings  → ApplicationSettings  (ScreenToGif/Views/Settings/ApplicationSettings.xaml.cs:14)
      BaseRecorder  → BaseRecorder  (ScreenToGif/Controls/BaseRecorder.cs:12)
      BaseWindow  → BaseWindow  (ScreenToGif/Controls/BaseWindow.cs:6)
      BorderPanel  → BorderPanel  (ScreenToGif/UserControls/BorderPanel.xaml.cs:5)
      DelayPanel  → DelayPanel  (ScreenToGif/UserControls/DelayPanel.xaml.cs:5)
      Dialog  → Dialog  (Other/Translator/Dialog.xaml.cs:10)
      DonateSettings  → DonateSettings  (ScreenToGif/Views/Settings/DonateSettings.xaml.cs:9)
      EditorSettings  → EditorSettings  (ScreenToGif/Views/Settings/EditorSettings.xaml.cs:11)
      ExceptionDialog  → ExceptionDialog  (Other/Translator/ExceptionDialog.xaml.cs:9)
      ExportPanel  → ExportPanel  (ScreenToGif/UserControls/ExportPanel.xaml.cs:42)
      ExWindow  → ExWindow  (ScreenToGif/Controls/ExWindow.cs:15)
      ImageViewer  → ImageViewer  (ScreenToGif/UserControls/ImageViewer.xaml.cs:33)
      ImgurPanel  → ImgurPanel  (ScreenToGif/UserControls/ImgurPanel.xaml.cs:17)
      KeyStrokesPanel  → KeyStrokesPanel  (ScreenToGif/UserControls/KeyStrokesPanel.xaml.cs:5)
      KGySoftGifOptionsPanel  → KGySoftGifOptionsPanel  (ScreenToGif/UserControls/KGySoftGifOptionsPanel.xaml.cs:16)
      LanguageSettings  → LanguageSettings  (ScreenToGif/Views/Settings/LanguageSettings.xaml.cs:15)
      MouseEventsPanel  → MouseEventsPanel  (ScreenToGif/UserControls/MouseClicksPanel.xaml.cs:5)
      PluginSettings  → PluginSettingsViewModel  (ScreenToGif/Views/Settings/PluginSettings.xaml.cs:16)
      ProgressPanel  → ProgressPanel  (ScreenToGif/UserControls/ProgressPanel.xaml.cs:7)
      … and 15 more (ui entries — use --focus for a drill-in)

PACKAGES
   Testing:  coverlet.collector 3.1.2, xunit 2.4.1, xunit.runner.visualstudio 2.4.3
   Other:  KGySoft.CoreLibraries 10.5.0, KGySoft.Drawing 10.0.1, KGySoft.Drawing.Core 10.0.1, KGySoft.Drawing.Wpf 10.0.1, Microsoft.CSharp 4.7.0, Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers 0.4.421302, Microsoft.NET.Test.Sdk 17.1.0, Microsoft.Windows.Compatibility 8.0.0 … (12 total)

→ drill in:  --focus "<entry>"   (e.g. --focus "ExportPanel")
