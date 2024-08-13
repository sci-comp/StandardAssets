using Godot;
using System;

namespace Game
{
    public partial class HiddenReward : Node3D, IFlag
    {
        public event Action OnFlagRaised;
        public event Action OnFlagLowered;

        public override void _Ready()
        {
            Visible = false;
        }

        public void RaiseFlag()
        {
            Visible = true;
            OnFlagRaised?.Invoke();
        }

        public void LowerFlag()
        {
            Visible = false;
            OnFlagLowered?.Invoke();
        }

    }

}

