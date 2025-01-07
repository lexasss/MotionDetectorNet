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

    public bool IsRunning { get; private set; }
    public double Level { get; private set; } = 0;
    public bool IsInMotion { get; private set; } = false;
    public ImageSource? Image { get; private set; } = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MotionDetectorModel(Dispatcher dispather,
        Camera[] cameras,
        MotionDetector motionDetector, 
        Alarm[] alarms)
    {
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
