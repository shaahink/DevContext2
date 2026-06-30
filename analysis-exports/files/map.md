LIBRARY  Files     (127 public types)

ENTRY API
   implement IAsyncInitialize   (IAsyncInitialize.cs)
      Allows an object to be initialized asynchronously.
   implement IOmnibarTextMemberPathProvider   (IOmnibarTextMemberPathProvider.cs)
      An interface that provides a way to get the text member path of .
   implement IToolbarItemSet   (IToolbarItemSet.cs)
      Interface to scope allowed items in the Private Item list
   implement IWindowsStorable   (IWindowsStorable.cs)
   extend    ArrayExtensions   (ArrayExtensions.cs)
   extend    ComponentModelExtensions   (ComponentModelExtensions.cs)
   extend    DateExtensions   (DateExtensions.cs)
   extend    EnumerableExtensions   (EnumerableExtensions.cs)
   extend    FileLoggerExtensions   (FileLoggerExtensions.cs)
   extend    GenericExtensions   (GenericExtensions.cs)
   extend    GlobalHelper   (GlobalHelper.cs)
   extend    LinqExtensions   (LinqExtensions.cs)

ABSTRACTIONS
   IToolbarItemSet (interface)  — 6 implementors
   IOmnibarTextMemberPathProvider (interface)  — 4 implementors
   IAsyncInitialize (interface)  — 3 implementors
   IWindowsStorable (interface)  — 3 implementors
   FtpStorable (class)  — 2 implementors
   ISidebarItemModel (interface)  — 2 implementors
   IStorageService (interface)  — 2 implementors
   WindowsStorable (class)  — 2 implementors
   IDirectCopy (interface)  — 1 implementor
   IDirectMove (interface)  — 1 implementor

