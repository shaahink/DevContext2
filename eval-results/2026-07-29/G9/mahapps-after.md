LIBRARY  MahApps.Metro     (202 public types)

ENTRY API
   derive    BaseMetroDialog   (BaseMetroDialog.cs)
      The base class for dialogs.
   derive    MarkupConverter   (MarkupConverter.cs)
   derive    MarkupMultiConverter   (MarkupConverter.cs)
   derive    MetroWindow   (MetroWindow.cs)
      An extended Window class.
   extend    DialogManager   (DialogManager.cs)
   extend    Extensions   (Extensions.cs)
   extend    TabControlHelper   (TabControlHelper.cs)
   extend    TreeHelper   (TreeHelper.cs)
      Helper methods for UI-related tasks.
   extend    TreeViewItemExtensions   (TreeViewMarginConverter.cs)
   extend    Utils   (ClipBorder.Utils.cs)
      A few very useful extension methods

ABSTRACTIONS
   BaseMetroDialog (class)  — 5 implementors
   MarkupConverter (class)  — 5 implementors
   MarkupMultiConverter (class)  — 5 implementors
   MetroWindow (class)  — 5 implementors
   BaseMetroTabControl (class)  — 3 implementors
   HamburgerMenuItem (class)  — 3 implementors
   HamburgerMenuItemBase (class)  — 3 implementors
   ColorPickerBase (class)  — 2 implementors
   CommandTriggerAction (class)  — 2 implementors
   IMetroThumb (interface)  — 2 implementors

