LIBRARY  MahApps.Metro     (196 public types)

ENTRY API
   derive    BaseMetroDialog   (BaseMetroDialog.cs)
      The base class for dialogs.
   derive    BaseMetroTabControl   (MetroTabControl.cs)
      A base class for every MetroTabControl (Pivot).
   derive    MarkupConverter   (MarkupConverter.cs)
   derive    MarkupMultiConverter   (MarkupConverter.cs)
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
   BaseMetroTabControl (class)  — 3 implementors
   HamburgerMenuItem (class)  — 3 implementors
   HamburgerMenuItemBase (class)  — 3 implementors
   ColorPickerBase (class)  — 2 implementors
   CommandTriggerAction (class)  — 2 implementors
   IMetroThumb (interface)  — 2 implementors
   IconElement (class)  — 2 implementors

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
      ColorHelper (class):  ColorFromString, ColorHelper, GetColorName
         A Helper class for the Color-Struct
      ColorPalette (class):  ColorPalette
      ColorPicker (class):  ColorPicker, OnApplyTemplate
      ColorPickerBase (class)
      ComboBoxHelper (class):  GetCharacterCasing, GetInterceptMouseWheelSelection, GetMaxLength, SetCharacterCasing, SetInterceptMouseWheelSelection, SetMaxLength
         A helper class that provides various attached properties for the control.
      ContentControlEx (class):  ContentControlEx
      ContentPresenterEx (class):  ContentPresenterEx
      ControlsHelper (class):  GetContentCharacterCasing, GetCornerRadius, GetDisabledVisualElementVisibility, GetFocusBorderBrush, GetFocusBorderThickness, GetIsReadOnly, GetMouseOverBorderBrush, GetRecognizesAccessKey, SetContentCharacterCasing, SetCornerRadius, SetDisabledVisualElementVisibility, SetFocusBorderBrush, SetFocusBorderThickness, SetIsReadOnly, SetMouseOverBorderBrush
         A helper class that provides various controls.
      CustomValidationPopup (class):  CustomValidationPopup
         This custom popup is used by the validation error template.
      DataGridColumnStylesHelperExtension (class):  Attach, Detach, ProvideValue
      DataGridHelper (class):  GetAutoGeneratedCheckBoxColumnEditingStyle, GetAutoGeneratedCheckBoxColumnStyle, GetAutoGeneratedComboBoxColumnEditingStyle, GetAutoGeneratedComboBoxColumnStyle, GetAutoGeneratedHyperlinkColumnEditingStyle, GetAutoGeneratedHyperlinkColumnStyle, GetAutoGeneratedNumericUpDownColumnEditingStyle, GetAutoGeneratedNumericUpDownColumnStyle, GetAutoGeneratedTextColumnEditingStyle, GetAutoGeneratedTextColumnStyle, GetCellPadding, GetColumnHeaderPadding, GetColumnStylesHelper, GetEnableCellEditAssist, GetSelectionUnit
      DataGridNumericUpDownColumn (class):  DataGridNumericUpDownColumn
      DatePickerHelper (class):  GetDropDownButtonContent, GetDropDownButtonContentTemplate, GetDropDownButtonFontFamily, GetDropDownButtonFontSize, SetDropDownButtonContent, SetDropDownButtonContentTemplate, SetDropDownButtonFontFamily, SetDropDownButtonFontSize
      DateTimePicker (class):  DateTimePicker, OnApplyTemplate
         Represents a control that allows the user to select a date and a time.
      DropDownButton (class):  DropDownButton, OnApplyTemplate
      ExpanderHelper (class):  GetCollapseStoryboard, GetExpandStoryboard, GetHeaderDownStyle, GetHeaderLeftStyle, GetHeaderRightStyle, GetHeaderUpStyle, GetShowToggleButton, SetCollapseStoryboard, SetExpandStoryboard, SetHeaderDownStyle, SetHeaderLeftStyle, SetHeaderRightStyle, SetHeaderUpStyle, SetShowToggleButton
         A helper class that provides various attached properties for the Expander control.
      Extensions (class):  BeginInvoke, ExecuteWhenLoaded, Invoke
      FlipView (class):  FlipView, GoBack, GoForward, HideControlButtons, OnApplyTemplate, ShowControlButtons
         A control that imitate a slide show with back/forward buttons.
      FlipViewItem (class):  FlipViewItem, OnApplyTemplate
      Flyout (class):  Flyout, OnApplyTemplate
         A sliding panel control that is hosted in a via a .
      FlyoutStatusChangedRoutedEventArgs (class)
      FlyoutsControl (class):  FlyoutsControl
         A FlyoutsControl is for displaying flyouts in a .
      FontIcon (class):  FontIcon, OnApplyTemplate
         Represents an icon that uses a glyph from the specified font.
      GridLengthAnimation (class):  GetCurrentValue
         A special animation used to animates the length of a .
      GridViewHeaderRowPresenterEx (class)
      HSVColor (struct):  Equals, HSVColor, ToColor
         This struct represent a Color in HSV (Hue, Saturation, Value) For more information visit: https://en.wikipedia.org/wi...
      HamburgerMenu (class):  HamburgerMenu, OnApplyTemplate, RaiseItemCommand, RaiseOptionsItemCommand
         The HamburgerMenu is based on a control.
      HamburgerMenuGlyphItem (class)
         The HamburgerMenuGlyphItem provides a glyph based implementation for HamburgerMenu entries.
      HamburgerMenuHeaderItem (class)
      HamburgerMenuIconItem (class)
         The HamburgerMenuIconItem provides an icon based implementation for HamburgerMenu entries.
      HamburgerMenuImageItem (class)
         The HamburgerMenuImageItem provides an image based implementation for HamburgerMenu entries.
      HamburgerMenuItem (class):  RaiseCommand
         The HamburgerMenuItem provides an implementation for HamburgerMenu entries.
      HamburgerMenuItemBase (class)
      HamburgerMenuItemCollection (class)
         The HamburgerMenuItemCollection provides typed collection of HamburgerMenuItemBase.
      HamburgerMenuItemInvokedEventArgs (class):  HamburgerMenuItemInvokedEventArgs
         EventArgs used for the event.
      HamburgerMenuItemStyleSelector (class):  SelectStyle
      HamburgerMenuListBox (class):  HamburgerMenuListBox
      HamburgerMenuSeparatorItem (class)
         The HamburgerMenuSeparatorItem provides an separator based implementation for HamburgerMenu entries.
      HeaderedControlHelper (class):  GetHeaderBackground, GetHeaderFontFamily, GetHeaderFontSize, GetHeaderFontStretch, GetHeaderFontWeight, GetHeaderForeground, GetHeaderHorizontalContentAlignment, GetHeaderMargin, GetHeaderVerticalContentAlignment, SetHeaderBackground, SetHeaderFontFamily, SetHeaderFontSize, SetHeaderFontStretch, SetHeaderFontWeight, SetHeaderForeground
      HotKey (class):  Equals, GetHashCode, HotKey, ToString
      HotKeyBox (class):  HotKeyBox, OnApplyTemplate
      IDataGridColumnStylesHelper (interface):  Attach, Detach
      IHamburgerMenuHeaderItem (interface)
      IHamburgerMenuItem (interface)
      IHamburgerMenuItemBase (interface)
      IHamburgerMenuSeparatorItem (interface)
      IMetroThumb (interface)
      IWindowPlacementSettings (interface):  Reload, Reset, Save, Upgrade
      IconElement (class):  IconElement
         Represents the base class for an icon UI element.
      ItemClickEventArgs (class):  ItemClickEventArgs
         EventArgs used for the and events.
      ItemHelper (class):  GetActiveSelectionBackgroundBrush, GetActiveSelectionBorderBrush, GetActiveSelectionForegroundBrush, GetDisabledBackgroundBrush, GetDisabledBorderBrush, GetDisabledForegroundBrush, GetDisabledSelectedBackgroundBrush, GetDisabledSelectedBorderBrush, GetDisabledSelectedForegroundBrush, GetGridViewHeaderIndicatorBrush, GetHoverBackgroundBrush, GetHoverBorderBrush, GetHoverForegroundBrush, GetHoverSelectedBackgroundBrush, GetHoverSelectedBorderBrush
      LayoutInvalidationCatcher (class)
      MahAppsCommands (class):  MahAppsCommands
      MetroAnimatedSingleRowTabControl (class):  MetroAnimatedSingleRowTabControl
         A MetroTabControl (Pivot) that wraps TabItem/MetroTabItem headers on a single row.
      MetroAnimatedTabControl (class):  MetroAnimatedTabControl
         A MetroTabControl (Pivot) that uses a TransitioningContentControl to animate the contents of a TabItem/MetroTabItem.
      MetroContentControl (class):  MetroContentControl, OnApplyTemplate, Reload
         A ContentControl which use a transition to slide in the content.
      MetroHeader (class):  MetroHeader, OnApplyTemplate
      MetroNavigationWindow (class):  AddBackEntry, GoBack, GoForward, GoHome, MetroNavigationWindow, Navigate, RemoveBackEntry, StopLoading
         A reimplementation of NavigationWindow based on MetroWindow.
      MetroProgressBar (class):  MetroProgressBar, OnApplyTemplate
         A metrofied ProgressBar.
      MetroTabControl (class):  MetroTabControl
         A standard MetroTabControl (Pivot).
      MetroTabItem (class):  MetroTabItem
         An extended TabItem with a metro style.
      MetroThumb (class)
      MetroThumbContentControl (class):  CancelDragAction, MetroThumbContentControl
         The MetroThumbContentControl control can be used for titles or something else and enables basic drag movement functio...
      MetroThumbContentControlDragCompletedEventArgs (class):  MetroThumbContentControlDragCompletedEventArgs
      MetroThumbContentControlDragStartedEventArgs (class):  MetroThumbContentControlDragStartedEventArgs
      MetroWindow (class):  GetWindowPlacementSettings, HideOverlay, HideOverlayAsync, IsOverlayVisible, MetroWindow, OnApplyTemplate, ResetStoredFocus, ShowOverlay, ShowOverlayAsync, StoreFocus
         An extended Window class.
      MultiFrameImage (class):  MultiFrameImage
      MultiSelectionComboBox (class):  GetSelectedItemsText, MultiSelectionComboBox, OnApplyTemplate, ResetEditableText, SelectAll, UnselectAll
      MultiSelectorHelper (class):  GetSelectedItems, SetSelectedItems
         Defines a helper class for SelectedItems binding on , or controls.
      NumericUpDown (class):  NumericUpDown, OnApplyTemplate, SelectAll
         Represents a Windows spin box (also known as an up-down control) that displays numeric values.
      NumericUpDownChangedRoutedEventArgs (class):  NumericUpDownChangedRoutedEventArgs
      PasswordBoxHelper (class):  GetCapsLockIcon, GetCapsLockWarningToolTip, GetRevealButtonContent, GetRevealButtonContentTemplate, SetCapsLockIcon, SetCapsLockWarningToolTip, SetRevealButtonContent, SetRevealButtonContentTemplate
      PathIcon (class):  OnApplyTemplate, PathIcon
         Represents an icon that uses a vector path as its content.
      Pivot (class):  GoToItem, OnApplyTemplate, Pivot
      PivotItem (class):  PivotItem
      Planerator (class):  Refresh
         Based on Greg Schechter's Planerator http://blogs.msdn.com/b/greg_schechter/archive/2007/10/26/enter-the-planerator-d...
      ProgressRing (class):  OnApplyTemplate, ProgressRing
      RadioButtonHelper (class):  GetBackgroundDisabled, GetBackgroundPointerOver, GetBackgroundPressed, GetBorderBrushDisabled, GetBorderBrushPointerOver, GetBorderBrushPressed, GetCheckGlyphFill, GetCheckGlyphFillDisabled, GetCheckGlyphFillPointerOver, GetCheckGlyphFillPressed, GetCheckGlyphStroke, GetCheckGlyphStrokeDisabled, GetCheckGlyphStrokePointerOver, GetCheckGlyphStrokePressed, GetForegroundDisabled
      RangeSelectionChangedEventArgs (class):  RangeSelectionChangedEventArgs
         This RangeSelectionChangedEventArgs class contains old and new value when RangeSelectionChanged is raised.
      RangeSlider (class):  MoveSelection, OnApplyTemplate, RangeSlider, ResetSelection
         A slider control with the ability to select a range between two values.
      RangeSliderAutoTooltipValues (class):  ToString
      RevealImage (class):  OnApplyTemplate, RevealImage
      ScrollViewerHelper (class):  GetBubbleUpScrollEventToParentScrollviewer, GetEndOfHorizontalScrollReachedCommand, GetEndOfScrollReachedCommandParameter, GetEndOfVerticalScrollReachedCommand, GetIsHorizontalScrollWheelEnabled, GetScrollContentPresenterMargin, GetVerticalScrollBarOnLeftSide, OnBubbleUpScrollEventToParentScrollviewerPropertyChanged, SetBubbleUpScrollEventToParentScrollviewer, SetEndOfHorizontalScrollReachedCommand, SetEndOfScrollReachedCommandParameter, SetEndOfVerticalScrollReachedCommand, SetIsHorizontalScrollWheelEnabled, SetScrollContentPresenterMargin, SetVerticalScrollBarOnLeftSide
      ScrollViewerOffsetMediator (class)
      SliderHelper (class):  GetChangeValueBy, GetEnableMouseWheel, GetThumbFillBrush, GetThumbFillDisabledBrush, GetThumbFillHoverBrush, GetThumbFillPressedBrush, GetTrackFillBrush, GetTrackFillDisabledBrush, GetTrackFillHoverBrush, GetTrackFillPressedBrush, GetTrackValueFillBrush, GetTrackValueFillDisabledBrush, GetTrackValueFillHoverBrush, GetTrackValueFillPressedBrush, SetChangeValueBy
      Spelling (class)
      SplitButton (class):  OnApplyTemplate, SplitButton
      SplitView (class):  OnApplyTemplate, SplitView
         Represents a container with two views; one view for the main content and another view that is typically used for navi...
      SplitViewPaneClosingEventArgs (class)
         Provides event data for the event.
      SplitViewTemplateSettings (class)
         Provides calculated values that can be referenced as TemplatedParent sources when defining templates for a .
      TabControlHelper (class):  ClearStyle, GetCloseButtonEnabled, GetCloseTabCommand, GetCloseTabCommandParameter, GetTransition, GetUnderlineBrush, GetUnderlineMargin, GetUnderlineMouseOverBrush, GetUnderlineMouseOverSelectedBrush, GetUnderlinePlacement, GetUnderlineSelectedBrush, GetUnderlined, SetCloseButtonEnabled, SetCloseTabCommand, SetCloseTabCommandParameter
      TabItemClosingEventArgs (class)
         Event args that is created when a TabItem is closed.
      TextBoxHelper (class):  GetAutoWatermark, GetButtonCommand, GetButtonCommandParameter, GetButtonCommandTarget, GetButtonContent, GetButtonContentTemplate, GetButtonFontFamily, GetButtonFontSize, GetButtonTemplate, GetButtonWidth, GetButtonsAlignment, GetClearTextButton, GetHasText, GetIsMonitoring, GetIsSpellCheckContextMenuEnabled
         A helper class that provides various attached properties for the TextBox control.
      Tile (class):  Tile
      TimePicker (class):  TimePicker
         Represents a control that allows the user to select a time.
      TimePickerBase (class):  Clear, OnApplyTemplate, TimePickerBase
         Represents a base-class for time picking.
      ToggleButtonHelper (class):  GetContentDirection, SetContentDirection
      ToggleSwitch (class):  OnApplyTemplate, ToggleSwitch
         A control that allows the user to toggle between two states: One represents true; The other represents false.
      TransitioningContentControl (class):  AbortTransition, OnApplyTemplate, ReloadTransition, TransitioningContentControl
         A ContentControl that animates content as it loads and unloads.
      TreeHelper (class):  FindChild, FindChildren, GetAncestors, GetChildObjects, GetParentObject, GetVisualAncestor, GetVisualAncestry, IsDescendantOf, TryFindFromPoint, TryFindParent
         Helper methods for UI-related tasks.
      TreeViewItemHelper (class):  GetToggleButtonStyle, SetToggleButtonStyle
      Underline (class):  OnApplyTemplate, Underline
      Utils (class):  CollapseThickness, Deflate, Inflate, IsCloseTo, IsEqualTo, IsGreaterThan, IsLessThan, IsNaN, IsOne, IsOpaqueSolidColorBrush, IsUniform, IsValid, IsZero, RoundLayoutValue
         A few very useful extension methods
      ValidationHelper (class):  GetAlwaysShowValidationError, GetCloseOnMouseLeftButtonDown, GetShowValidationErrorOnKeyboardFocus, GetShowValidationErrorOnMouseOver, SetAlwaysShowValidationError, SetCloseOnMouseLeftButtonDown, SetShowValidationErrorOnKeyboardFocus, SetShowValidationErrorOnMouseOver
      VisibilityHelper (class):  GetIsCollapsed, GetIsHidden, GetIsVisible, SetIsCollapsed, SetIsHidden, SetIsVisible
      WindowButtonCommands (class):  WindowButtonCommands
      WindowCommands (class):  WindowCommands
      WindowCommandsItem (class):  OnApplyTemplate, WindowCommandsItem
      WindowPlacementSetting (class)
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
      MetroDialogSettings (class):  MetroDialogSettings
         A class that represents the settings used by Metro Dialogs.
      ProgressDialog (class):  OnApplyTemplate, ProgressDialog
         An internal control that represents a message dialog.
      ProgressDialogController (class):  CloseAsync, SetCancelable, SetIndeterminate, SetMessage, SetProgress, SetProgressBarForegroundBrush, SetTitle
         A class for manipulating an open ProgressDialog.
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
      CornerRadiusFilterConverter (class):  Convert, ConvertBack
         Filters a CornerRadius by the given Filter property.
      FontSizeOffsetConverter (class):  Convert, ConvertBack
      HSVColorChannel2BrushConverter (class):  Convert, ConvertBack
         Converts a given HSVColor to a new SolidColorBrush with the specified Channel.
      HSVColorChannel2GradientBrushConverter (class):  Convert, ConvertBack
         Converts a given HSVColor to a new LinearGradientBrush with the specified Channel.
      HSVColorChannelMinMaxConverter (class):  Convert, ConvertBack
         Converts a given Color to a new Color with the specified Channel turned to the Min or Max Value
      IsNotNullConverter (class):  Convert, ConvertBack
      IsNullConverter (class):  Convert, ConvertBack
      MarkupConverter (class):  Convert, ConvertBack, ProvideValue
      MarkupMultiConverter (class):  Convert, ConvertBack, ProvideValue
      MathAddConverter (class):  Convert, ConvertBack
         MathAddConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
      MathConverter (class):  Convert, ConvertBack
         MathConverter provides a value converter which can be used for math operations.
      MathDivideConverter (class):  Convert, ConvertBack
         MathDivideConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
      MathMultiplyConverter (class):  Convert, ConvertBack
         MathMultiplyConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
      MathSubtractConverter (class):  Convert, ConvertBack
         MathSubtractConverter provides a multi value converter as a MarkupExtension which can be used for math operations.
      NullToUnsetValueConverter (class)
      PercentageToGridLengthConverter (class):  Convert, ConvertBack
      ResizeModeMinMaxButtonVisibilityConverter (class):  Convert, ConvertBack
      SizeToCornerRadiusConverter (class)
         This Converter converts a given height or width of an control to a CornerRadius
      StringIsNullOrEmptyConverter (class):  Convert, ConvertBack
      StringToVisibilityConverter (class):  StringToVisibilityConverter
         Converts a String into a Visibility enumeration (and back).
      ThicknessFilterConverter (class):  Convert, ConvertBack
         Filters a Thickness by the given Filter property.
      ThicknessToDoubleConverter (class):  Convert, ConvertBack
      ToLowerConverter (class)
      ToUpperConverter (class)
      TreeViewItemExtensions (class):  GetDepth
      TreeViewMarginConverter (class):  Convert, ConvertBack
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
   System.Diagnostics.CodeAnalysis
      MaybeNullWhenAttribute (class):  MaybeNullWhenAttribute
         Specifies that when a method returns , the parameter may be null even if the corresponding type disallows it.
      NotNullWhenAttribute (class):  NotNullWhenAttribute
         Specifies that when a method returns , the parameter will not be null even if the corresponding type allows it.

CONSUMER PATHS
   extend  →  derive BaseMetroDialog
   extend  →  derive BaseMetroTabControl
   extend  →  derive MarkupConverter
   extend  →  derive MarkupMultiConverter
   configure  →  DialogManager.*
   configure  →  Extensions.*

PACKAGES
   Other:  ControlzEx, Microsoft.Windows.CsWin32, System.Memory, System.ValueTuple, XAMLTools.MSBuild

→ drill in:  --focus "<TypeName>"   (e.g. --focus BaseMetroDialog)
