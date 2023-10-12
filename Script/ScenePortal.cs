using Godot;

public partial class ScenePortal : Area3D
{
    [Export] public string SceneToLoad;

    public override void _Ready()
    {
        BodyEntered += OnBodyEnter;
    }

    public void OnBodyEnter(Node body)
    {
        GD.Print("Portal Collision");
        if (!SceneManager.Instance.IsTransitioning && body.IsInGroup("player"))
        {
            GD.Print("Player entered portal");
            SceneManager.Instance.ChangeScene(SceneToLoad);
        }
    }

}

