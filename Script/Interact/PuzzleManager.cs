using Godot;
using System.Collections.Generic;

public partial class PuzzleManager : Node
{
    /*
    private List<Interactable> levers = new();
    private Unlockable unlockable;

    public override void _Ready()
    {
        foreach (Node child in GetNode("Levers").GetChildren())
        {
            if (child is Interactable lever)
            {
                //lever
                levers.Add(lever);
            }
        }

        unlockable = GetNode<Unlockable>("UnlockableObject");
    }

    private void OnLeverInteracted()
    {
        foreach (var lever in levers)
        {
            if (!lever.IsActivated()) return;
        }

        unlockable.Unlock();
    }
    */
}
