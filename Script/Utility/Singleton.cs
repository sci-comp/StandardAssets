using Godot;

public partial class Singleton<T> : Node where T : Node
{
    private static T inst = null;

    public static T Inst
    {
        get
        {        
            return inst;
        }
    }

    public Singleton()
    {
        if (inst == null)
        {
            inst = this as T;
        }
        else if (inst != this)
        {
            QueueFree();
        }
    }
}
