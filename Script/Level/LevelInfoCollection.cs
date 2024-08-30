using Godot;
using System.Collections.Generic;

namespace Game
{
    public partial class LevelInfoCollection : Resource
    {
        private readonly static string levelDir = "res://Data/Level/";

        private Dictionary<string, LevelInfo> levelInfo;

        public Dictionary<string, LevelInfo> LevelInfo
        {
            get
            {
                if (levelInfo == null)
                {
                    levelInfo = new Dictionary<string, LevelInfo>();

                    using var dir = DirAccess.Open(levelDir);

                    if (dir != null)
                    {
                        dir.ListDirBegin();
                        string fileName = dir.GetNext();
                        while (fileName != "")
                        {
                            // DirAccess returns
                            //   in an exported build: dir/fileName.extension.import
                            //   in the editor: dir/fileName.extension
                            // In an exported build, ResourceLoader can load from the original path
                            fileName = fileName.Replace(".import", "");
                            fileName = fileName.Replace(".remap", "");
                            if (ResourceLoader.Exists(levelDir + fileName))
                            {
                                if (ResourceLoader.Load(levelDir + fileName) is LevelInfo _levelInfo)
                                {
                                    string levelInfoName = fileName.TrimSuffix(".tres");
                                    levelInfo[levelInfoName] = _levelInfo;
                                }
                            }

                            fileName = dir.GetNext();
                        }
                    }

                    GD.Print("[LevelInfoCollection] Level info has been initialized with ", levelInfo.Count, " levels");

                }

                return levelInfo;
            }
        }

    }

}

