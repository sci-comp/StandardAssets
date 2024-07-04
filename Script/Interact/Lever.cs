using Godot;

namespace Game
{
    public partial class Lever : InteractableArea
    {
        private Node3D leverArm;
        [Export] public AxisDirection rotateAround = AxisDirection.Right;
        [Export] public bool Reusable = true;
        [Export] public float LeverMoveDuration = 0.6f;
        [Export] public float MoveDuration = 1.0f;
        [Export] public float RestDuration = 1.0f;
        [Export] public float DegreesRotation = 60.0f;
        [Export] public string _Title = "Lever";
        [Export] public string _Details = "";

        private bool alreadyTriggered = false;
        private bool canInteract = true;
        private bool isMoving = false;
        private float t_current = 0.0f;
        private Basis initialBasis;
        private Basis destBasis;
        private Vector3 rotateAroundAxis;
        private Tween tween;

        public bool Activated => !canInteract;

        public bool CanInteract() { return canInteract; }

        public override string Title => _Title;
        public override string Details => _Details;

        public override void _Ready()
        {
            leverArm = GetNode<Node3D>("LeverArm");

            rotateAroundAxis = Toolbox.GetAxisDirection(rotateAround);

            initialBasis = leverArm.Basis;
            destBasis = initialBasis.Rotated(rotateAroundAxis, Mathf.DegToRad(DegreesRotation));

            tween = CreateTween();
            tween.TweenProperty(leverArm, "basis", destBasis, MoveDuration);
            tween.TweenInterval(RestDuration);

            if (Reusable)
            {
                tween.TweenProperty(leverArm, "basis", initialBasis, MoveDuration);
                tween.SetLoops();
                tween.LoopFinished += OnIdle;
            }

            tween.Pause();
        }

        private void OnIdle(long _loopCount)
        {
            tween.Pause();
            canInteract = true;
        }

        public override void Interact()
        {
            if (!canInteract)
            {
                return;
            }
            canInteract = false;
            tween.Play();
            base.Interact();
        }

    }

}

