using Godot;
using System.Collections.Generic;

public partial class LevelInfoCollection : Resource
{
    [Export] public Godot.Collections.Array<LevelInfo> LevelInfoList = new();

    private Dictionary<string, LevelInfo> levelInfo;

    public Dictionary<string, LevelInfo> LevelInfo
    {
        get
        {
            if (levelInfo == null)
            {
                levelInfo = new Dictionary<string, LevelInfo>();
                for (int i = 0; i < LevelInfoList.Count; ++i)
                {
                    LevelInfo _sceneInfo = LevelInfoList[i];
                    if (!levelInfo.TryAdd(_sceneInfo.LevelName, _sceneInfo))
                    {
                        GD.PrintErr("Duplicate scene name found: " + _sceneInfo.LevelName);
                    }
                }
            }

            return levelInfo;
        }
    }
} 


