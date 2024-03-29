using Godot;

public partial class LevelPortal : Area3D
{
    [Export] public string LevelToLoad;

    private LevelManager levelManager;

    public override void _Ready()
    {
        levelManager = GetNode<LevelManager>("/root/LevelManager");

        BodyEntered += OnBodyEnter;
    }

    public void OnBodyEnter(Node body)
    {
        if (!levelManager.IsTransitioning)
        {
            GD.Print("Level portal triggered");
            levelManager.ChangeLevel(LevelToLoad);
        }
    }

}

