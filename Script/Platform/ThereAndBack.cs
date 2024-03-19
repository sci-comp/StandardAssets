using Godot;
using System;

public partial class ThereAndBack : AnimatableBody3D, ITriggeredPlatform
{
    [Export] public float RestDuration = 1.0f;
    [Export] public Vector3 MoveDistance = new(0, 1, 0);
    [Export] public float MoveDuration = 1.0f;

    private bool canBeTriggered = true;
    private bool enabled = false;
    private Vector3 initialPos;
    private Vector3 dest;
    private Tween tween;

    public bool CanBeTriggered() { return canBeTriggered; }
    public bool Enabled() { return enabled; }

    public override void _Ready()
    {
        initialPos = Position;
        dest = initialPos + MoveDistance;

        tween = CreateTween();
        tween.TweenProperty(this, "position", dest, MoveDuration);
        tween.TweenInterval(RestDuration);
        tween.TweenProperty(this, "position", initialPos, MoveDuration);
        tween.TweenCallback(new Callable(this, nameof(OnIdle)));
    }

    public void Disable()
    {
        
    }

    public void Enable()
    {
        
    }

    public void Trigger()
    {
        if (!canBeTriggered)
        {
            return;
        }

        canBeTriggered = false;
        tween.Play();
    }

    private void OnIdle()
    {
        GD.Print("On idle");
        canBeTriggered = true;
    }

}

