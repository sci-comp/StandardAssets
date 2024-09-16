using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game
{
    public partial class LevelManager : Node
    {
        [Export] public float FadeSpeedScale = 1f;
        [Export] public string LevelDir = "res://Data/Level/";
        [Export] public string LevelInfoCollectionPath = "res://Data/LevelInfoCollection.tres";
        [Export] public Color FadeToColor = new("#000000");

        private AnimationPlayer animationPlayer;
        private ColorRect colorRect;
        private Node spawnpoints;
        private readonly object levelChangeLock = new();
        private readonly Dictionary<string, LevelInfo> levelInfo = new();

        public bool IsTransitioning { get; set; } = false;
        public string PreviousLevelName { get; set; } = "";
        
        public Node CurrentLevel { get; set; }
        public Node SceneTree { get; set; }
        public Node SceneTreeRoot { get; set; }
        public string CurrentLevelName => CurrentLevel.Name;

        public event Action LevelLoaded;
        public event Action BeginUnloadingLevel;
        public event Action FadeInComplete;
        public event Action FadeOutComplete;

        public LevelInfo CurrentLevelInfo
        {
            get
            {
                if (levelInfo.ContainsKey(CurrentLevel.Name))
                {
                    return levelInfo[CurrentLevel.Name];
                }
                else
                {
                    GD.PrintErr("[LevelManager] Level name does not exist in the level info key ring: ", CurrentLevel.Name);
                    return null;
                }
            }
        }

        public override void _Ready()
        {
            var SceneTree = GetTree();
            SceneTreeRoot = SceneTree.Root;
            CurrentLevel = SceneTree.CurrentScene;

            if (CurrentLevel.HasNode("Spawnpoints"))
            {
                spawnpoints = CurrentLevel.GetNode("Spawnpoints");
            }

            LoadLevelInformation();

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            colorRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
            
            GD.Print("[LevelManager] Ready");
        }

        public void ChangeLevel(string levelName)
        {
            LevelInfo _info = levelInfo[levelName];

            IsTransitioning = true;  // Must set this here since ChangeSceneNow is async
            CallDeferred(nameof(ChangeSceneNow), _info.Path);
        }

        public async void ChangeSceneNow(string path)
        {
            if (path == null)
            {
                GD.PrintErr("[LevelManager] Scene path is null");
                IsTransitioning = false;
                return;
            }

            lock (levelChangeLock)
            {
                if (CurrentLevel != null && CurrentLevel.SceneFilePath == path)
                {
                    GD.PrintErr("[LevelManager] Blocking attempt to transition to the already loaded scene");
                    return;
                }

                IsTransitioning = true;  // Should already be true
            }

            if (ResourceLoader.Load(path, "PackedScene", 0) is not PackedScene nextLevel)
            {
                GD.PrintErr("[LevelManager] Invalid level path: ", path);
                IsTransitioning = false;
                return;
            }

            GD.Print("[LevelManager] Unloading level: " + CurrentLevel.Name);
            BeginUnloadingLevel?.Invoke();
            PreviousLevelName = CurrentLevel.Name;
            await FadeOut();
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            CurrentLevel?.CallDeferred("queue_free");
            await ToSignal(CurrentLevel, "tree_exited");
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            CurrentLevel = nextLevel.Instantiate();
            SceneTreeRoot.AddChild(CurrentLevel);
            GD.Print("[LevelManager] Level loaded: " + CurrentLevel.Name);
            LevelLoaded?.Invoke();

            await FadeIn();

            IsTransitioning = false;
        }

        public async Task FadeOut()
        {
            animationPlayer.SpeedScale = FadeSpeedScale;
            animationPlayer.Play("FadeToBlack");
            await ToSignal(animationPlayer, "animation_finished");
            FadeOutComplete?.Invoke();
        }

        public async Task FadeIn()
        {
            animationPlayer.SpeedScale = FadeSpeedScale;
            animationPlayer.PlayBackwards("FadeToBlack");
            await ToSignal(animationPlayer, "animation_finished");
            FadeInComplete?.Invoke();
        }

        public void LoadLevelInformation()
        {
            DirAccess dir = DirAccess.Open(LevelDir);

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
                    if (ResourceLoader.Exists(LevelDir + fileName))
                    {
                        if (ResourceLoader.Load(LevelDir + fileName) is LevelInfo _levelInfo)
                        {
                            string levelInfoName = fileName.TrimSuffix(".tres");
                            levelInfo[levelInfoName] = _levelInfo;
                        }
                    }

                    fileName = dir.GetNext();
                }
            }
            else
            {
                GD.PrintErr("[LevelDatabase] Level information directory is missing: ", LevelDir);
            }

            GD.Print("[LevelDatabase] Ready with information on ", "0", " levels");
        }

        public LevelInfo GetLevelInfo(string levelName)
        {
            return levelInfo[levelName];
        }

    }

}

