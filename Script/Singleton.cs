using Godot;

public partial class Singleton<T> : Node where T : Node
{
    private static T instance = null;

    public static T Instance
    {
        get
        {        
            return instance;
        }
    }

    public Singleton()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else if (instance != this)
        {
            QueueFree();
        }
    }
}
