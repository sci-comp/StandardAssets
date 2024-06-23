using Godot;

namespace Game
{
    public partial class PlayParticlesOnEnter : Area3D
    {
        private GpuParticles3D particles;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            particles = GetNode<GpuParticles3D>("./vfx_confetti");
        }

        private void OnBodyEntered(Node3D other)
        {
            particles.Emitting = true;
        }

    }

}

