using OpenCvSharp;
using System.IO;

namespace MotionDetectorNet;

public class MotionDetector : IDisposable
{
    public class FrameReceivedEventArgs(Mat frame, double level, bool isInMotion) : EventArgs
    {
        public Mat Frame => frame;
        public double Level => level;
        public bool IsInMotion => isInMotion;
    }
    public class ActivityChangedEventArgs(bool isRunning) : EventArgs
    {
        public bool IsRunning => isRunning;
    }

    public event EventHandler<FrameReceivedEventArgs>? FrameReceived;
    public event EventHandler<ActivityChangedEventArgs>? ActivityChanged;

    public bool IsRunning => _capture != null;

    public MotionDetector(Settings settings)
    {
        _settings = settings;
    }

    public bool Start(int cameraIndex = 0)
    {
        if (_capture != null)
            return false;

        _capture = new VideoCapture(cameraIndex);
        if (!_capture.IsOpened())
        {
            _capture.Release();
            _capture = null;
            return false;
        }

        _motionLevel = 0;
        _matrixSize = _capture.FrameHeight * _capture.FrameWidth * 255;

        PrepareOutput();

        _isBreakRequested = false;
        Task.Run(ProcessFrames);

        ActivityChanged?.Invoke(this, new ActivityChangedEventArgs(true));

        return true;
    }

    public void Stop()
    {
        if (_capture != null)
        {
            _isBreakRequested = true;
        }

        // We need to be sure that capturing has finished before deleting empty folder
        ShutdownCapture(1000);

        if (Directory.Exists(_settings.LogFolder))
        {
            var dir = new DirectoryInfo(_settings.LogFolder);
            if (dir.GetFiles().Length == 0)
            {
                dir.Delete();
            }
        }
    }

    public void Dispose()
    {
        //Cv2.DestroyAllWindows();
        Stop();
        GC.SuppressFinalize(this);
    }

    // Internal

    readonly Settings _settings;

    double _motionLevel = 0;
    int _matrixSize = 1;

    bool _hasMotion = false;
    int _motionFrameId = 0;
    int _imageId = 0;

    VideoCapture? _capture;
    bool _isBreakRequested = false;

    private void ProcessFrames()
    {
        var frame = new Mat();
        var prevFrame = new Mat();
        var diffFrame = new Mat();

        while (!_isBreakRequested)
        {
            _capture?.Read(frame);

            if (frame.Empty())
                break;

            if (!prevFrame.Empty())
            {
                Cv2.Absdiff(frame, prevFrame, diffFrame);
                Mat thresholdFrame = diffFrame.Clone();

                Cv2.CvtColor(thresholdFrame, thresholdFrame, ColorConversionCodes.BGR2GRAY);
                Cv2.Threshold(thresholdFrame, thresholdFrame, _settings.Sensitivity, 255, ThresholdTypes.Binary);
                double motionLevel = Cv2.Sum(thresholdFrame).Val0 / _matrixSize;

                _motionLevel = _motionLevel * _settings.MotionLevelDamp + motionLevel * (1.0 - _settings.MotionLevelDamp);

                bool isInMotion = _motionLevel >= _settings.Threshold;

                TryToSaveImage(frame, isInMotion);

                var frameToSend = _settings.FrameType switch
                {
                    FrameType.Difference => diffFrame,
                    FrameType.Threshold => thresholdFrame,
                    _ => frame
                };

                FrameReceived?.Invoke(this, new FrameReceivedEventArgs(frameToSend, _motionLevel, isInMotion));
            }

            frame.CopyTo(prevFrame);
            Cv2.WaitKey(30);
        }

        ShutdownCapture();

        ActivityChanged?.Invoke(this, new ActivityChangedEventArgs(false));
    }

    private void PrepareOutput()
    {
        if (!Directory.Exists(_settings.LogFolder))
        {
            Directory.CreateDirectory(_settings.LogFolder);
        }
        else
        {
            var dir = new DirectoryInfo(_settings.LogFolder);
            foreach (var fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }
    }

    private void TryToSaveImage(Mat frame, bool isInMotion)
    {
        if (isInMotion)
        {
            if (!_hasMotion)
            {
                _motionFrameId = 0;
                _hasMotion = true;
            }

            if ((_motionFrameId++ % 30) == 0)
            {
                if (_settings.IsSavingMotionImages)
                {
                    var file = Path.Combine(_settings.LogFolder, $"picture_{_imageId++}.jpg");
                    frame.SaveImage(file);
                }
            }
        }
        else if (_hasMotion)
        {
            _hasMotion = false;
        }
    }

    private void ShutdownCapture(int waitIntervalBeforeShutdown = 0)
    {
        if (waitIntervalBeforeShutdown > 0)
        {
            int delay = 0;
            while (_capture != null && delay < waitIntervalBeforeShutdown)
            {
                Thread.Sleep(10);
                delay += 10;
            }
        }

        if (_capture != null)
        {
            lock (_capture)
            {
                _capture?.Release();
                _capture = null;
            }
        }
    }
}
