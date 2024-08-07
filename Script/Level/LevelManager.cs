using Godot;
using System.Threading.Tasks;

namespace Game
{
    public partial class LevelManager : Node
    {
        [Export] public int Speed = 2;
        [Export] public Color ShaderColor = new("#000000");
        [Export] public Texture Pattern = new Texture2D();
        [Export] public float WaitTime = 0.5f;
        [Export] public bool InvertOnLeave = true;
        [Export] public float Ease = 1.0f;
        [Export] public string LevelInfoCollectionPath = "res://Data/LevelInfoCollection.tres";

        private AnimationPlayer animationPlayer;
        private ColorRect shaderBlendRect;
        private readonly object levelChangeLock = new();
        private Node spawnpoints;

        public bool IsTransitioning { get; set; } = false;
        public string RequestedSpawnpoint { get; set; } = "";
        public string PreviousLevelName { get; set; } = "";
        public LevelInfoCollection LevelInfoCollection { get; private set; }
        public Node CurrentLevel { get; set; }
        public Node SceneTree { get; set; }
        public Node SceneTreeRoot { get; set; }
        public string CurrentLevelName => CurrentLevel.Name;

        [Signal] public delegate void LevelLoadedEventHandler();
        [Signal] public delegate void BeginUnloadingLevelEventHandler();
        [Signal] public delegate void FadeInCompleteEventHandler();
        [Signal] public delegate void FadeOutCompleteEventHandler();

        public LevelInfo CurrentLevelInfo
        {
            get
            {
                if (LevelInfoCollection == null)
                {
                    GD.PrintErr("[LevelManager] LevelInfoCollection is null");
                    return null;
                }

                if (LevelInfoCollection.LevelInfo.ContainsKey(CurrentLevel.Name))
                {
                    return LevelInfoCollection.LevelInfo[CurrentLevel.Name];
                }
                else
                {
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

            if (ResourceLoader.Exists(LevelInfoCollectionPath))
            {
                var _resource = ResourceLoader.Load(LevelInfoCollectionPath);
                LevelInfoCollection = (LevelInfoCollection)_resource.Duplicate();
                if (LevelInfoCollection == null)
                {
                    GD.PrintErr("[LevelManager] LevelInfoCollection is null");
                    return;
                }
            }
            else
            {
                GD.PrintErr("[LevelManager] LevelInfoCollection not found at location: ", LevelInfoCollectionPath);
                return;
            }

            animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            shaderBlendRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
            
            ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("dissolve_texture", Pattern);
            ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("fade_color", ShaderColor);

            GD.Print("[LevelManager] Ready");
        }

        public void ChangeLevel(string levelName, string spawnpoint = "")
        {
            RequestedSpawnpoint = spawnpoint;

            LevelInfo _info = LevelInfoCollection.LevelInfo[levelName];

            // Wait until the end of the frame
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
            EmitSignal(nameof(BeginUnloadingLevel));

            PreviousLevelName = CurrentLevel.Name;

            await FadeOut();

            CurrentLevel?.CallDeferred("queue_free");
            await ToSignal(CurrentLevel, "tree_exited");
            CurrentLevel = nextLevel.Instantiate();
            SceneTreeRoot.AddChild(CurrentLevel);

            GD.Print("[LevelManager] Level loaded: " + CurrentLevel.Name);
            EmitSignal(nameof(LevelLoaded));

            await FadeIn();

            IsTransitioning = false;
        }

        public async Task FadeOut()
        {
            animationPlayer.SpeedScale = Speed;
            ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("inverted", false);
            var animation = animationPlayer.GetAnimation("ShaderFade");
            animation.TrackSetKeyTransition(0, 0, Ease);
            animationPlayer.Play("ShaderFade");
            await ToSignal(animationPlayer, "animation_finished");
            EmitSignal(nameof(FadeOutComplete));
        }

        public async Task FadeIn()
        {
            animationPlayer.SpeedScale = Speed;
            ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("inverted", true);
            var animation = animationPlayer.GetAnimation("ShaderFade");
            animation.TrackSetKeyTransition(0, 0, Ease);
            animationPlayer.PlayBackwards("ShaderFade");
            await ToSignal(animationPlayer, "animation_finished");
            EmitSignal(nameof(FadeInComplete));
        }

    }

}

