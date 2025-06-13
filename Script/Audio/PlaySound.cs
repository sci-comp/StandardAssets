using Godot;

namespace Game
{
    public partial class PlaySound : InteractableArea
    {
        [Export] public string SoundName = "DoorOpen";
        [Export] public string _Title = "Object";
        [Export] public string _Details = "Unidentified";
        [Export] public bool PlayOnEnter = false;
        [Export] public bool PlayOnInteract = true;

        private SFX sfx;

        public override string Title => _Title;
        public override string Details => _Details;

        public override void _Ready()
        {
            sfx = GetNode<SFX>("/root/SFX");

            if (PlayOnEnter)
            {
                BodyEntered += OnBodyEntered;
            }
        }

        public override void _ExitTree()
        {
            if (PlayOnEnter)
            {
                BodyEntered -= OnBodyEntered;
            }
        }

        public override void Interact(string playerID = "")
        {
            if (PlayOnInteract)
            {
                sfx.PlaySound(SoundName, Position);
            }
            base.Interact(playerID);
        }

        private void OnBodyEntered(Node3D body)
        {
            sfx.PlaySound(SoundName, Position);
        }
    }
}