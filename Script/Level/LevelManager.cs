using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

namespace Game
{
    public partial class LevelManager : Node
    {
        [Export] public float FadeSpeedScale = 1f;
        [Export] public Color FadeToColor = new("#000000");
        [Export] public Dictionary<StringName, LevelInfo> LevelInfo = [];

        private string spawnpointAfterEpigraph = "";
        private readonly object levelChangeLock = new();
        private AnimationPlayer animationPlayer;
        private ColorRect colorRect;
        private Label levelNameLabel;
        private LevelInfo epigraphLevelInfo;

        public bool IsTransitioning { get; set; } = false;
        public string CurrentLevelName => CurrentLevelInfo.LevelName;
        public string CurrentLevelID => CurrentLevelInfo.LevelID;
        public string LevelIDAfterEpigraph { get; set; } = "";
        public LevelInfo CurrentLevelInfo { get; set; }
        public Node SceneTree { get; set; }
        public Node SceneTreeRoot { get; set; }
        public Node3D CurrentLevel { get; set; }
        public SaveManager SaveManager { get; set; }

        public event Action LevelLoaded;
        public event Action<string, string> BeginUnloadingLevel;
        public event Action FadeInComplete;
        public event Action FadeOutComplete;

        public override void _Ready()
        {
            var SceneTree = GetTree();
            SceneTreeRoot = SceneTree.Root;
            CurrentLevel = SceneTree.CurrentScene as Node3D;
            
            if (LevelInfo.TryGetValue(CurrentLevel.Name, out LevelInfo _currentLevelInfo))
            {
                CurrentLevelInfo = _currentLevelInfo;
            }
            else
            {
                GD.PrintErr("[LevelManager] Unable to find first scene in the Level Info collection: ", CurrentLevelName);
            }

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            colorRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
            levelNameLabel = GetNode<Label>("LevelNameLabel");

            levelNameLabel.Modulate = new Color(1, 1, 1, 0);

            if (LevelInfo.TryGetValue("Epigraph", out epigraphLevelInfo))
            {
                // All good
            }
            else
            {
                GD.PrintErr("[LevelManager] epigraphLevelInfo not found");
            }

            GD.Print("[LevelManager] Loaded information on ", LevelInfo.Count, " levels");
            GD.Print("[LevelManager] Ready");
        }

        public void ReturningFromEpigraph()
        {
            SaveManager.UpdateBooleanValue("Epigraph_" + LevelIDAfterEpigraph, true);
            ChangeLevel(LevelIDAfterEpigraph, spawnpointAfterEpigraph);
        }

        public void ChangeLevel(string levelID)
        {
            ChangeLevel(levelID, "");
        }

        public void ChangeLevel(string nextLevelID, string nextSpawnpoint)
        {
            if (LevelInfo.TryGetValue(nextLevelID, out LevelInfo nextLevelInfo))
            {
                if (nextLevelInfo.Level == null)
                {
                    GD.PrintErr("[LevelManager] Missing level in collection: ", nextLevelID);
                    return;
                }

                if (nextLevelInfo.HasEpigraph && SaveManager != null && !SaveManager.GetBooleanValue("Epigraph_" + nextLevelID))
                {
                    GD.Print("[LevelManager] Epigraph not yet shown.. diverting from requested level");
                    LevelIDAfterEpigraph = nextLevelID;
                    spawnpointAfterEpigraph = nextSpawnpoint;
                    nextLevelID = "Epigraph";
                    nextSpawnpoint = "";
                    nextLevelInfo = epigraphLevelInfo;
                }

                IsTransitioning = true;  // Must set this here since ChangeSceneNow is async
                CallDeferred(nameof(ChangeLevelNow), nextLevelInfo, nextSpawnpoint);
            }
            else
            {
                GD.PrintErr("[LevelManager] Requested level not found: ", nextLevelID);
            }
        }

        private async void ChangeLevelNow(LevelInfo nextLevelInfo, string spawnpoint)
        {
            GD.Print("[LevelManager] Chaning scene now to: ", nextLevelInfo.LevelName);

            if (nextLevelInfo.Level == null)
            {
                GD.PrintErr("[LevelManager] nextLevelPacked is null");
                IsTransitioning = false;
                return;
            }

            lock (levelChangeLock)
            {
                if (CurrentLevel != null && CurrentLevel.Name == nextLevelInfo.LevelID)
                {
                    GD.PrintErr("[LevelManager] Blocking attempt to transition to the already loaded scene");
                    return;
                }
                IsTransitioning = true;  // Should already be true
            }

            GD.Print("[LevelManager] Unloading level: " + CurrentLevel.Name);
            BeginUnloadingLevel?.Invoke(nextLevelInfo.LevelID, spawnpoint);

            await FadeOut();

            CurrentLevel.CallDeferred("queue_free");
            await ToSignal(CurrentLevel, "tree_exited");

            CurrentLevelInfo = nextLevelInfo;
            CurrentLevel = nextLevelInfo.Level.Instantiate<Node3D>(); ;
            SceneTreeRoot.AddChild(CurrentLevel);

            LevelLoaded?.Invoke();
            GD.Print("[LevelManager] Level loaded: " + CurrentLevel.Name);

            await FadeIn();

            DisplayLevelName();

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

        private async void DisplayLevelName()
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

