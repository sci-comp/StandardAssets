using Godot;
using System;

public partial class HiddenReward : Node3D, Unlockable
{
    public void Unlock()
    {
        GD.Print("Treasure Chest Unlocked!");
        // Add unlocking logic here (e.g., animation, open chest, etc.)
    }

}

