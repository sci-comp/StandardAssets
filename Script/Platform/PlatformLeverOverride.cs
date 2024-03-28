using Godot;
using System.Collections.Generic;

public partial class PlatformLeverOverride : Node3D
{
    [Export] public string Title = "Lever";
    [Export] public string Details = "";

    private List<PlatformLever> levers = new();
    private List<ITriggeredPlatform> platforms = new();

    public override void _Ready()
    {
        FindAndPopulate(this, levers);
        FindAndPopulate(this, platforms);

        foreach (PlatformLever lever in levers)
        {
            lever.Initialize(Title, Details, platforms);
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
}

