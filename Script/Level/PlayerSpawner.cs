using Godot;

public partial class PlayerSpawner : Node
{
    [Export] public string PlayerPath = "res://Prefab/player.tscn";

    private PackedScene player;

    public override void _Ready()
    {
        LevelManager.Inst.LevelLoaded += OnLevelLoaded;
        player = GD.Load<PackedScene>(PlayerPath);
        if (player != null)
        {
            GD.Print("PlayerSpawner: Player loaded.");
        }
        else
        {
            GD.PrintErr("PlayerSpawner: Player not loaded.");
        }   
    }

    private void OnLevelLoaded()
    {
        GD.Print("PlayerSpawner: OnLevelLoaded");
        if (LevelManager.Inst.CurrentLevelInfo != null && LevelManager.Inst.CurrentLevelInfo.PlayerExistsInLevel)
        {
            string spFromPreviousLevel = "SP_From_" + LevelManager.Inst.PreviousLevelName;
            Node3D _spawnpoint = (Node3D) LevelManager.Inst.CurrentLevel.FindChild(spFromPreviousLevel);

            if (_spawnpoint == null)
            {
                string spCurrentLevel = "SP_" + LevelManager.Inst.CurrentLevelName;
                GD.Print("spCurrent: " + spCurrentLevel);
                _spawnpoint = (Node3D) LevelManager.Inst.CurrentLevel.FindChild(spCurrentLevel);
            }

            if (_spawnpoint == null)
            {
                GD.PrintErr("Spawn point not found.");
            }

            Node3D playerInstance = (Node3D) player.Instantiate();
            LevelManager.Inst.CurrentLevel.AddChild(playerInstance);
            playerInstance.GlobalPosition = _spawnpoint.GlobalPosition;
        }
    }
}

