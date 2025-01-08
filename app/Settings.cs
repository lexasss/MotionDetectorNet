using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace MotionDetectorNet;

public enum FrameType
{
    Original,
    Difference,
    Threshold
}

public class Settings : INotifyPropertyChanged
{
    /// <summary>
    /// Valid range is 0.000_001..0.5
    /// </summary>
    public double Threshold { get; set; } = 0.005;

    /// <summary>
    /// The smaller the value, the more sensitive the detector is.
    /// Valid range is 1..255
    /// </summary>
    public int Sensitivity { get; set; } = 25;

    public bool IsSavingMotionImages { get; set; } = false;

    public string LogFolder { get; private set; }

    public bool IsShowingCamera { get; set; } = false;

    public FrameType FrameType { get; set; } = FrameType.Original;

    /// <summary>
    /// Defines the share of previous motion level when computing the motion level
    /// for the current frame. Zero means "ignore previous motion level",
    /// one means "ignore new motion level". Valid range is 0..0.95
    /// </summary>
    public double MotionLevelDamp { get; set; } = 0.2;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Settings()
    {
        LogFolder = Path.Combine(".\\images", DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_ff"));
    }

    public static Settings Load()
    {
        Settings? result = null;

        var settingsJson = Properties.Settings.Default.SettingsAsJson;
        if (!string.IsNullOrEmpty(settingsJson))
        {
            try
            {
                result = JsonSerializer.Deserialize<Settings>(settingsJson);
            }
            catch
            {
                Properties.Settings.Default.SettingsAsJson = "";
                Properties.Settings.Default.Save();
            }
        }

        return result ?? new Settings();
    }

    public void Save()
    {
        Properties.Settings.Default.SettingsAsJson = JsonSerializer.Serialize(this);
        Properties.Settings.Default.Save();
    }

    public void SelectLogFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog()
        {
            Title = $"Select a folder to store {App.Name} log files",
            DefaultDirectory = !string.IsNullOrEmpty(LogFolder) ?
                LogFolder :
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        };

        if (dialog.ShowDialog() == true)
        {
            LogFolder = dialog.FolderName;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogFolder)));
        }
    }
}