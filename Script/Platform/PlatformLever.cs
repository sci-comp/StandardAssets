using Godot;
using Godot.Collections;
using System;

public partial class PlatformLever : AnimatableBody3D, IInteractable
{
    [Export] public Array<AnimatableBody3D> platforms;
    //[Export] Node3D lever;
    [Export] public bool Reusable = true;
    [Export] public float LeverMoveDuration = 0.6f;
    [Export] public float MoveDuration = 1.0f;
    [Export] public float RestDuration = 1.0f;
    [Export] public float DegreesRotation = 60.0f;

    private bool alreadyTriggered = false;
    private bool isMoving = false;
    private float t_current = 0.0f;

    private bool canBeTriggered = true;
    private bool enabled = false;
    private Vector3 initialRot;
    private Vector3 dest;
    private Tween tween;

    public string Title => "Lever";
    public string Details => "Details";

    public bool CanBeTriggered() { return canBeTriggered; }
    public bool Enabled() { return enabled; }

    public override void _Ready()
    {
        initialRot = Rotation;
        dest = new Vector3(initialRot.X + Mathf.DegToRad(DegreesRotation), initialRot.Y, initialRot.Z);

        tween = CreateTween();
        tween.TweenProperty(this, "rotation", dest, MoveDuration);
        tween.TweenInterval(RestDuration);
        tween.TweenProperty(this, "rotation", initialRot, MoveDuration);
        tween.SetLoops();
        tween.LoopFinished += OnIdle;
        tween.Pause();
    }

    private void OnIdle(long _loopCount)
    {
        GD.Print("On idle");
        tween.Pause();
        canBeTriggered = true;
    }

    public void Interact()
    {
        if (!canBeTriggered)
        {
            return;
        }
        canBeTriggered = false;
        tween.Play();
    }

    public void Inspect()
    {
        // Do nothing
    }

    public void Select()
    {
        // Do nothing
    }

    public void Deselect()
    {
        // Do nothing
    }

}

