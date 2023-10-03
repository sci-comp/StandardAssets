using Godot;
using System.Collections.Generic;

public partial class SceneSpecificInformationCollection : Resource
{
    [Export] public Godot.Collections.Array SceneInfoList = new();

    private Dictionary<string, SceneSpecificInformation> sceneInfo;

    public Dictionary<string, SceneSpecificInformation> SceneInfo
    {
        get
        {
            if (sceneInfo == null)
            {
                sceneInfo = new Dictionary<string, SceneSpecificInformation>();
                for (int i = 0; i < SceneInfoList.Count; ++i)
                {
                    SceneSpecificInformation _sceneInfo = (SceneSpecificInformation)SceneInfoList[i];
                    if (!sceneInfo.TryAdd(_sceneInfo.SceneName, _sceneInfo))
                    {
                        GD.PrintErr("Duplicate scene name found: " + _sceneInfo.SceneName);
                    }
                }
            }

            return sceneInfo;
        }
    }
}

