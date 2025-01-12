using System.ComponentModel;
using MotionDetectorNet.Camera;

namespace MotionDetectorNet.ViewModels;

public class Camera : INotifyPropertyChanged
{
    public MotionDetectorNet.Camera.Camera[] Items { get; } = DeviceEnumerator.Get();
    public int SelectedIndex
    {
        get => _cameraIndex;
        set
        {
            _cameraIndex = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndex)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSelectedItem)));
        }
    }
    public bool HasSelectedItem => _cameraIndex != -1;

    public string? SelectedCameraName => SelectedIndex >= 0 && SelectedIndex < Items.Length ? Items[SelectedIndex].Name : null;
    public int SelectedCameraID => SelectedIndex < Items.Length ? Items[SelectedIndex].ID : -1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Camera()
    {
        var usedCameraName = Properties.Settings.Default.UsedCameraName;

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].Name == usedCameraName)
            {
                SelectedIndex = i;
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
