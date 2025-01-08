using System.ComponentModel;
using MotionDetectorNet.Helpers;

namespace MotionDetectorNet.ViewModels;

public class Camera : INotifyPropertyChanged
{
    public Helpers.Camera[] Items { get; } = OpenCVDeviceEnumerator.EnumerateCameras();
    public int CameraIndex
    { 
        get => _cameraIndex;
        set
        {
            _cameraIndex = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraIndex)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCameraName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCameraSelected)));
        }
    }
    public string? SelectedCameraName => CameraIndex < Items.Length ? Items[CameraIndex].Name : null;
    public bool IsCameraSelected => _cameraIndex != -1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public Camera()
    {
        var usedCameraName = Properties.Settings.Default.UsedCameraName;

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].Name == usedCameraName)
            {
                CameraIndex = i;
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
