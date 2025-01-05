using System.Windows;

namespace MotionDetectorNet;

public partial class MainWindow : Window
{
    public MotionDetectorModel MotionDetectorModel => _motionDetectorModel;
    public Settings Settings => _settings;

    public MainWindow()
    {
        InitializeComponent();

        MotionDetector motionDetector = new MotionDetector(_settings);

        _cameras = _deviceEnumerator.EnumerateCameras();

        _motionDetectorModel = new MotionDetectorModel(Dispatcher, motionDetector, _cameras);

        DataContext = this;
        Title = App.Name;

        if (_cameras.Length > 0)
            cmbCameras.SelectedIndex = 0;

        //WindowTools.HideWindowMinMaxButtons(this);
    }

    // Internal

    readonly MotionDetectorModel _motionDetectorModel;
    readonly Settings _settings = Settings.Load();
    readonly OpenCVDeviceEnumerator _deviceEnumerator = new();

    OpenCVDeviceEnumerator.Camera[] _cameras = [];

    private void Window_Closed(object sender, EventArgs e)
    {
        _settings.Save();
    }

    private void StartStop_Click(object sender, RoutedEventArgs e)
    {
        if (_motionDetectorModel.IsRunning)
        {
            _motionDetectorModel.MotionDetector.Stop();
        }
        else
        {
            if (!_motionDetectorModel.MotionDetector.Start(_cameras[cmbCameras.SelectedIndex].ID))
            {
                MessageBox.Show("Cannot start video stream", Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void SelectFolder_Click(object sender, RoutedEventArgs e)
    {
        _settings.SelectLogFolder();
    }
}