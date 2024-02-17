using Godot;

public partial class PlayerSpawner : Node
{
    [Export] public string PlayerPath = "res://Prefab/player.tscn";

    private PackedScene player;

    public Game Game;

    public override void _Ready()
    {
        LevelManager.Inst.LevelLoaded += OnLevelLoaded;
        player = GD.Load<PackedScene>(PlayerPath);
        Game = GetNode<Game>("..");
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

            Node playerInstance = player.Instantiate();
            Menu _menu = playerInstance.GetNode<Menu>("Menu");
            _menu.Initialize(Game);
            LevelManager.Inst.CurrentLevel.AddChild(playerInstance);

            // TODO
            GD.Print("TODO: set player position to _spawnpoint.GlobalPosition");
        }
    }
}

