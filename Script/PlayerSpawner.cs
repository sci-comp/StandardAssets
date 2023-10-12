using Godot;

public partial class PlayerSpawner : Node
{
    [Export] public string PlayerPath = "res://Game/Data/Player.tscn";
    private PackedScene Player;

    public override void _Ready()
    {
        SceneManager.Instance.SceneLoaded += OnSceneLoaded;
        Player = GD.Load<PackedScene>(PlayerPath);
    }

    private void OnSceneLoaded()
    {
        GD.Print("Scene loaded: " + SceneManager.Instance.CurrentSceneName);

        if (SceneManager.Instance.CurrentSceneInfo.PlayerExistsInScene)
        {
            string spFromPreviousScene = "SP_From_" + SceneManager.Instance.PreviousSceneName;
            Node3D _spawnpoint = (Node3D)SceneManager.Instance.CurrentScene.FindChild(spFromPreviousScene);

            if (_spawnpoint == null)
            {
                string spCurrentScene = "SP_" + SceneManager.Instance.CurrentSceneName;
                _spawnpoint = (Node3D)SceneManager.Instance.CurrentScene.FindChild(spCurrentScene);
            }

            if (_spawnpoint == null)
            {
                GD.PrintErr("Spawn point not found.");
            }

            Node3D playerInstance = (Node3D)Player.Instantiate();
            SceneManager.Instance.CurrentScene.AddChild(playerInstance);
            playerInstance.GlobalPosition = _spawnpoint.GlobalPosition;
        }
    }
}

