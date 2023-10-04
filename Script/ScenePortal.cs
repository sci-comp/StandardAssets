using Godot;

public partial class ScenePortal : Area3D
{
    [Export] public string SceneToLoad;

    public override void _Ready()
    {
        BodyEntered += OnPlayerEnter;
    }

    public void OnPlayerEnter(Node body)
    {
        GD.Print("Portal entered");
        if (!SceneManager.Instance.IsTransitioning && body.IsInGroup("player"))
        {
            SceneManager.Instance.ChangeScene(SceneToLoad);
        }
    }

}

