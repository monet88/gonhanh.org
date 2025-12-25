using System.Windows;
using GoNhanh.Core;
using GoNhanh.Services;
using GoNhanh.Views;

namespace GoNhanh;

/// <summary>
/// GoNhanh - Vietnamese Input Method for Windows
/// Main application entry point
/// Matches macOS App.swift flow
/// </summary>
public partial class App : System.Windows.Application
{
    private TrayIcon? _trayIcon;
    private KeyboardHook? _keyboardHook;
    private readonly SettingsService _settings = new();
    private readonly UpdateService _updateService = new();
    private System.Threading.Mutex? _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Prevent multiple instances
        if (!EnsureSingleInstance())
        {
            Shutdown();
            return;
        }

        // Initialize Rust core engine
        RustBridge.Initialize();

        // Load settings
        _settings.Load();
        ApplySettings();

        // Initialize keyboard hook
        _keyboardHook = new KeyboardHook();
        _keyboardHook.KeyPressed += OnKeyPressed;
        _keyboardHook.HotkeyToggle += OnHotkeyToggle;
        _keyboardHook.Start();

        // Initialize system tray
        _trayIcon = new TrayIcon();
        _trayIcon.OnExitRequested += ExitApplication;
        _trayIcon.OnMethodChanged += ChangeInputMethod;
        _trayIcon.OnEnabledChanged += ToggleEnabled;
        _trayIcon.OnModernToneChanged += ChangeModernTone;
        _trayIcon.OnShortcutChanged += ChangeShortcutEnabled;
        _trayIcon.OnSettingsRequested += ShowSettings;
        _trayIcon.Initialize(_settings.CurrentMethod, _settings.IsEnabled);
        _trayIcon.UpdateToggles(_settings.UseModernTone, _settings.ShortcutEnabled);

        // Show onboarding if first run (like macOS)
        if (_settings.IsFirstRun)
        {
            ShowOnboarding();
        }

        // Check for updates (async, fire and forget)
        _ = CheckForUpdatesAsync();
    }

    private bool EnsureSingleInstance()
    {
        _mutex = new System.Threading.Mutex(true, "GoNhanh_SingleInstance", out bool createdNew);
        if (!createdNew)
        {
            System.Windows.MessageBox.Show(
                $"{AppMetadata.Name} đang chạy.\nKiểm tra khay hệ thống (system tray).",
                AppMetadata.Name,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return false;
        }
        return true;
    }

    private void ApplySettings()
    {
        RustBridge.SetMethod(_settings.CurrentMethod);
        RustBridge.SetEnabled(_settings.IsEnabled);
        RustBridge.SetModernTone(_settings.UseModernTone);
    }

    private async Task CheckForUpdatesAsync()
    {
        await _updateService.CheckForUpdatesAsync();

        if (_updateService.UpdateAvailable &&
            _updateService.LatestVersion != null &&
            _updateService.ReleaseUrl != null)
        {
            _trayIcon?.ShowUpdateItem(
                _updateService.LatestVersion,
                _updateService.ReleaseUrl);
        }
    }

    private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
    {
        if (!_settings.IsEnabled) return;

        var result = RustBridge.ProcessKey(e.VirtualKeyCode, e.Shift, e.CapsLock);

        if (result.Action == ImeAction.Send && result.Count > 0)
        {
            e.Handled = true;
            TextSender.SendText(result.GetText(), result.Backspace);
        }
        else if (result.Action == ImeAction.Restore)
        {
            e.Handled = true;
            TextSender.SendText(result.GetText(), result.Backspace);
        }
    }

    private void ShowOnboarding()
    {
        var onboarding = new OnboardingWindow(_settings);
        onboarding.ShowDialog();

        // Save settings after onboarding
        _settings.IsFirstRun = false;
        _settings.Save();

        ApplySettings();
        _trayIcon?.UpdateState(_settings.CurrentMethod, _settings.IsEnabled);
    }

    private void ChangeInputMethod(InputMethod method)
    {
        _settings.CurrentMethod = method;
        _settings.Save();
        RustBridge.SetMethod(method);
    }

    private void ToggleEnabled(bool enabled)
    {
        _settings.IsEnabled = enabled;
        _settings.Save();
        RustBridge.SetEnabled(enabled);
    }

    private void OnHotkeyToggle(object? sender, EventArgs e)
    {
        _settings.IsEnabled = !_settings.IsEnabled;
        _settings.Save();
        RustBridge.SetEnabled(_settings.IsEnabled);
        _trayIcon?.UpdateState(_settings.CurrentMethod, _settings.IsEnabled);
    }

    private void ChangeModernTone(bool modern)
    {
        _settings.UseModernTone = modern;
        _settings.Save();
        RustBridge.SetModernTone(modern);
    }

    private void ChangeShortcutEnabled(bool enabled)
    {
        _settings.ShortcutEnabled = enabled;
        _settings.Save();
        // Note: Shortcut toggle FFI not yet implemented in Rust core
        // RustBridge.SetShortcutEnabled(enabled);
    }

    private void ChangeAutoStart(bool autoStart)
    {
        _settings.AutoStart = autoStart;
        _settings.Save();
    }

    private void ShowSettings()
    {
        var popup = new SettingsPopup(_settings);
        popup.OnMethodChanged += ChangeInputMethod;
        popup.OnModernToneChanged += ChangeModernTone;
        popup.OnShortcutChanged += ChangeShortcutEnabled;
        popup.OnAutoStartChanged += ChangeAutoStart;
        popup.ShowDialog();

        // Update tray after popup closes
        _trayIcon?.UpdateState(_settings.CurrentMethod, _settings.IsEnabled);
        _trayIcon?.UpdateToggles(_settings.UseModernTone, _settings.ShortcutEnabled);
    }

    private void ExitApplication()
    {
        _keyboardHook?.Stop();
        _keyboardHook?.Dispose();
        _trayIcon?.Dispose();
        RustBridge.Clear();
        _mutex?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _keyboardHook?.Dispose();
        _trayIcon?.Dispose();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
