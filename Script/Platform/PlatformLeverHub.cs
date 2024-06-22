using Godot;
using System.Collections.Generic;

public partial class PlatformLeverHub : Node
{
    private List<Lever> levers = new();
    private List<ITriggeredPlatform> platforms = new();

    public override void _Ready()
    {
        Toolbox.FindAndPopulate(this, levers);
        Toolbox.FindAndPopulate(this, platforms);

        foreach (Lever lever in levers)
        {
            lever.Interacted += OnInteract;
        }
    }

    private void OnInteract()
    {        
        foreach (ITriggeredPlatform platform in platforms)
        {
            platform.Trigger();
        }
    }

}

