namespace MotionDetectorNet.Camera;

public class Camera(int id, string name)
{
    public int ID => id;
    public string Name => name;
    public override string ToString() => name;
}
