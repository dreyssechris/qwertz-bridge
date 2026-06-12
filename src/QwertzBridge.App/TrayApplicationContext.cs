using System.Diagnostics;
using QwertzBridge.Core.Config;
using QwertzBridge.Core.Domain;
using QwertzBridge.Core.Engine;
using QwertzBridge.Infrastructure;

namespace QwertzBridge.App;

/// <summary>
/// Wires everything together: config store, engine, keyboard hook, SendInput,
/// and exposes control via the tray icon.
/// </summary>
internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly ConfigStore _configStore = new();
    private readonly RemapEngine _engine;
    private readonly LowLevelKeyboardHook _hook = new();
    private readonly SendInputTextOutput _output = new();
    private readonly AutostartManager _autostart = new();

    private readonly NotifyIcon _trayIcon;
    private readonly Icon _activeIcon = TrayIcons.Create(active: true);
    private readonly Icon _inactiveIcon = TrayIcons.Create(active: false);
    private readonly ToolStripMenuItem _profileItem;
    private readonly ToolStripMenuItem _enabledItem;
    private readonly ToolStripMenuItem _autostartItem;

    // Invisible control whose handle lets background threads marshal onto the UI thread.
    private readonly Control _uiMarshal = new();

    public TrayApplicationContext()
    {
        _uiMarshal.CreateControl();

        var load = _configStore.LoadOrCreate();
        _engine = new RemapEngine(load.Config, new ForegroundProcessProvider());

        _hook.Handler = OnKeyEvent;
        _hook.Install();

        _autostart.SyncPathIfEnabled();

        _profileItem = new ToolStripMenuItem { Enabled = false };
        _enabledItem = new ToolStripMenuItem("Enabled", null, (_, _) => ToggleEnabled());
        _autostartItem = new ToolStripMenuItem("Start with Windows", null, (_, _) => ToggleAutostart());
        var menu = new ContextMenuStrip();
        menu.Items.Add(_profileItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_enabledItem);
        menu.Items.Add(_autostartItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Open configuration", null, (_, _) => OpenConfig()));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitApplication()));
        menu.Opening += (_, _) => UpdateStatus();

        _trayIcon = new NotifyIcon
        {
            Icon = _activeIcon,
            ContextMenuStrip = menu,
            Visible = true,
        };
        _trayIcon.DoubleClick += (_, _) => ToggleEnabled();
        UpdateStatus();

        _configStore.ConfigReloaded += OnConfigReloaded;
        _configStore.StartWatching();

        if (_configStore.CreatedDefaultFile)
        {
            _trayIcon.ShowBalloonTip(8000, "QWERTZ-Bridge is active",
                "AltGr + ,  →  <\nAltGr + .  →  >\nAltGr + -  →  |", ToolTipIcon.Info);
        }
        else if (load.UsedFallback)
        {
            ShowConfigError(load.Error);
        }
    }

    private bool OnKeyEvent(KeyInput input)
    {
        var decision = _engine.ProcessKey(input);
        if (decision.Output is { Length: > 0 })
            _output.Send(decision.Output, _engine.IsAltGrActive);
        return decision.Suppress;
    }

    private void OnConfigReloaded(ConfigLoadResult result)
    {
        _engine.UpdateConfig(result.Config);
        _uiMarshal.BeginInvoke(() =>
        {
            UpdateStatus();
            if (result.UsedFallback)
                ShowConfigError(result.Error);
        });
    }

    private void ShowConfigError(string? error) =>
        _trayIcon.ShowBalloonTip(8000, "QWERTZ-Bridge: invalid configuration",
            $"{error}\nThe built-in defaults are active.", ToolTipIcon.Warning);

    private void ToggleEnabled()
    {
        _engine.Enabled = !_engine.Enabled;
        UpdateStatus();
    }

    private void ToggleAutostart()
    {
        if (_autostart.IsEnabled)
            _autostart.Disable();
        else
            _autostart.Enable();
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var enabled = _engine.Enabled;
        _trayIcon.Icon = enabled ? _activeIcon : _inactiveIcon;
        _trayIcon.Text = $"QWERTZ-Bridge ({(enabled ? "active" : "paused")})";
        _profileItem.Text = $"Profile: {_engine.ActiveProfileName}";
        _enabledItem.Checked = enabled;
        _autostartItem.Checked = _autostart.IsEnabled;
    }

    private void OpenConfig()
    {
        try
        {
            Process.Start(new ProcessStartInfo(_configStore.ConfigPath) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _trayIcon.ShowBalloonTip(5000, "QWERTZ-Bridge",
                $"Could not open the configuration file:\n{ex.Message}", ToolTipIcon.Error);
        }
    }

    private void ExitApplication()
    {
        _trayIcon.Visible = false;
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hook.Dispose();
            _configStore.Dispose();
            _trayIcon.Dispose();
            _activeIcon.Dispose();
            _inactiveIcon.Dispose();
            _uiMarshal.Dispose();
        }

        base.Dispose(disposing);
    }
}
