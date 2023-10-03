using Godot;

public partial class PlayerSpawner : Node
{
    [Export] public PackedScene Player;

    public override void _Ready()
    {
        SceneManager.Instance.SceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName)
    {   
        string spFromPreviousScene = "SP_From_" + SceneManager.Instance.PreviousSceneName;
        string spCurrentScene = "SP_" + GetTree().CurrentScene.Name;

        Node3D _spawnpoint = (Node3D)GetTree().CurrentScene.GetNodeOrNull(spFromPreviousScene);
        if (_spawnpoint != null && _spawnpoint.IsInGroup("Spawnpoint"))
        {
            _spawnpoint = (Node3D)GetNode(spFromPreviousScene);
        }
        else
        {
            _spawnpoint = (Node3D)GetTree().CurrentScene.GetNodeOrNull(spCurrentScene);
            if (_spawnpoint != null && _spawnpoint.IsInGroup("Spawnpoint"))
            {
                _spawnpoint = (Node3D)GetNode(spCurrentScene);
            }
        }
        
        if (_spawnpoint == null)
        {
            GD.PrintErr("Spawn point not found.");
        }

        CharacterBody3D playerInstance = (CharacterBody3D)Player.Instantiate();
        playerInstance.GlobalPosition = _spawnpoint.GlobalPosition;
        AddChild(playerInstance);
    }
    
}

