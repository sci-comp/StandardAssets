using Godot;

public partial class Singleton : Node
{
    private static Singleton instance = null;

    public static Singleton Instance
    {
        get
        {
            return instance;
        }
    }

    public override void _Ready()
    {
        if (instance != null)
        {
            QueueFree();
            return;
        }
        instance = this;
        GetTree().Root.AddChild(this);
    }
}
