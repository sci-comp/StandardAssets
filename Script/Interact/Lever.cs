using Godot;

namespace Game
{
    public partial class Lever : Interactable
    {
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
        private Vector3 initialRot;
        private Vector3 dest;
        private Tween tween;

        public bool Activated => !canInteract;

        public override string Title => _Title;
        public override string Details => _Details;

        public override void _Ready()
        {
            initialRot = Rotation;
            dest = new Vector3(initialRot.X + Mathf.DegToRad(DegreesRotation), initialRot.Y, initialRot.Z);

            tween = CreateTween();
            tween.TweenProperty(this, "rotation", dest, MoveDuration);
            tween.TweenInterval(RestDuration);

            if (Reusable)
            {
                tween.TweenProperty(this, "rotation", initialRot, MoveDuration);
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

