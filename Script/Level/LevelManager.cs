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
        private readonly object levelChangeLock = new();
        private readonly Dictionary<string, LevelInfo> levelInfo = new();

        public bool IsTransitioning { get; set; } = false;
        public string PreviousLevelName { get; set; } = "";
        
        public Node CurrentLevel { get; set; }
        public Node SceneTree { get; set; }
        public Node SceneTreeRoot { get; set; }
        public string CurrentLevelName => CurrentLevel.Name;

        public event Action LevelLoaded;
        public event Action<string, string> BeginUnloadingLevel;
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

            LoadLevelInformation();

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            colorRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
            
            GD.Print("[LevelManager] Ready");
        }

        public void ChangeLevel(string levelName, string spawnpoint)
        {
            LevelInfo _info = levelInfo[levelName];

            IsTransitioning = true;  // Must set this here since ChangeSceneNow is async
            CallDeferred(nameof(ChangeSceneNow), _info.Path, levelName, spawnpoint);
        }

        public async void ChangeSceneNow(string path, string levelName, string spawnpoint)
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

            GD.Print("[LevelManager] Unloading level: " + CurrentLevel.Name);
            BeginUnloadingLevel?.Invoke(levelName, spawnpoint);

            await FadeOut();

            PreviousLevelName = CurrentLevel.Name;
            PackedScene packedScene = (PackedScene)ResourceLoader.Load(path);

            if (packedScene == null)
            {
                GD.PrintErr("[LevelManager] Null scene at path: " + path);
                IsTransitioning = false;
                await FadeIn();
                return;
            }

            CurrentLevel?.CallDeferred("queue_free");
            await ToSignal(CurrentLevel, "tree_exited");
            
            CurrentLevel = packedScene.Instantiate();
            SceneTreeRoot.AddChild(CurrentLevel);
            
            LevelLoaded?.Invoke();
            GD.Print("[LevelManager] Level loaded: " + CurrentLevel.Name);

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

        public void LoadLevelInformation(string path = null)
        {
            GD.Print("[LevelManager] Beginning to load level information");

            List<LevelInfo> levels = Toolbox.LoadResourcesFromDirectory<LevelInfo>(path ?? LevelDir);
            foreach (LevelInfo level in levels)
            {
                string levelInfoName = level.ResourcePath.GetFile().TrimSuffix(".tres");
                levelInfo[levelInfoName] = level;
                GD.Print("[LevelManager] Successfully added ", levelInfoName);
            }

            GD.Print("[LevelManager] Loaded information on ", levelInfo.Count, " levels");
        }

        public LevelInfo GetLevelInfo(string levelName)
        {
            return levelInfo[levelName];
        }

    }

}

//await ToSignal(GetTree().CreateTimer(1.0f), "timeout");