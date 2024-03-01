using Godot;
using System.Threading.Tasks;

public partial class LevelManager : Node
{
    [Export] public int Speed = 2;
    [Export] public Color ShaderColor = new("#000000");
    [Export] public Texture Pattern = new Texture2D();
    [Export] public float WaitTime = 0.5f;
    [Export] public bool InvertOnLeave = true;
    [Export] public float Ease = 1.0f;
    [Export] public string LevelInfoCollectionPath = "res://Game/Data/LevelInfoCollectionPath.tres";
        
    private AnimationPlayer animationPlayer;
    private ColorRect shaderBlendRect;
    private readonly object levelChangeLock = new();

    public bool IsTransitioning { get; set; } = false;
    public string PreviousLevelName { get; set; } = "";
    public LevelInfoCollection LevelInfoCollection { get; private set; }
    public Node CurrentLevel { get; set; }
    public Node SceneTree { get; set; }
    public Node SceneTreeRoot { get; set; }

    [Signal] public delegate void LevelLoadedEventHandler();
    [Signal] public delegate void BeginUnloadingLevelEventHandler();
    [Signal] public delegate void FadeInCompleteEventHandler();
    [Signal] public delegate void FadeOutCompleteEventHandler();

    public LevelInfo CurrentLevelInfo
    {
        get
        {
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

        var _resource = (Resource)GD.Load(LevelInfoCollectionPath);
        LevelInfoCollection = (LevelInfoCollection)_resource.Duplicate();

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        shaderBlendRect = GetNode<ColorRect>("CanvasLayer/ColorRect");

        ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("dissolve_texture", Pattern);
        ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("fade_color", ShaderColor);

        GD.Print("Level manager ready, loaded: " + CurrentLevel.Name);
        EmitSignal(nameof(LevelLoaded));
    }

    public void ChangeLevel(string levelName)
    {
        LevelInfo _info = LevelInfoCollection.LevelInfo[levelName];

        // Wait until the end of the frame
        IsTransitioning = true;  // Must set this here since ChangeSceneNow is async
        CallDeferred(nameof(ChangeSceneNow), _info.Path);
    }

    public async void ChangeSceneNow(string path)
    {
        if (path == null)
        {
            GD.PrintErr("Scene path is null");
            IsTransitioning = false;
            return;
        }

        lock (levelChangeLock)
        {
            if (CurrentLevel != null && CurrentLevel.SceneFilePath == path)
            {
                GD.Print("We are trying to transition to the same scene. Does that make sense?");
                return;
            }

            IsTransitioning = true;  // Should already be true
        }

        if (ResourceLoader.Load(path, "PackedScene", 0) is not PackedScene nextLevel)
        {
            GD.PrintErr("Invalid level path");
            IsTransitioning = false;
            return;
        }

        GD.Print("Unloading level: " + CurrentLevel.Name);
        EmitSignal(nameof(BeginUnloadingLevel));

        PreviousLevelName = CurrentLevel.Name;

        await FadeOut();

        CurrentLevel?.CallDeferred("queue_free");
        await ToSignal(CurrentLevel, "tree_exited");
        CurrentLevel = nextLevel.Instantiate();
        SceneTreeRoot.AddChild(CurrentLevel);

        GD.Print("Level loaded: " + CurrentLevel.Name);
        EmitSignal(nameof(LevelLoaded));

        await FadeIn();

        IsTransitioning = false;
    }

    public async Task FadeOut()
    {
        animationPlayer.SpeedScale = Speed;
        ((ShaderMaterial) shaderBlendRect.Material).SetShaderParameter("inverted", false);
        var animation = animationPlayer.GetAnimation("ShaderFade");
        animation.TrackSetKeyTransition(0, 0, Ease);
        animationPlayer.Play("ShaderFade");
        await ToSignal(animationPlayer, "animation_finished");
        EmitSignal(nameof(FadeOutComplete));
    }

    public async Task FadeIn()
    {
        animationPlayer.SpeedScale = Speed;
        ((ShaderMaterial) shaderBlendRect.Material).SetShaderParameter("inverted", true);
        var animation = animationPlayer.GetAnimation("ShaderFade");
        animation.TrackSetKeyTransition(0, 0, Ease);
        animationPlayer.PlayBackwards("ShaderFade");
        await ToSignal(animationPlayer, "animation_finished");
        EmitSignal(nameof(FadeInComplete));
    }

}

