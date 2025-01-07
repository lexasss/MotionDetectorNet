using DebounceThrottle;
using MotionDetectorNet.Alarms;
using OpenCvSharp.WpfExtensions;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Threading;

namespace MotionDetectorNet;

public class MotionDetectorModel : INotifyPropertyChanged
{
    public MotionDetector MotionDetector { get; }
    public Camera[] Cameras { get; }

    /// <summary>
    /// I do not like that this property duolicates Settings.CameraIndex,
    /// and therefore I have to pass Settings instance to MotionDetectorModel,
    /// but I need binding both CameraIndex and IsRunning in the same widget in the UI,
    /// and there is no way to use different DataContext withing the same widget.
    /// 
    /// Or is there?
    /// </summary>
    public int CameraIndex
    {
        get => _settings.CameraIndex;
        set
        {
            _settings.CameraIndex = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CameraIndex)));
        }
    }

    public bool IsRunning { get; private set; }
    public double Level { get; private set; } = 0;
    public bool IsInMotion { get; private set; } = false;
    public ImageSource? Image { get; private set; } = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MotionDetectorModel(Dispatcher dispather,
        Settings settings,
        Camera[] cameras,
        MotionDetector motionDetector, 
        Alarm[] alarms)
    {
        _settings = settings;
        _dispather = dispather;
        _alarms = alarms;
        MotionDetector = motionDetector;
        Cameras = cameras;

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

            if (IsInMotion)
            {
                _throttleDispatcher.Throttle(PlayAlarms);
            }
            else
            {
                TerminateAlarms();
            }
        });
    }

    // Internal

    readonly Settings _settings;
    readonly Dispatcher _dispather;
    readonly Alarm[] _alarms;

    readonly ThrottleDispatcher _throttleDispatcher = new(TimeSpan.FromSeconds(2));

    private void PlayAlarms()
    {
        foreach (var alarm in _alarms)
        {
            if (alarm.IsEnabled)
                alarm.Start();
        }
    }

    private void TerminateAlarms()
    {
        foreach (var alarm in _alarms)
        {
            if (alarm.IsEnabled)
                alarm.Stop();
        }
    }
}
