using Godot;
using System.Threading.Tasks;

public partial class SceneManager : Node
{
    [Export] public int Speed = 2;
    [Export] public Color ShaderColor = new("#000000");
    [Export] public Texture Pattern = new Texture2D();
    [Export] public float WaitTime = 0.5f;
    [Export] public bool InvertOnLeave = true;
    [Export] public float Ease = 1.0f;

    [Signal] public delegate void SceneLoadedEventHandler();
    [Signal] public delegate void BeginUnloadingSceneEventHandler();
    [Signal] public delegate void FadeInCompleteEventHandler();
    [Signal] public delegate void FadeOutCompleteEventHandler();

    public bool IsTransitioning { get; set; } = false;

    private SceneTree sceneTree;
    private Node sceneTreeRoot;
    private Node currentScene;
    private AnimationPlayer animationPlayer;
    private ColorRect shaderBlendRect;

    public override void _Ready()
    {
        sceneTree = GetTree();
        sceneTreeRoot = sceneTree.Root;
        currentScene = sceneTree.CurrentScene;

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        shaderBlendRect = GetNode<ColorRect>("CanvasLayer/ColorRect");

        ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("dissolve_texture", Pattern);
        ((ShaderMaterial)shaderBlendRect.Material).SetShaderParameter("fade_color", ShaderColor);

        EmitSignal(nameof(SceneLoaded));
    }

    private object sceneChangeLock = new object();

    public async Task ChangeScene(string path)
    {
        lock (sceneChangeLock)
        {
            if (IsTransitioning || (currentScene != null && currentScene.SceneFilePath == path))
            {
                return;
            }

            IsTransitioning = true;
        }

        if (path == null)
        {
            GD.PrintErr("Scene path is null");
            return;
        }

        if (ResourceLoader.Load(path, "PackedScene", 0) is not PackedScene nextScene)
        {
            GD.PrintErr("Invalid scene path");
            return;
        }


        IsTransitioning = true;

        EmitSignal(nameof(BeginUnloadingScene));

        currentScene?.QueueFree();

        await FadeOut();

        currentScene = nextScene.Instantiate();  // Synchronous
        sceneTreeRoot.AddChild(currentScene);
        sceneTree.CurrentScene = currentScene;

        EmitSignal(nameof(SceneLoadedEventHandler));

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

        EmitSignal(nameof(FadeOutCompleteEventHandler));
    }

    public async Task FadeIn()
    {
        animationPlayer.SpeedScale = Speed;

        ((ShaderMaterial) shaderBlendRect.Material).SetShaderParameter("inverted", true);

        var animation = animationPlayer.GetAnimation("ShaderFade");
        animation.TrackSetKeyTransition(0, 0, Ease);

        animationPlayer.PlayBackwards("ShaderFade");
        await ToSignal(animationPlayer, "animation_finished");

        EmitSignal(nameof(FadeInCompleteEventHandler));
    }

}

