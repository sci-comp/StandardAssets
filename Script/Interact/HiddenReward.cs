using Godot;
using System;

public partial class HiddenReward : Node3D, Unlockable
{
    public override void _Ready()
    {
        Visible = false;
    }

    public void Unlock()
    {
        GD.Print("Treasure Chest Unlocked!");
        Visible = true;
        // Play smoke vfx
    }

}

