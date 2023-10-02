using Godot;
using System.Threading.Tasks;

public partial class SceneManager2 : Node
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
    [Signal] public delegate void TransitionFinishedEventHandler();

    public bool IsTransitioning { get; set; } = false;

    private SceneTree _tree;
    private Node _root;
    private Node _currentScene;
    private AnimationPlayer _animationPlayer;
    private ColorRect _shaderBlendRect;

    public override void _Ready()
    {
        _tree = GetTree();
        _root = _tree.Root;
        _currentScene = _tree.CurrentScene;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _shaderBlendRect = GetNode<ColorRect>("CanvasLayer/ColorRect");
        EmitSignal(nameof(SceneLoaded));
    }

    public async void ChangeScene(string path)
    {
        if (path == null)
        {
            GD.PrintErr("Scene path is null");
            return;
        }

        var nextScene = (PackedScene)ResourceLoader.Load(path, "PackedScene", 0);

        if (nextScene == null)
        {
            GD.PrintErr("Invalid scene path");
            return;
        }

        IsTransitioning = true;

        EmitSignal(nameof(BeginUnloadingScene));

        _currentScene?.QueueFree();

        await FadeOut();

        _currentScene = nextScene.Instantiate();  // Synchronous
        _root.AddChild(_currentScene);
        _tree.CurrentScene = _currentScene;

        EmitSignal(nameof(SceneLoadedEventHandler));

        await FadeIn();

        IsTransitioning = false;
    }

    public async Task FadeOut()
    {
        _animationPlayer.SpeedScale = Speed;

        ((ShaderMaterial)_shaderBlendRect.Material).SetShaderParameter("dissolve_texture", Pattern);
        ((ShaderMaterial)_shaderBlendRect.Material).SetShaderParameter("fade_color", ShaderColor);
        ((ShaderMaterial)_shaderBlendRect.Material).SetShaderParameter("inverted", false);

        var animation = _animationPlayer.GetAnimation("ShaderFade");
        animation.TrackSetKeyTransition(0, 0, Ease);

        _animationPlayer.Play("ShaderFade");
        await ToSignal(_animationPlayer, "animation_finished");

        EmitSignal(nameof(FadeOutCompleteEventHandler));
    }

    public async Task FadeIn()
    {
        _animationPlayer.SpeedScale = Speed;

        ((ShaderMaterial) _shaderBlendRect.Material).SetShaderParameter("dissolve_texture", Pattern);
        ((ShaderMaterial) _shaderBlendRect.Material).SetShaderParameter("fade_color", ShaderColor);
        ((ShaderMaterial) _shaderBlendRect.Material).SetShaderParameter("inverted", true);

        var animation = _animationPlayer.GetAnimation("ShaderFade");
        animation.TrackSetKeyTransition(0, 0, Ease);

        _animationPlayer.PlayBackwards("ShaderFade");
        await ToSignal(_animationPlayer, "animation_finished");

        EmitSignal(nameof(FadeInCompleteEventHandler));
    }

}

