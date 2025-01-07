using System.ComponentModel;
using System.Windows;
using MotionDetectorNet.Alarms;

namespace MotionDetectorNet;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public MotionDetectorModel MotionDetectorModel { get; }
    public Settings Settings { get; } = Settings.Load();
    public Alarm[] Alarms { get; } = Alarm.Load();

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();

        _motionDetector = new MotionDetector(Settings);
        _cameras = OpenCVDeviceEnumerator.EnumerateCameras();

        MotionDetectorModel = new MotionDetectorModel(Dispatcher, Settings, _cameras, _motionDetector, Alarms);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MotionDetectorModel)));

        Title = App.Name;

        if (_cameras.Length > 0)
            MotionDetectorModel.CameraIndex = Math.Min(_cameras.Length - 1, Settings.CameraIndex);

        //WindowTools.HideWindowMinMaxButtons(this);
    }

    readonly MotionDetector _motionDetector;
    readonly Camera[] _cameras;

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _motionDetector.Dispose();
        Settings.Save();
        Alarm.Save(Alarms);
    }

    private void StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (_motionDetector.IsRunning)
        {
            _motionDetector.Stop();
        }
        else if (Settings.CameraIndex >= 0)
        {
            if (!_motionDetector.Start(_cameras[Settings.CameraIndex].ID))
            {
                MessageBox.Show("Cannot start video stream", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SelectFolder_Click(object sender, RoutedEventArgs e)
    {
        Settings.SelectLogFolder();
    }
}