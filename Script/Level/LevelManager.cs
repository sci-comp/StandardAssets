using Godot;
using System.Threading.Tasks;

namespace Game
{
    public partial class LevelManager : Node
    {
        [Export] public float FadeSpeedScale = 1f;
        [Export] public Color FadeToColor = new("#000000");
        [Export] public string LevelInfoCollectionPath = "res://Data/LevelInfoCollection.tres";

        private AnimationPlayer animationPlayer;
        private ColorRect colorRect;
        private readonly object levelChangeLock = new();
        private Node spawnpoints;

        public bool IsTransitioning { get; set; } = false;
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
            colorRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
            
            GD.Print("[LevelManager] Ready");
        }

        public void ChangeLevel(string levelName)
        {
            LevelInfo _info = LevelInfoCollection.LevelInfo[levelName];

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

            GD.Print("Before fade out");
            await FadeOut();

            GD.Print("Before timer 1");
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

            CurrentLevel?.CallDeferred("queue_free");
            //CurrentLevel.QueueFree();

            GD.Print("Before tree exit");
            await ToSignal(CurrentLevel, "tree_exited");

            GD.Print("Before timer 2");
            await ToSignal(GetTree().CreateTimer(1.0f), "timeout");
            // await ToSignal(GetTree(), "process_frame");

            GD.Print("After timer");
            CurrentLevel = nextLevel.Instantiate();
            SceneTreeRoot.AddChild(CurrentLevel);

            GD.Print("[LevelManager] Level loaded: " + CurrentLevel.Name);
            EmitSignal(nameof(LevelLoaded));

            await FadeIn();

            IsTransitioning = false;
        }

        public async Task FadeOut()
        {
            animationPlayer.SpeedScale = FadeSpeedScale;
            animationPlayer.Play("FadeToBlack");
            await ToSignal(animationPlayer, "animation_finished");
            GD.Print("Fade out completed");
            EmitSignal(nameof(FadeOutComplete));
        }

        public async Task FadeIn()
        {
            animationPlayer.SpeedScale = FadeSpeedScale;
            animationPlayer.PlayBackwards("FadeToBlack");
            await ToSignal(animationPlayer, "animation_finished");
            EmitSignal(nameof(FadeInComplete));
        }
    }

}

