using Godot;
using System.Collections.Generic;

public partial class PlatformLever : AnimatableBody3D, IInteractable
{
    [Export] public bool Reusable = true;
    [Export] public float LeverMoveDuration = 0.6f;
    [Export] public float MoveDuration = 1.0f;
    [Export] public float RestDuration = 1.0f;
    [Export] public float DegreesRotation = 60.0f;

    private List<ITriggeredPlatform> platforms;

    private bool alreadyTriggered = false;
    private bool isMoving = false;
    private float t_current = 0.0f;

    private bool canBeTriggered = true;
    private bool enabled = false;
    private Vector3 initialRot;
    private Vector3 dest;
    private Tween tween;

    public string Title => title;
    public string Details => details;

    private string title = "Lever";
    private string details = "Details";

    public bool CanBeTriggered() { return canBeTriggered; }
    public bool Enabled() { return enabled; }

    public void Initialize(string _title, string _details, List<ITriggeredPlatform> _platforms)
    {
        title = _title;
        details = _details;
        platforms = _platforms;

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
        tween.Pause();
        canBeTriggered = true;
    }

    public void Interact()
    {
        if (!canBeTriggered)
        {
            return;
        }
        
        foreach(ITriggeredPlatform platform in platforms)
        {
            if (!platform.CanBeTriggered())
            {
                return;
            }
        }

        // Only trigger lever if all platforms can be triggered
        canBeTriggered = false;
        tween.Play();
        foreach (ITriggeredPlatform platform in platforms)
        {
            platform.Trigger();
        }
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
