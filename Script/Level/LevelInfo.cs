using Godot;

namespace Game
{
    public partial class LevelInfo : Resource
    {
        [Export] public bool AllowMenu = true;
        [Export] public bool FadeLevelTitleInOut = false;
        [Export] public bool HasEpigraph = false;
        [Export] public bool PlayerExistsInLevel = true;
        [Export] public string Environment = "wind";
        [Export] public string LevelName = "";
        [Export] public string LevelID = "";
        [Export] public string LevelPath = "";
        [Export(PropertyHint.MultilineText)] public string Epigraph = "";
    }
}

