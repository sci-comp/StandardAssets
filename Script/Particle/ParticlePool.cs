using Godot;
using System.Collections.Generic;

/// <summary>
/// When TriggerParticleSystemAt is called, the first GpuParticles3D in line is,
/// dequeued, stopped, started, then enqueued.
/// </summary>
namespace Game
{
    public partial class ParticlePool : Node
    {
        private Queue<GpuParticles3D> particleSystemsQueue = new();

        public override void _Ready()
        {
            foreach (Node child in GetChildren())
            {
                if (child is GpuParticles3D particles)
                {
                    particles.Visible = false;
                    particles.Emitting = false;
                    particleSystemsQueue.Enqueue(particles);
                }
            }
        }

        public void TriggerParticleSystemAt(Vector3 position)
        {
            if (particleSystemsQueue.Count == 0)
            {
                GD.PrintErr("[ParticleSystemPool] Queue is empty, returning early: ", Name);
                return;
            }

            GpuParticles3D particles = particleSystemsQueue.Dequeue();

            if (particles.Emitting)
            {
                particles.Emitting = false;
            }

            particles.Restart();
            particles.Position = position;
            particles.Visible = true;
            particles.Emitting = true;

            particleSystemsQueue.Enqueue(particles);
        }
    }

}

