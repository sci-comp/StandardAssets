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
        CharacterBody3D playerInstance = (CharacterBody3D)Player.Instantiate();

        playerInstance.Position = new(0,0,0);
        AddChild(playerInstance);
    }

}

