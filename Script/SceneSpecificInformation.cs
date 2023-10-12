using Godot;


[GlobalClass]
public partial class SceneSpecificInformation : Resource
{
    [Export] public string SceneName;
    [Export] public bool PlayerExistsInScene;
    [Export] public string Path;
}

