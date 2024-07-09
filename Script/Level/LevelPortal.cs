using Godot;

namespace Game
{
    public partial class LevelPortal : Area3D
    {
        [Export] public string LevelToLoad = "Sandbox";
        [Export] public string Spawnpoint = "SP_Sandbox";
        [Export] public string PortalEnteredSFX = "portal";

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
                levelManager.ChangeLevel(LevelToLoad, Spawnpoint);
            }
        }

    }

}

