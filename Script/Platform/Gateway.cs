using Godot;
using System.Threading.Tasks;

public partial class Gateway : Area3D
{
    [Export] Gateway otherGateway;
    
    private bool timedOut = false;
    private float timeoutDuration = 1.0f;

    private float t_current = 0.0f;
    private float checkEvery = 0.15f;
    private bool checkAgain = false;
    private Vector3 spawnPosition;

    public Vector3 SpawnPosition => spawnPosition;

    public override void _Ready()
    {
        spawnPosition.X = Position.X;
        spawnPosition.Y = Position.Y + 1.5f;
        spawnPosition.Z = Position.Z;
    }

    public override void _Process(double _dt)
    {
        float dt = (float)_dt;

        if (t_current < checkEvery)
        {
            t_current += dt;
        }
        else
        {
            t_current = 0.0f;

            if (!timedOut && HasOverlappingBodies())
            {
                Node3D other = GetOverlappingBodies()[0];

                if (other is CharacterBody3D character)
                {
                    if (Mathf.IsZeroApprox(character.Velocity.Length()))
                    {
                        _ = otherGateway.TimeOut();
                        character.Position = otherGateway.SpawnPosition;
                    }
                }
            }
        }
    }

    public async Task TimeOut()
    {
        if (!timedOut)
        {
            timedOut = true;
            await ToSignal(GetTree().CreateTimer(timeoutDuration), "timeout");
            timedOut = false;
        }
    }

}

