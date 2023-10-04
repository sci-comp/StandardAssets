using Godot;

public partial class PlayerSpawner : Node
{
    [Export] public PackedScene Player;

    public override void _Ready()
    {
        SceneManager.Instance.SceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded()
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

        CharacterBody3D playerInstance = (CharacterBody3D)Player.Instantiate();
        SceneManager.Instance.CurrentScene.AddChild(playerInstance);
        playerInstance.GlobalPosition = _spawnpoint.GlobalPosition;
    }
}

