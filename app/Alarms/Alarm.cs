using System.Text.Json;

namespace MotionDetectorNet.Alarms;

public abstract class Alarm
{
    public abstract string Name { get; }

    public virtual bool IsEnabled { get; set; } = false;

    public abstract void Start();
    public abstract void Stop();

    /// <summary>
    /// Returns istances of all alarms derived from <see cref="Alarm"/>
    /// </summary>
    public static Alarm[] Load()
    {
        var alarms = System.Reflection.Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(Alarm)))
            .Select(alarmType => (Alarm?)Activator.CreateInstance(alarmType))
            .Where(alarm => alarm != null)
            .Select(alarm => alarm!)
            .ToArray();

        var alarmsJson = Properties.Settings.Default.AlarmStates;
        if (!string.IsNullOrEmpty(alarmsJson))
        {
            try
            {
                var savedAlarmStates = JsonSerializer.Deserialize<Dictionary<string,bool>>(alarmsJson);
                if (savedAlarmStates != null)
                {
                    foreach (var alarm in alarms)
                    {
                        if (savedAlarmStates.TryGetValue(alarm.Name, out bool isEnabled))
                        {
                            alarm.IsEnabled = isEnabled;
                        }
                    }
                }
            }
            catch
            {
                Properties.Settings.Default.AlarmStates = "";
                Properties.Settings.Default.Save();
            }
        }

        return alarms;
    }

    public static void Save(Alarm[] alarms)
    {
        var alarmStates = alarms
            .Select(alarm => new KeyValuePair<string, bool>(alarm.Name, alarm.IsEnabled))
            .ToDictionary();
        Properties.Settings.Default.AlarmStates = JsonSerializer.Serialize(alarmStates);
        Properties.Settings.Default.Save();
    }
}
