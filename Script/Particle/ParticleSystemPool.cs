using Godot;
using System.Collections.Generic;

/// <summary>
/// When TriggerParticleSystemAt is called, the first GpuParticles3D in line is,
/// dequeued, stopped, started, then enqueued.
/// </summary>
public partial class ParticleSystemPool : Node
{
    private Queue<GpuParticles3D> particleSystemsQueue = new();
    private GpuParticles3D[] Pool;

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

