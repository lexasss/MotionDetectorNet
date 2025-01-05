using OpenCvSharp.WpfExtensions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace MotionDetectorNet;

public class MotionDetectorModel : INotifyPropertyChanged
{
    public MotionDetector MotionDetector { get; }

    public ObservableCollection<OpenCVDeviceEnumerator.Camera> Cameras { get; }
    public bool IsRunning { get; private set; }
    public double Level { get; private set; } = 0;
    public bool IsInMotion { get; private set; } = false;
    public ImageSource? Image { get; private set; } = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MotionDetectorModel(Dispatcher dispather, MotionDetector motionDetector, OpenCVDeviceEnumerator.Camera[] cameras)
    {
        _dispather = dispather;
        MotionDetector = motionDetector;
        Cameras = new ObservableCollection<OpenCVDeviceEnumerator.Camera>(cameras);

        MotionDetector.ActivityChanged += (s, e) => _dispather.Invoke(() =>
        {
            IsRunning = e.IsRunning;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));

            if (!IsRunning)
            {
                Level = 0;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));

                IsInMotion = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInMotion)));

                Image = new OpenCvSharp.Mat(480, 640, OpenCvSharp.MatType.CV_8UC3, OpenCvSharp.Scalar.White).ToBitmapSource();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        });

        MotionDetector.FrameReceived += (s, e) => _dispather.Invoke(() =>
        {
            Level = e.Level;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));

            IsInMotion = e.IsInMotion;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInMotion)));

            Image = e.Frame.ToBitmapSource();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
        });
    }

    // Internal

    readonly Dispatcher _dispather;
}
