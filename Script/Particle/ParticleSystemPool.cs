using Godot;
using System.Collections.Generic;

/// <summary>
/// Manages a pool of particle systems, allowing for efficient reuse of the systems by
/// activating and deactivating them as needed. The pool is populated with pre-configured
/// particle systems, and provides a method to trigger a particle system at a specified position.
/// </summary>
public partial class ParticleSystemPool : Node
{
    [Export] public GpuParticles3D[] Pool;

    private readonly Queue<GpuParticles3D> particleSystemsQueue = new();

    public override void _Ready()
    {
        foreach (GpuParticles3D system in Pool)
        {
            system.Visible = false;
            particleSystemsQueue.Enqueue(system);
        }
    }

    public void TriggerParticleSystemAt(Vector3 position)
    {
        if (particleSystemsQueue.Count == 0)
        {
            return;
        }

        GpuParticles3D systemToUse = particleSystemsQueue.Dequeue();

        if (systemToUse.Emitting)
        {
            systemToUse.Emitting = false;
        }

        systemToUse.Position = position;
        systemToUse.Visible = true;
        systemToUse.Emitting = true;

        particleSystemsQueue.Enqueue(systemToUse);
    }

}

