using System.ComponentModel;
using MotionDetectorNet.Helpers;

namespace MotionDetectorNet.ViewModels;

public class Camera : INotifyPropertyChanged
{
    public Helpers.Camera[] Items { get; } = OpenCVDeviceEnumerator.EnumerateCameras();
    public int SeleectedIndex
    {
        get => _cameraIndex;
        set
        {
            _cameraIndex = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SeleectedIndex)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelectedItem)));
        }
    }
    public bool HasSelectedItem => _cameraIndex != -1;

    public string? SelectedCameraName => SeleectedIndex >= 0 && SeleectedIndex < Items.Length ? Items[SeleectedIndex].Name : null;
    public int SelectedCameraID => SeleectedIndex < Items.Length ? Items[SeleectedIndex].ID : -1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Camera()
    {
        var usedCameraName = Properties.Settings.Default.UsedCameraName;

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].Name == usedCameraName)
            {
                SeleectedIndex = i;
                break;
            }
        }
    }

    public void Save()
    {
        if (!string.IsNullOrEmpty(SelectedCameraName))
        {
            Properties.Settings.Default.UsedCameraName = SelectedCameraName;
            Properties.Settings.Default.Save();
        }
    }

    // Internal

    int _cameraIndex = -1;
}
