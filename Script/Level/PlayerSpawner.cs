using Godot;

public partial class PlayerSpawner : Node
{
    [Export] public string PlayerPath = "res://Prefab/Player.tscn";

    private LevelManager levelManager;
    private PackedScene player;

    public override void _Ready()
    {
        levelManager = GetNode<LevelManager>("/root/LevelManager");

        levelManager.LevelLoaded += OnLevelLoaded;
        player = GD.Load<PackedScene>(PlayerPath);

        if (player == null)
        {
            GD.Print("player is null");
        }

        if (levelManager == null)
        {
            GD.Print("levelManager is null");
        }
    }

    private void OnLevelLoaded()
    {
        if (levelManager.CurrentLevelInfo != null && levelManager.CurrentLevelInfo.PlayerExistsInLevel)
        {
            // Find spawnpoint
            string spFromPreviousLevel = "SP_From_" + levelManager.PreviousLevelName;
            Node3D _spawnpoint = (Node3D) levelManager.CurrentLevel.FindChild(spFromPreviousLevel);
            if (_spawnpoint == null)
            {
                string spCurrentLevel = "SP_" + levelManager.CurrentLevel.Name;
                _spawnpoint = (Node3D) levelManager.CurrentLevel.FindChild(spCurrentLevel);
            }

            // Instantiate player
            if (_spawnpoint == null)
            {
                GD.PrintErr("No spawnpoint found in level: " + levelManager.CurrentLevel.Name);
            }
            else
            {
                GD.Print("Instantiating player at spawnpoint: " + _spawnpoint.Name);
                CharacterBody3D playerInstance = (CharacterBody3D)player.Instantiate();
                levelManager.CurrentLevel.AddChild(playerInstance);
                playerInstance.Position = _spawnpoint.Position;
            }
        }
    }

}

