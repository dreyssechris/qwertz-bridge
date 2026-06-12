using QwertzBridge.Core.Config;

namespace QwertzBridge.Infrastructure;

/// <summary>
/// Owns the JSON config file next to the executable (portable deployment): creates it
/// with defaults on first run, loads it, and raises <see cref="ConfigReloaded"/> when
/// the file changes on disk (hot reload, debounced).
/// </summary>
public sealed class ConfigStore : IDisposable
{
    private const string FileName = "qwertzbridge.json";
    private const int DebounceMilliseconds = 400;

    private readonly FileSystemWatcher _watcher;
    private readonly System.Threading.Timer _debounce;

    /// <summary>Creates the store; call <see cref="LoadOrCreate"/> then <see cref="StartWatching"/>.</summary>
    public ConfigStore()
    {
        ConfigPath = Path.Combine(AppContext.BaseDirectory, FileName);
        _debounce = new System.Threading.Timer(_ => Reload(), null, Timeout.Infinite, Timeout.Infinite);
        _watcher = new FileSystemWatcher(AppContext.BaseDirectory, FileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
        };
        _watcher.Changed += OnFileEvent;
        _watcher.Created += OnFileEvent;
        _watcher.Renamed += OnFileEvent;
    }

    /// <summary>Full path of the config file (next to the EXE).</summary>
    public string ConfigPath { get; }

    /// <summary>True if <see cref="LoadOrCreate"/> created the file (first run).</summary>
    public bool CreatedDefaultFile { get; private set; }

    /// <summary>Raised after a hot reload, on a background thread.</summary>
    public event Action<ConfigLoadResult>? ConfigReloaded;

    /// <summary>Loads the config file, creating it with defaults if it does not exist.</summary>
    public ConfigLoadResult LoadOrCreate()
    {
        if (!File.Exists(ConfigPath))
        {
            try
            {
                File.WriteAllText(ConfigPath, ConfigLoader.SerializeDefault());
                CreatedDefaultFile = true;
            }
            catch (IOException)
            {
                // Read-only location: run with in-memory defaults, no config file.
            }

            return ConfigLoader.Parse(ConfigLoader.SerializeDefault());
        }

        return ConfigLoader.Parse(ReadFileWithRetry());
    }

    /// <summary>Starts watching the config file for changes.</summary>
    public void StartWatching() => _watcher.EnableRaisingEvents = true;

    /// <inheritdoc />
    public void Dispose()
    {
        _watcher.Dispose();
        _debounce.Dispose();
    }

    private void OnFileEvent(object sender, FileSystemEventArgs e) =>
        _debounce.Change(DebounceMilliseconds, Timeout.Infinite);

    private void Reload()
    {
        try
        {
            var result = File.Exists(ConfigPath)
                ? ConfigLoader.Parse(ReadFileWithRetry())
                : ConfigLoader.Parse("");
            ConfigReloaded?.Invoke(result);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // File is gone or locked for good; keep the current config.
        }
    }

    private string ReadFileWithRetry()
    {
        // Editors often hold a short-lived lock while saving; retry briefly.
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                return File.ReadAllText(ConfigPath);
            }
            catch (IOException) when (attempt < 4)
            {
                Thread.Sleep(50);
            }
        }
    }
}
