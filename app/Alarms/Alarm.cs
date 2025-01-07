namespace MotionDetectorNet.Alarms;

public abstract class Alarm
{
    public abstract string Name { get; }

    public virtual bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Returns types of all alarms derived from "Alarm"
    /// </summary>
    /// <returns>List of types</returns>
    public static Type[] GetDescendantTypes() => System.Reflection.Assembly
        .GetExecutingAssembly()
        .GetTypes()
        .Where(type => type.IsSubclassOf(typeof(Alarm)))
        .ToArray();

    public abstract void Start();
    public abstract void Stop();
}
