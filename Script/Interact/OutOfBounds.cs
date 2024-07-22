using Godot;

namespace Game
{
    public partial class OutOfBounds : Area3D
    {
        [Export] public string SfxToPlay = "Splash";
        
        private Node3D playerResetPosition;
        private SFXPlayer3D sfxPlayer3D;
        private CameraBridge cameraBridge;

        public override void _Ready()
        {
            playerResetPosition = GetNode<Marker3D>("Spawnpoint");
            sfxPlayer3D = GetNode<SFXPlayer3D>("/root/SFXPlayer3D");
            cameraBridge = GetNode<CameraBridge>("/root/CameraBridge");

            BodyEntered += OnBodyEntered;
        }

        private void OnBodyEntered(Node body)
        {
            if (body is CharacterBody3D characterBody)
            {
                cameraBridge.Blink();
                sfxPlayer3D.PlaySound(SfxToPlay, characterBody.GlobalPosition);
                characterBody.GlobalTransform = new Transform3D(characterBody.GlobalTransform.Basis, playerResetPosition.GlobalTransform.Origin);
            }
        }

    }

}

