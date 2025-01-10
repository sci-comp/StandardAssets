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
        private Label levelNameLabel;
        private readonly object levelChangeLock = new();
        private readonly Dictionary<string, LevelInfo> levelInfo = new();

        public bool IsTransitioning { get; set; } = false;
        public string PreviousLevelName { get; set; } = "";
        
        public Node CurrentLevel { get; set; }
        public Node SceneTree { get; set; }
        public Node SceneTreeRoot { get; set; }
        public string CurrentLevelID => CurrentLevelInfo.LevelID;
        public SaveManager SaveManager { get; set; }

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
            levelNameLabel = GetNode<Label>("LevelNameLabel");

            levelNameLabel.Modulate = new Color(1, 1, 1, 0);

            GD.Print("[LevelManager] Ready");
        }

        public void ReturningFromEpigraph()
        {
            SaveManager.UpdateBooleanValue("Epigraph_" + levelIDAfterEpigraph, true);
            ChangeLevel(levelIDAfterEpigraph, spawnpointAfterEpigraph);
        }

        public void ChangeLevel(string levelID)
        {
            ChangeLevel(levelID, "");
        }

        private string levelPathAfterEpigraph = "";
        public string levelIDAfterEpigraph = "";
        private string spawnpointAfterEpigraph = "";
        public void ChangeLevel(string levelID, string spawnpoint)
        {
            LevelInfo _info = levelInfo[levelID];

            string _path = _info.LevelPath;

            if (_info.HasEpigraph && SaveManager != null && !SaveManager.GetBooleanValue("Epigraph_" + levelID))
            {
                GD.Print("[LevelManager] Epigraph not yet shown.. diverting from requested level");
                levelIDAfterEpigraph = levelID;
                levelPathAfterEpigraph = _info.LevelPath;
                spawnpointAfterEpigraph = spawnpoint;

                levelID = "Epigraph";
                spawnpoint = "";
                _path = "res://Level/Epigraph.tscn";
            }

            IsTransitioning = true;  // Must set this here since ChangeSceneNow is async
            CallDeferred(nameof(ChangeSceneNow), _path, levelID, spawnpoint);
        }

        public async void ChangeSceneNow(string path, string levelID, string spawnpoint)
        {
            GD.Print("[LevelManager] Chaning scene now:  ", levelID);

            if (path == "")
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
            BeginUnloadingLevel?.Invoke(levelID, spawnpoint);

            await FadeOut();

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

            PreviousLevelName = CurrentLevel.Name;
            CurrentLevel = packedScene.Instantiate();
            SceneTreeRoot.AddChild(CurrentLevel);
            
            LevelLoaded?.Invoke();
            GD.Print("[LevelManager] Level loaded: " + CurrentLevel.Name);

            await FadeIn();

            DisplayLevelName(levelID);

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

            List<LevelInfo> levels = Toolbox.FileSystem.LoadResourcesFromDirectory<LevelInfo>(path ?? LevelDir);
            foreach (LevelInfo level in levels)
            {
                string levelInfoName = level.ResourcePath.GetFile().TrimSuffix(".tres");
                levelInfo[levelInfoName] = level;
            }

            GD.Print("[LevelManager] Loaded information on ", levelInfo.Count, " levels");
        }

        public LevelInfo GetLevelInfo(string levelName)
        {
            return levelInfo[levelName];
        }

        private async void DisplayLevelName(string levelID)
        {
            if (levelNameLabel == null || !CurrentLevelInfo.FadeLevelTitleInOut)
            {
                return;
            }

            levelNameLabel.Text = CurrentLevelInfo.LevelName;
            levelNameLabel.Modulate = new Color(1, 1, 1, 0);
            levelNameLabel.Visible = true;

            Tween fadeInTween = CreateTween();
            fadeInTween.TweenProperty(levelNameLabel, "modulate:a", 1, .5);
            await ToSignal(fadeInTween, "finished");

            await ToSignal(GetTree().CreateTimer(2), "timeout");

            Tween fadeOutTween = CreateTween();
            fadeOutTween.TweenProperty(levelNameLabel, "modulate:a", 0, 1);
            await ToSignal(fadeOutTween, "finished");

            levelNameLabel.Visible = false;
        }

    }

}

