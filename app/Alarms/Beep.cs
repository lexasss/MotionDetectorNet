using System.Media;

namespace MotionDetectorNet.Alarms;

public class Beep : Alarm
{
    public override string Name => "Beep";

    public override void Start() => SystemSounds.Beep.Play();

    public override void Stop() { }
}
