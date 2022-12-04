using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyCaster.Alarm.Controls;

public partial class HotKeyEditorControl : UserControl
{
    public static readonly DependencyProperty HotKeyProperty =
        DependencyProperty.Register
        (
            nameof(HotKey),
            typeof(HotKey),
            typeof(HotKeyEditorControl),
            new FrameworkPropertyMetadata(
                default(HotKey),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
        );

    public static readonly DependencyProperty NotSetTextProperty =
     DependencyProperty.Register
     (
         nameof(NotSetText),
         typeof(string),
         typeof(HotKeyEditorControl),
         new FrameworkPropertyMetadata(
             "",
             FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
     );

    public string NotSetText
    {
        get => (string)GetValue(NotSetTextProperty);
        set => SetValue(NotSetTextProperty, value);
    }


    public HotKey HotKey
    {
        get => (HotKey)GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }

    public HotKeyEditorControl()
    {
        InitializeComponent();
    }

    private void HotKeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Don't let the event pass further
        // because we don't want standard textbox shortcuts working
        e.Handled = true;

        // Get modifiers and key data
        var modifiers = Keyboard.Modifiers;
        var key = e.Key;

        // When Alt is pressed, SystemKey is used instead
        if (key == Key.System)
        {
            key = e.SystemKey;
        }

        // Pressing delete, backspace or escape without modifiers clears the current value
        if (modifiers == ModifierKeys.None &&
            (key == Key.Delete || key == Key.Back))
        {
            HotKey = null;
            return;
        }

        // If no actual key was pressed - return
        if (key == Key.LeftCtrl ||
            key == Key.RightCtrl ||
            key == Key.LeftAlt ||
            key == Key.RightAlt ||
            key == Key.LeftShift ||
            key == Key.RightShift ||
            key == Key.LWin ||
            key == Key.RWin ||
            key == Key.Clear ||
            key == Key.OemClear ||
            key == Key.Apps)
        {
            return;
        }
        // Update the value
        HotKey = new HotKey(key, modifiers);
        if (!string.IsNullOrEmpty(HotKeyTextBox.Text))
            HotKeyTextBox.CaretIndex = HotKeyTextBox.Text.Length;
    }
}
