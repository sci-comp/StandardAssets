using Godot;

namespace Game
{
    public partial class HiddenReward : Node3D, IUnlockable
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

}

