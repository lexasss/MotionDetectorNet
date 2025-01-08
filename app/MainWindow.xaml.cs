using System.ComponentModel;
using System.Windows;
using MotionDetectorNet.Alarms;

namespace MotionDetectorNet;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public ViewModels.MotionDetector MotionDetector { get; }
    public ViewModels.Camera Camera { get; }

    public Settings Settings { get; } = Settings.Load();
    public Alarm[] Alarms { get; } = Alarm.Load();

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();

        _motionDetector = new MotionDetector(Settings);

        MotionDetector = new ViewModels.MotionDetector(Dispatcher, _motionDetector, Alarms);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MotionDetector)));

        Camera = new ViewModels.Camera();
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Camera)));

        Title = App.Name;

        //WindowTools.HideWindowMinMaxButtons(this);
    }

    // Internal

    readonly MotionDetector _motionDetector;

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _motionDetector.Dispose();
        Settings.Save();
        Camera.Save();
        Alarm.Save(Alarms);
    }

    private void StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (_motionDetector.IsRunning)
        {
            _motionDetector.Stop();
        }
        else if (Camera.CameraIndex >= 0)
        {
            if (!_motionDetector.Start(Camera.Items[Camera.CameraIndex].ID))
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