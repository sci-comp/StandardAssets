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

    public override void _Ready()
    {
        base._Ready();

        if (instance != null)
        {
            QueueFree();
            return;
        }

        if (this is T)
        {
            instance = this as T;
            GetTree().Root.AddChild(this);
        }
    }
}
