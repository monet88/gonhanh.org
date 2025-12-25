using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GoNhanh.Core;
using GoNhanh.Services;

namespace GoNhanh.Views;

/// <summary>
/// Settings popup window
/// Compact popup UI positioned near system tray
/// </summary>
public partial class SettingsPopup : Window
{
    private readonly SettingsService _settings;
    private bool _isInitializing = true;

    public event Action<InputMethod>? OnMethodChanged;
    public event Action<bool>? OnModernToneChanged;
    public event Action<bool>? OnShortcutChanged;
    public event Action<bool>? OnAutoStartChanged;

    public SettingsPopup(SettingsService settings)
    {
        InitializeComponent();
        _settings = settings;
        LoadSettings();
        PositionNearTray();
        _isInitializing = false;
    }

    private void LoadSettings()
    {
        MethodCombo.SelectedIndex = (int)_settings.CurrentMethod;
        ModernToneCheck.IsChecked = _settings.UseModernTone;
        ShortcutCheck.IsChecked = _settings.ShortcutEnabled;
        AutoStartCheck.IsChecked = _settings.AutoStart;
        VersionText.Text = $"Phiên bản: v{AppMetadata.Version}";
    }

    private void PositionNearTray()
    {
        // Position near bottom-right (system tray area)
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 10;
        Top = workArea.Bottom - Height - 10;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
            Close();
        base.OnKeyDown(e);
    }

    private void MethodCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;
        var method = (InputMethod)MethodCombo.SelectedIndex;
        OnMethodChanged?.Invoke(method);
    }

    private void ModernToneCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        OnModernToneChanged?.Invoke(ModernToneCheck.IsChecked ?? true);
    }

    private void ShortcutCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        OnShortcutChanged?.Invoke(ShortcutCheck.IsChecked ?? true);
    }

    private void AutoStartCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing) return;
        OnAutoStartChanged?.Invoke(AutoStartCheck.IsChecked ?? false);
    }
}
