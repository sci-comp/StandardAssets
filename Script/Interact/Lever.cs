using Godot;

namespace Game
{
    public partial class Lever : InteractableArea
    {
        private Node3D leverArm;
        [Export] public AxisDirection rotateAround = AxisDirection.Right;
        [Export] public float LeverMoveDuration = 1.0f;
        [Export] public float RestDuration = 1.0f;
        [Export] public float DegreesRotation = 60.0f;
        [Export] public string _Title = "Lever";
        [Export] public string _Details = "";
        [Export] public string SoundName = "Lever";

        private bool alreadyTriggered = false;
        private bool canInteract = true;
        private bool isMoving = false;
        private float t_current = 0.0f;
        private Basis initialBasis;
        private Basis destBasis;
        private Vector3 rotateAroundAxis;
        private SFXPlayer3D sfxPlayer3D;
        private Tween tween;
        
        public bool Activated => !canInteract;

        public bool CanInteract() { return canInteract; }

        public override string Title => _Title;
        public override string Details => _Details;

        public override void _Ready()
        {
            sfxPlayer3D = GetNode<SFXPlayer3D>("/root/SFXPlayer3D");
            leverArm = GetNode<Node3D>("LeverArm");

            rotateAroundAxis = Toolbox.GetAxisDirection(rotateAround);

            initialBasis = leverArm.Basis;
            destBasis = initialBasis.Rotated(rotateAroundAxis, Mathf.DegToRad(DegreesRotation));
        }


        public void ResetLever()
        {
            if (canInteract)
            {
                return;
            }

            PlaySound();
            tween = CreateTween();
            tween.TweenProperty(leverArm, "basis", initialBasis, LeverMoveDuration);
            tween.Finished += () => canInteract = true;
            tween.Play();
        }

        public void PlaySound()
        {
            sfxPlayer3D.PlaySound(SoundName, GlobalPosition);
        }

        public override void Interact()
        {
            if (!canInteract)
            {
                return;
            }

            canInteract = false;
            PlaySound();
            tween = CreateTween();
            tween.TweenProperty(leverArm, "basis", destBasis, LeverMoveDuration);
            tween.Play();
            base.Interact();
        }

    }

}

