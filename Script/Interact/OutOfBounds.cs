using Godot;

namespace Game
{
    public partial class OutOfBounds : Area3D
    {
        [Export] public string SfxToPlay = "Splash";
        
        private CameraBridge cameraBridge;
        private LevelManager levelManager;
        private SaveManager saveManager;
        private SFXPlayer3D sfxPlayer3D;

        public override void _Ready()
        {
            saveManager = GetNode<SaveManager>("/root/SaveManager");
            sfxPlayer3D = GetNode<SFXPlayer3D>("/root/SFXPlayer3D");
            cameraBridge = GetNode<CameraBridge>("/root/CameraBridge");
            levelManager = GetNode<LevelManager>("/root/LevelManager");
            BodyEntered += OnBodyEntered;
        }

        private void OnBodyEntered(Node body)
        {
            if (body is CharacterBody3D characterBody)
            {
                Marker3D sp = saveManager.FindLastSpawnpoint();

                cameraBridge.Blink();
                sfxPlayer3D.PlaySound(SfxToPlay, characterBody.GlobalPosition);
                characterBody.GlobalTransform = new Transform3D(characterBody.GlobalTransform.Basis, sp.GlobalTransform.Origin);
            }
        }

    }

}

