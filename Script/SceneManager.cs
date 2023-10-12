using Godot;
using System.Threading.Tasks;

public partial class SceneManager : Singleton<SceneManager>
{
    [Export] public int Speed = 2;
    [Export] public Color ShaderColor = new("#000000");
    [Export] public Texture Pattern = new Texture2D();
    [Export] public float WaitTime = 0.5f;
    [Export] public bool InvertOnLeave = true;
    [Export] public float Ease = 1.0f;
    [Export] public string SceneSpecificInformationCollectionPath = "res://Game/Data/SceneSpecificInformationCollection.tres";
    public Node CurrentScene { get; set; }
    public string CurrentSceneName => CurrentScene.Name;

    public string PreviousSceneName { get; set; } = "";

    private AnimationPlayer animationPlayer;
    private ColorRect shaderBlendRect;
    public Node sceneTreeRoot;
    
    private readonly object sceneChangeLock = new();

    public bool IsTransitioning { get; set; } = false;

    [Signal] public delegate void SceneLoadedEventHandler();
    [Signal] public delegate void BeginUnloadingSceneEventHandler();
    [Signal] public delegate void FadeInCompleteEventHandler();
    [Signal] public delegate void FadeOutCompleteEventHandler();

    public Node SceneTree { get; set; }

    public SceneSpecificInformationCollection SceneInfoCollection { get; private set; }

    public SceneSpecificInformation CurrentSceneInfo
    {
        get
        {
            return SceneInfoCollection.SceneInfo[CurrentSceneName];
        }
    }

    public override void _Ready()
    {
        var SceneTree = GetTree();
        sceneTreeRoot = SceneTree.Root;
        CurrentScene = SceneTree.CurrentScene;

        var _resource = (Resource)GD.Load(SceneSpecificInformationCollectionPath);
        SceneInfoCollection = (SceneSpecificInformationCollection)_resource.Duplicate();

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        shaderBlendRect = GetNode<ColorRect>("CanvasLayer/ColorRect");

        ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("dissolve_texture", Pattern);
        ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("fade_color", ShaderColor);

        EmitSignal(nameof(SceneLoaded));
    }

    public void ChangeScene(string sceneName)
    {
        SceneSpecificInformation _info = SceneInfoCollection.SceneInfo[sceneName];

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

        lock (sceneChangeLock)
        {
            if (CurrentScene != null && CurrentScene.SceneFilePath == path)
            {
                return;
            }

            IsTransitioning = true;  // Should already be true
        }

        if (ResourceLoader.Load(path, "PackedScene", 0) is not PackedScene nextScene)
        {
            GD.PrintErr("Invalid scene path");
            IsTransitioning = false;
            return;
        }

        EmitSignal(nameof(BeginUnloadingScene));

        PreviousSceneName = CurrentScene.Name;

        await FadeOut();

        CurrentScene?.CallDeferred("queue_free");
        await ToSignal(CurrentScene, "tree_exited");
        CurrentScene = nextScene.Instantiate();
        sceneTreeRoot.AddChild(CurrentScene);

        EmitSignal(nameof(SceneLoaded));

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
        ((ShaderMaterial) shaderBlendRect.Material).SetShaderParameter("inverted", true);
        var animation = animationPlayer.GetAnimation("ShaderFade");
        animation.TrackSetKeyTransition(0, 0, Ease);
        animationPlayer.PlayBackwards("ShaderFade");
        await ToSignal(animationPlayer, "animation_finished");
        EmitSignal(nameof(FadeInComplete));
    }

}

