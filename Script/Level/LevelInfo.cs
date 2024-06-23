using Godot;

namespace Game
{
    public partial class LevelInfo : Resource
    {
        [Export] public string LevelName;
        [Export] public bool PlayerExistsInLevel;
        [Export] public string Path;
        [Export] public string Environment;
    }

}

