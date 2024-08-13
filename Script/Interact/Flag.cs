using Godot;
using System;

namespace Game
{
    public partial class Lock : Node, IFlag, ISaveable
    {
        public bool FlagRaised = false;
        public event Action OnFlagRaised;
        public event Action OnFlagLowered;
        private SaveManager saveManager;

        public override void _Ready()
        {
            saveManager = GetNode<SaveManager>("/root/SaveManager");
            saveManager.RegisterSaveable(this);
        }

        public override void _ExitTree()
        {
            saveManager.UnregisterSaveable(this);
        }

        public void ApplyData()
        {
            if (saveManager.HasBooleanValue(Name))
            {
                FlagRaised = saveManager.GetBooleanValue(Name);
            }
            else
            {
                saveManager.UpdateBooleanValue(Name, FlagRaised);
            }

            if (saveManager.HasBooleanValue(Name))
            {
                FlagRaised = saveManager.GetBooleanValue(Name);
            }
        }

        public void RecordData()
        {
            saveManager.UpdateBooleanValue(Name, FlagRaised);
        }

        public void RaiseFlag()
        {
            if (!FlagRaised)
            {
                FlagRaised = true;
                OnFlagRaised?.Invoke();
                GD.Print("[Unlockable] Unlocked, ", Name);
            }
        }

        public void LowerFlag()
        {
            OnFlagLowered?.Invoke();
        }
    }

}
