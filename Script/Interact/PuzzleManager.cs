using Godot;
using System.Collections.Generic;

namespace Game
{
    public partial class PuzzleManager : Node
    {
        private readonly List<Lever> levers = new();
        private readonly List<IUnlockable> unlockables = new();

        public override void _Ready()
        {
            Toolbox.FindAndPopulate(this, levers);
            Toolbox.FindAndPopulate(this, unlockables);

            foreach (Lever lever in levers)
            {
                lever.Interacted += OnInteract;
                lever.Reusable = false;
            }
        }

        private void OnInteract()
        {
            foreach (var lever in levers)
            {
                if (!lever.Activated) return;
            }

            foreach (IUnlockable unlockable in unlockables)
            {
                unlockable.Unlock();
            }
        }

    }

}