PUBLIC SURFACE
   Extras
      IDCompositionTarget (interface):  SetRoot
   Files.App.BackgroundTasks
      UpdateTask (class):  Run
   Files.App.Controls
      AdaptiveGridView (class):  AdaptiveGridView
         The AdaptiveGridView control allows to present information within a Grid View perfectly adjusting the total display a...
      BladeItem (class):  BladeItem, SetWidth
         The Blade is used as a child in the BladeView
      BladeItemAutomationPeer (class):  BladeItemAutomationPeer
         Defines a framework element automation peer for the .
      BladeView (class):  BladeView, ScrollToEnd
         A container that hosts controls in a horizontal scrolling list Based on the Azure portal UI
      BladeViewAutomationPeer (class):  BladeViewAutomationPeer
         Defines a framework element automation peer for the control.
      BreadcrumbBar (class):  BreadcrumbBar
      BreadcrumbBarItem (class):  BreadcrumbBarItem, OnIsChevronVisibleChanged, OnIsEllipsisChanged, OnItemClicked, SetOwner
      BreadcrumbBarItemAutomationPeer (class):  BreadcrumbBarItemAutomationPeer, Invoke
      BreadcrumbBarItemClickedEventArgs (record)
      BreadcrumbBarItemDropDownFlyoutEventArgs (record)
      BreadcrumbBarLayout (class):  BreadcrumbBarLayout
         Handles layout of , collapsing items into an ellipsis button when necessary.
      FlatSidebarItem (class):  FlatSidebarItem
         Per-row wrapper used by the sidebar's virtualized ItemsRepeater.
      GlobalHelper (class):  ChangeCursor, WriteDebugStringForOmnibar
      GridSplitter (class):  GridSplitter
         Represents the control that redistributes space between columns or rows of a Grid control.
      IOmnibarTextMemberPathProvider (interface):  GetTextMemberPath
         An interface that provides a way to get the text member path of .
      ISidebarItemModel (interface)
      IToolbarItemSet (interface)
         Interface to scope allowed items in the Private Item list
      IToolbarOverflowItemSet (interface)
         Interface to scope allowed items in the Private Overflow list
      ItemContextInvokedArgs (record)
      ItemDragOverEventArgs (record)
      ItemDroppedEventArgs (record)
      ItemInvokedEventArgs (record)
      Omnibar (class):  ChooseSuggestionItem, Omnibar, OnCurrentSelectedModeNameChanged, OnCurrentSelectedModePropertyChanged, OnIsFocusedChanged, PopulateModes
      OmnibarIsFocusedChangedEventArgs (record)
      OmnibarMode (class):  OmnibarMode, OnTextChanged, SetOwner, ToString
      OmnibarModeChangedEventArgs (record)
      OmnibarModeSeparator (class):  OmnibarModeSeparator
      OmnibarQuerySubmittedEventArgs (record)
      OmnibarSuggestionChosenEventArgs (record)
      OmnibarTextChangedEventArgs (record)
      SamplePanel (class):  OnSideContentChanged, SamplePanel
      SidebarItem (class):  HandleItemChange, OnPropertyChanged, SidebarItem
      SidebarItemAutomationPeer (class):  AddToSelection, Collapse, Expand, Invoke, RemoveFromSelection, Select, SidebarItemAutomationPeer
         Exposes types to Microsoft UI Automation.
      SidebarView (class):  OnPropertyChanged, ScrollToVerticalOffset, SidebarView
      SidebarViewAutomationPeer (class):  GetSelection, SidebarViewAutomationPeer
         Exposes types to Microsoft UI Automation.
      StorageBar (class):  OnBarShapeChanged, OnPercentCautionChanged, OnPercentChanged, OnPercentCriticalChanged, OnTrackBarHeightChanged, OnValueBarHeightChanged, StorageBar
      StorageControlsHelpers (class):  CalculateInterpolatedValue, CalculateModulus, DoubleToPercentage, EaseOutCubic, EasingInOutFunction, GapThicknessToAngle, GetAdjustedAngle, GetInterpolatedAngle, GetThicknessTransition, IsFullCircle, PercentageToValue
         Helpers for and .
      StorageRing (class):  OnMaxAngleChanged, OnMinAngleChanged, OnPercentCautionChanged, OnPercentChanged, OnPercentCriticalChanged, OnStartAngleChanged, OnTrackRingThicknessChanged, OnValueRingThicknessChanged, StorageRing
      ThemedIcon (class):  OnColorChanged, OnFilledIconDataChanged, OnIconColorTypeChanged, OnIconSizeChanged, OnIconTypeChanged, OnIsFilledChanged, OnIsHighContrastChanged, OnIsToggledChanged, OnLayersChanged, OnOutlineIconDataChanged, OnToggleBehaviorChanged, ThemedIcon
      ThemedIconLayer (class):  OnIconColorTypeChanged, OnLayerColorChanged, OnLayerSizeChanged, OnLayerTypeChanged, OnPathDataChanged, ThemedIconLayer
      ThemedIconLayers (class)
         A collection of Layers for the ThemedIcon's Layered IconType
      ThemedIconMarkup (class)
      Toolbar (class):  Toolbar
      ToolbarButton (class):  ToolbarButton
      ToolbarFlyoutButton (class):  ToolbarFlyoutButton
      ToolbarItem (class)
         An Abstract control to simplify items added to the Toolbar, and map them to other controls dependent on their overflo...
      ToolbarRadioButton (class):  ToolbarRadioButton
      ToolbarSeparator (class):  ToolbarSeparator
      ToolbarSplitButton (class):  ToolbarSplitButton
      ToolbarToggleButton (class):  ToolbarToggleButton
   Files.App.Controls.Primitives
      RingShape (class):  BeginUpdate, EndUpdate, OnEndAngleChanged, OnIsCircleChanged, OnMaxAngleChanged, OnMinAngleChanged, OnRadiusHeightChanged, OnRadiusWidthChanged, OnStartAngleChanged, OnSweepDirectionChanged, RingShape, UpdateSizeAndStroke
         Represents primitive Path shape for drawing a circular or elliptical ring.
      ToolbarLayout (class):  ToolbarLayout
   Files.App.Storage
      FtpManager (class)
      FtpStorable (class):  GetParentAsync
      FtpStorageFile (class):  FtpStorageFile, OpenStreamAsync
      FtpStorageFolder (class):  CreateCopyOfAsync, CreateFileAsync, CreateFolderAsync, DeleteAsync, FtpStorageFolder, GetFirstByNameAsync, GetFolderWatcherAsync, GetItemsAsync, MoveFromAsync
      FtpStorageService (class):  GetFileAsync, GetFolderAsync
      IWindowsFile (interface)
      IWindowsFolder (interface)
      IWindowsStorable (interface)
      JumpListItem (class)
      JumpListManager (class):  Dispose, GetAutomaticDestinations, GetCustomDestinations, JumpListManager
      STATask (class):  Run
         Represents a work scheduled to execute on a STA thread.
      SystemTrayManager (class):  CreateIcon, DeleteIcon, Dispose
         Exposes a manager to create or delete an system tray icon you provide.
      TaskbarManager (class):  Dispose, SetProgressState, SetProgressValue, TaskbarManager
      WindowsBulkOperations (class):  Dispose, PerformAllOperations, QueueCopyOperation, QueueCreateOperation, QueueDeleteOperation, QueueMoveOperation, QueueRenameOperation, WindowsBulkOperations
         Handles bulk file operations in Windows, such as copy, move, delete, create, and rename, supporting progress tracking...
      WindowsBulkOperationsEventArgs (class):  ToString
      WindowsContextMenuItem (class)
         Represents a Windows Shell ContextMenu item.
      WindowsDriveManager (class):  Dispose, Start, Stop
      WindowsFile (class):  OpenStreamAsync, WindowsFile
      WindowsFolder (class):  Dispose, GetItemsAsync, WindowsFolder
      WindowsStorable (class):  Dispose, Equals, GetHashCode, GetParentAsync, ToString, TryParse
      WindowsStorableHelpers (class):  GetDisplayName, GetPropertyValue, GetShellNewItems, GetThumbnailAsync, HasShellAttributes, InvokeShellNewItem, IsOnArmProcessor, TryConvertGpBitmapToByteArray, TryExtractImageFromDll, TryGetFileAttributes, TryGetShellTooltip, TryGetThumbnail, TryInvokeContextMenuVerb, TryInvokeContextMenuVerbs, TryPinFolderToQuickAccess
   Files.App.Storage.Storables
      HomeFolder (class):  GetItemsAsync, GetLogicalDrivesAsync, GetNetworkLocationsAsync, GetQuickAccessFolderAsync, GetRecentFilesAsync
      IHomeFolder (interface):  GetLogicalDrivesAsync, GetNetworkLocationsAsync, GetQuickAccessFolderAsync, GetRecentFilesAsync
      NativeStorageLegacyService (class):  GetFileAsync, GetFolderAsync
   Files.Core.Storage
      IFtpStorageService (interface)
         Provides an abstract layer for accessing an ftp file system
      IStorageService (interface):  GetFileAsync, GetFolderAsync
         Provides an abstract layer for accessing the file system.
   Files.Core.Storage.Contracts
      ITrashWatcher (interface)
      IWatcher (interface):  StartWatcher, StopWatcher
         A disposable object which can notify of changes to the folder.
   Files.Core.Storage.EventArguments
      DeviceEventArgs (class):  DeviceEventArgs
   Files.Core.Storage.Extensions
      StorageExtensions (class):  CopyContentsToAsync, OpenStreamAsync, TryCreateFileAsync, TryCreateFolderAsync, TryGetFileAsync, TryGetFileByNameAsync, TryGetFolderAsync, TryGetFolderByNameAsync, TryGetStorableAsync, TryOpenStreamAsync
   Files.Core.Storage.Storables
      IDirectCopy (interface):  CreateCopyOfAsync
         Provides direct copy operation of storage objects.
      IDirectMove (interface):  MoveFromAsync
         Provides direct move operation of storage objects.
   Files.Shared
      FileLogger (class):  BeginScope, FileLogger, IsEnabled, Log, PurgeLogs
      FileLoggerProvider (class):  CreateLogger, Dispose, FileLoggerProvider
   Files.Shared.Attributes
      GeneratedRichCommandAttribute (class)
      GeneratedVTableFunctionAttribute (class)
      RegistryIgnoreAttribute (class)
      RegistrySerializableAttribute (class)
   Files.Shared.Extensions
      ArrayExtensions (class):  CloneArray
      ComponentModelExtensions (class):  GetDescription, GetValueFromDescription
      DateExtensions (class):  ToDateTime
      EnumerableExtensions (class):  AddIfNotPresent, CreateEnumerable, CreateList, ParallelForEachAsync, ToListAsync
      FileLoggerExtensions (class):  AddFile
      GenericExtensions (class):  TryCast
      LinqExtensions (class):  AddSorted, ExceptBy, ForEach, Get, GetAsync, IntersectBy, IsEmpty, RemoveFrom, WhereAsync
      SafetyExtensions (class):  IgnoreExceptions, Wrap, WrapAsync
      SerializationExtensions (class):  DeserializeAsync, SerializeAsync
      StringExtensions (class):  Left, Right
      TaskExtensions (class):  AndThen, WithTimeoutAsync
   Files.Shared.Helpers
      AsyncManualResetEvent (class):  Reset, Set, WaitAsync
      ChecksumHelpers (class):  CalculateChecksumForPath, CreateCRC32, CreateMD5, CreateSHA1, CreateSHA256, CreateSHA384, CreateSHA512
      FileExtensionHelpers (class):  HasExtension, IsAhkFile, IsAudioFile, IsBatchFile, IsBrowsableZipFile, IsCertificateFile, IsCmdFile, IsCompatibleToSetAsWindowsWallpaper, IsExecutableFile, IsFontFile, IsHtmlFile, IsImageFile, IsImagePreviewFile, IsInfFile, IsMarkdownFile
         Provides static extension for path extension.
      PathHelpers (class):  Combine, IsInSystemFontsFolder, TryGetFullPath
   Files.Shared.Utils
      IAsyncInitialize (interface):  InitAsync
         Allows an object to be initialized asynchronously.
      IAsyncSerializer (interface):  DeserializeAsync, SerializeAsync
         Provides data serialization abstractions for data.
      IImage (interface)
         Represents an image which can be displayed in the UI.
      IPersistable (interface):  LoadAsync, SaveAsync
         Allows for data to be saved and loaded from a persistence store.
      IWrapper (interface)
         Wraps and exposes implementation for access.
   System
      HashCode (struct):  Add, AddBytes, Combine, Equals, GetHashCode, ToHashCode
   Windows.Win32
      BHID (class)
      CLSID (class)
      ComHeapPtr (struct):  ComHeapPtr, Dispose, Get, GetAddressOf
         Contains a heap pointer allocated via CoTaskMemAlloc and a set of methods to work with the pointer safely.
      ComPtr (struct):  As, Attach, CoCreateInstance, ComPtr, Detach, Dispose, Get, GetAddressOf
         Contains a COM pointer and a set of methods to work with the pointer safely.
      FOLDERID (class)
      IID (class)
      PInvoke (class):  SHUpdateRecycleBinIcon, SetWindowLongPtr
   Windows.Win32.Foundation
      HRESULT (struct):  ThrowIfFailedOnDebug
   Windows.Win32.System.WinRT
      IStorageProviderQuotaUI (struct):  GetQuotaTotalInBytes, GetQuotaUsedInBytes
      IStorageProviderStatusUI (struct):  GetQuotaUI
      IStorageProviderStatusUISource (struct):  GetStatusUI
      IStorageProviderStatusUISourceFactory (struct):  GetStatusUISource
   Windows.Win32.UI.Shell
      IDetectionAndSharing (struct):  GetStatus
      IOpenControlPanel (struct):  Open

CONSUMER PATHS
   contract  →  implement IAsyncInitialize
   contract  →  implement IOmnibarTextMemberPathProvider
   contract  →  implement IToolbarItemSet
   contract  →  implement IWindowsStorable
   configure  →  ArrayExtensions.*
   configure  →  ComponentModelExtensions.*

PACKAGES
   Other:  CommunityToolkit.Labs.WinUI.DependencyPropertyGenerator, CommunityToolkit.WinUI.Extensions, Dongle.GuidRVAGen, FluentFTP, Microsoft.CodeAnalysis.Analyzers, Microsoft.CodeAnalysis.CSharp, Microsoft.CodeAnalysis.Workspaces.Common, Microsoft.Extensions.Logging … (19 total)

→ drill in:  --focus "<TypeName>"   (e.g. --focus IAsyncInitialize)
