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

        private AnimationPlayer animationPlayer;
        private ColorRect colorRect;
        private Label levelNameLabel;
        private readonly object levelChangeLock = new();

        private Node3D levelAfterEpigraph;
        private string spawnpointAfterEpigraph = "";
        public string levelIDAfterEpigraph = "";

        public string CurrentLevelName => CurrentLevelInfo.LevelName;
        public string CurrentLevelID => CurrentLevelInfo.LevelID;


        public bool IsTransitioning { get; set; } = false;
        //public string PreviousLevelName { get; set; } = "";

        public LevelInfo CurrentLevelInfo;

        public Node3D CurrentLevel { get; set; }
        public Node SceneTree { get; set; }
        public Node SceneTreeRoot { get; set; }
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

            GD.Print("[LevelManager] Loaded information on ", LevelInfo.Count, " levels");
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

        public void ChangeLevel(string nextLevelID, string nextSpawnpoint)
        {
            if (LevelInfo.TryGetValue(nextLevelID, out LevelInfo nextLevelInfo))
            {
                if (nextLevelInfo.Level == null)
                {
                    GD.PrintErr("[LevelManager] Missing level in collection: ", nextLevelID);
                    return;
                }

                Node3D nextLevel = nextLevelInfo.Level.Instantiate<Node3D>();
                LevelInfo _nextLevelInfo = nextLevelInfo;

                if (nextLevelInfo.HasEpigraph && SaveManager != null && !SaveManager.GetBooleanValue("Epigraph_" + nextLevelID))
                {
                    GD.Print("[LevelManager] Epigraph not yet shown.. diverting from requested level");
                    levelIDAfterEpigraph = nextLevelID;
                    levelAfterEpigraph = nextLevelInfo.Level.Instantiate<Node3D>();
                    spawnpointAfterEpigraph = nextSpawnpoint;

                    LevelInfo.TryGetValue("Epigraph", out _nextLevelInfo);
                    nextLevelID = "Epigraph";
                    nextSpawnpoint = "";

                    if (LevelInfo.TryGetValue("Epigraph", out LevelInfo _epigraphInfo))
                    {
                        nextLevel = _epigraphInfo.Level.Instantiate<Node3D>();
                    }
                }

                IsTransitioning = true;  // Must set this here since ChangeSceneNow is async
                CallDeferred(nameof(ChangeLevelNow), _nextLevelInfo, nextSpawnpoint);
            }
            else
            {
                GD.PrintErr("[LevelManager] Requested level not found: ", nextLevelID);
            }
        }

        public async void ChangeLevelNow(LevelInfo nextLevelInfo, string spawnpoint)
        {
            GD.Print("[LevelManager] Chaning scene now to: ", nextLevelInfo.LevelName);

            if (nextLevelInfo.Level == null)
            {
                GD.PrintErr("[LevelManager] nextLevelPacked is null");
                IsTransitioning = false;
                return;
            }

            Node3D nextLevel = nextLevelInfo.Level.Instantiate<Node3D>();
            if (nextLevel == null)
            {
                GD.PrintErr("[LevelManager] nextLevel is null");
                IsTransitioning = false;
                return;
            }

            lock (levelChangeLock)
            {
                if (CurrentLevel != null && CurrentLevel.Name == nextLevel.Name)
                {
                    GD.PrintErr("[LevelManager] Blocking attempt to transition to the already loaded scene");
                    return;
                }
                IsTransitioning = true;  // Should already be true
            }

            GD.Print("[LevelManager] Unloading level: " + CurrentLevel.Name);
            BeginUnloadingLevel?.Invoke(nextLevelInfo.LevelID, spawnpoint);

            await FadeOut();

            //PreviousLevelName = CurrentLevel.Name;
            CurrentLevel?.CallDeferred("queue_free");
            await ToSignal(CurrentLevel, "tree_exited");

            CurrentLevelInfo = nextLevelInfo;
            CurrentLevel = CurrentLevelInfo.Level.Instantiate<Node3D>();

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

