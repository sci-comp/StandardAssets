using Godot;

public partial class LevelPortal : Area3D
{
    [Export] public string LevelToLoad;

    public override void _Ready()
    {
        BodyEntered += OnBodyEnter;
    }

    public void OnBodyEnter(Node body)
    {
        GD.Print("Portal Collision");
        if (!LevelManager.Inst.IsTransitioning)
        {
            GD.Print("Player entered portal");
            LevelManager.Inst.ChangeLevel(LevelToLoad);
        }
    }

}

