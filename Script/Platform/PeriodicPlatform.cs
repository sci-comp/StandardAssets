using Godot;

public partial class PeriodicPlatform : AnimatableBody3D
{
    [Export] float restDuration = 1.0f;
    [Export] Vector3 moveDistance = new(0, 1, 0);
    [Export] float moveDuration = 1.0f;

    private Vector3 initialPos;
    private Vector3 dest;
    private Tween tween;

    public override void _Ready()
    {
        initialPos = Position;
        dest = initialPos + moveDistance;

        tween = CreateTween();

        tween.TweenProperty(this, "position", dest, moveDuration);
        tween.TweenInterval(restDuration);
        tween.TweenProperty(this, "position", initialPos, moveDuration);
        tween.TweenInterval(restDuration);
        tween.SetLoops(0);
        tween.Play(); 
    }

}

