using Godot;

public partial class PlayerSpawner : Node
{
    //[Export] public PackedScene Player;
    [Export] public string PlayerPath = "res://Game/Scene/player.tscn";
    private PackedScene Player;

    public override void _Ready()
    {
        LevelManager.Inst.LevelLoaded += OnLevelLoaded;
        Player = GD.Load<PackedScene>(PlayerPath);
    }

    private void OnLevelLoaded()
    {
        GD.Print("Level loaded: " + LevelManager.Inst.CurrentLevelInfo.Path);

        if (LevelManager.Inst.CurrentLevelInfo.PlayerExistsInLevel)
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

            Node3D playerInstance = (Node3D) Player.Instantiate();
            LevelManager.Inst.CurrentLevel.AddChild(playerInstance);
            playerInstance.GlobalPosition = _spawnpoint.GlobalPosition;
        }
    }
}

