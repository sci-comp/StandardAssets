using Godot;
using System.Collections.Generic;

public partial class GatewayHub : Node3D
{
    private List<Gateway> gateways = new();
    private float timeoutDuration = 1.0f;
    private float t_current = 0.0f;
    private bool isTimeout = false;

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is Gateway gateway)
            {
                gateways.Add(gateway);
            }
        }

        for (int i = 0; i < gateways.Count; i++)
        {
            gateways[i].Initialize(gateways[(i + 1) % gateways.Count]);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isTimeout)
        {
            t_current += (float)delta;
            if (t_current >= timeoutDuration)
            {
                isTimeout = false;
                t_current = 0.0f;
            }
            return;
        }

        foreach (var gateway in gateways)
        {
            if (gateway.HasOverlappingBodies() && !gateway.WaitingOnPlayerToExitPlatform)
            {
                var bodies = gateway.GetOverlappingBodies();
                foreach (var body in bodies)
                {
                    if (body is CharacterBody3D character && Mathf.IsZeroApprox(character.Velocity.Length()))
                    {
                        gateway.ActivateGateway(character);
                        isTimeout = true;
                        return;
                    }
                }
            }
        }
    }

}