PUBLIC SURFACE
   MahApps.Metro
      MahAppsException (class):  MahAppsException
      MissingRequiredTemplatePartException (class):  MissingRequiredTemplatePartException
   MahApps.Metro.Accessibility
      AccessibilitySwitches (class)
   MahApps.Metro.Actions
      CloseFlyoutAction (class)
      CloseTabItemAction (class)
      CommandTriggerAction (class):  CommandTriggerAction
         This CommandTriggerAction can be used to bind any event on any FrameworkElement to an .
   MahApps.Metro.Automation.Peers
      FlyoutAutomationPeer (class):  FlyoutAutomationPeer
      MetroDialogAutomationPeer (class):  MetroDialogAutomationPeer
      MetroHeaderAutomationPeer (class):  MetroHeaderAutomationPeer
         The MetroHeaderAutomationPeer class exposes the type to UI Automation.
      MetroThumbContentControlAutomationPeer (class):  MetroThumbContentControlAutomationPeer
         The MetroThumbContentControlAutomationPeer class exposes the type to UI Automation.
      MetroWindowAutomationPeer (class):  MetroWindowAutomationPeer
      NumericUpdDownAutomationPeer (class):  NumericUpdDownAutomationPeer
      ProgressRingAutomationPeer (class):  ProgressRingAutomationPeer
      ToggleSwitchAutomationPeer (class):  GetPattern, Toggle, ToggleSwitchAutomationPeer
      WindowCommandsAutomationPeer (class):  WindowCommandsAutomationPeer
   MahApps.Metro.Behaviors
      DatePickerTextBoxBehavior (class)
      PasswordBoxBindingBehavior (class):  GetPassword, SetPassword
      ReloadBehavior (class):  GetOnDataContextChanged, GetOnSelectedTabChanged, SetOnDataContextChanged, SetOnSelectedTabChanged
      StylizedBehaviorCollection (class)
      StylizedBehaviors (class):  GetBehaviors, SetBehaviors
      TabControlSelectFirstVisibleTabBehavior (class)
         Sets the first TabItem with Visibility="" as the SelectedItem of the TabControl.
      TiltBehavior (class)
      WindowsSettingBehavior (class)
   MahApps.Metro.Controls
      AddedItemEventArgs (class):  AddedItemEventArgs
         Provides data for the
      AddingItemEventArgs (class):  AddingItemEventArgs
         Provides data for the
      AmPmComparer (class):  Compare
         Represents an hour comparison operation that ensures that 12 is smaller than 1.
      Badged (class):  Badged, OnApplyTemplate
      BaseMetroTabControl (class):  BaseMetroTabControl
         A base class for every MetroTabControl (Pivot).
      BuildInColorPalettes (class):  AddColorToRecentColors, GetMaximumRecentColorsCount, SetMaximumRecentColorsCount
      CheckBoxHelper (class):  GetBackgroundChecked, GetBackgroundCheckedDisabled, GetBackgroundCheckedMouseOver, GetBackgroundCheckedPressed, GetBackgroundIndeterminate, GetBackgroundIndeterminateDisabled, GetBackgroundIndeterminateMouseOver, GetBackgroundIndeterminatePressed, GetBackgroundUnchecked, GetBackgroundUncheckedDisabled, GetBackgroundUncheckedMouseOver, GetBackgroundUncheckedPressed, GetBorderBrushChecked, GetBorderBrushCheckedDisabled, GetBorderBrushCheckedMouseOver
      ClipBorder (class)
         Represents a border whose contents are clipped within the bounds of the border.
      ClosingWindowEventHandlerArgs (class)
      ColorCanvas (class):  ColorCanvas, OnApplyTemplate
      ColorEyeDropper (class):  ColorEyeDropper
      ColorEyePreviewData (class)
      … and 113 more (the structured surface lists them all)
   MahApps.Metro.Controls.Dialogs
      BaseMetroDialog (class):  BaseMetroDialog, WaitForCloseAsync, WaitForLoadAsync, WaitUntilUnloadedAsync
         The base class for dialogs.
      CustomDialog (class):  CustomDialog
         An implementation of BaseMetroDialog allowing arbitrary content.
      DialogCoordinator (class):  GetCurrentDialogAsync, HideMetroDialogAsync, ShowInputAsync, ShowLoginAsync, ShowMessageAsync, ShowMetroDialogAsync, ShowModalInputExternal, ShowModalLoginExternal, ShowModalMessageExternal, ShowProgressAsync
      DialogManager (class):  GetCurrentDialogAsync, HideMetroDialogAsync, ShowInputAsync, ShowLoginAsync, ShowMessageAsync, ShowMetroDialogAsync, ShowModalInputExternal, ShowModalLoginExternal, ShowModalMessageExternal, ShowProgressAsync
      DialogParticipation (class):  GetRegister, SetRegister
      DialogStateChangedEventArgs (class)
      IDialogCoordinator (interface):  GetCurrentDialogAsync, HideMetroDialogAsync, ShowInputAsync, ShowLoginAsync, ShowMessageAsync, ShowMetroDialogAsync, ShowModalInputExternal, ShowModalLoginExternal, ShowModalMessageExternal, ShowProgressAsync
         Use the dialog coordinator to help you interface with dialogs from a view model.
      InputDialog (class):  InputDialog, OnApplyTemplate
      LoginDialog (class):  LoginDialog, OnApplyTemplate
      LoginDialogData (class)
      LoginDialogSettings (class):  LoginDialogSettings
      MessageDialog (class):  MessageDialog, OnApplyTemplate
         An internal control that represents a message dialog.
      … and 3 more (the structured surface lists them all)
   MahApps.Metro.Controls.Helper
      BindingHelper (class):  Eval
         A helper class to evaluate Bindings in code behind
   MahApps.Metro.Converters
      BackgroundToForegroundConverter (class):  Convert, ConvertBack
      ClockDegreeConverter (class):  Convert, ConvertBack
         Converts a double representing either hour/minute/second to the corresponding angle.
      ColorChannel2GradientBrushConverter (class):  Convert, ConvertBack
         Converts a given Color to a new LinearGradientBrush with the specified Channel.
      ColorChannelMinMaxConverter (class):  Convert, ConvertBack
         Converts a given Color to a new Color with the specified Channel turned to the Min or Max Value
      ColorToNameConverter (class):  Convert, ConvertBack
      ColorToSolidColorBrushConverter (class):  Convert, ConvertBack
         Converts a given into a .
      CornerRadiusBindingConverter (class):  Convert, ConvertBack
         Converts a CornerRadius to a new CornerRadius.
      CornerRadiusFilterConverter (class):  Convert, ConvertBack
         Filters a CornerRadius by the given Filter property.
      FontSizeOffsetConverter (class):  Convert, ConvertBack
      HSVColorChannel2BrushConverter (class):  Convert, ConvertBack
         Converts a given HSVColor to a new SolidColorBrush with the specified Channel.
      HSVColorChannel2GradientBrushConverter (class):  Convert, ConvertBack
         Converts a given HSVColor to a new LinearGradientBrush with the specified Channel.
      HSVColorChannelMinMaxConverter (class):  Convert, ConvertBack
         Converts a given Color to a new Color with the specified Channel turned to the Min or Max Value
      … and 22 more (the structured surface lists them all)
   MahApps.Metro.Lang
      MultiSelectionComboBox (class)
         A strongly-typed resource class, for looking up localized strings, etc.
   MahApps.Metro.Markup
      StaticResourceExtension (class):  StaticResourceExtension
         Implements a markup extension that supports static (XAML load time) resource references made from XAML.
   MahApps.Metro.Theming
      MahAppsLibraryThemeProvider (class):  FillColorSchemeValues, MahAppsLibraryThemeProvider
         Provides theme resources from MahApps.Metro.
   MahApps.Metro.ValueBoxes
      BooleanBoxes (class):  Box
         Helps boxing Boolean values.

CONSUMER PATHS
   extend  →  derive BaseMetroDialog
   extend  →  derive MarkupConverter
   extend  →  derive MarkupMultiConverter
   extend  →  derive MetroWindow
   configure  →  DialogManager.*
   configure  →  Extensions.*

PACKAGES
   Other:  ControlzEx, Microsoft.Windows.CsWin32 0.3.162, System.Memory 4.6.3, System.ValueTuple 4.6.1, XAMLTools.MSBuild 1.0.0-alpha0167

→ drill in:  trace a focused type   (e.g. trace BaseMetroDialog)
