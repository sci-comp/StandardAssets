using Godot;
using System.Collections.Generic;

public partial class PlatformLeverHub : Node3D
{
    private List<Lever> levers = new();
    private List<ITriggeredPlatform> platforms = new();

    public override void _Ready()
    {
        FindAndPopulate(this, levers);
        FindAndPopulate(this, platforms);

        foreach (Lever lever in levers)
        {
            lever.Interacted += OnInteract;
        }
    }

    private void FindAndPopulate<T>(Node node, List<T> list) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T item)
            {
                list.Add(item);
            }

            FindAndPopulate(child, list);
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

