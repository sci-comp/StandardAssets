using Godot;
using System.Collections.Generic;

namespace Game
{
    public partial class PuzzleManager : Node
    {
        private readonly List<Lever> levers = new();
        private readonly List<IFlag> flags = new();

        public override void _Ready()
        {
            Toolbox.Toolbox.FindAndPopulate(this, levers);
            Toolbox.Toolbox.FindAndPopulate(this, flags);
            
            foreach (Lever lever in levers)
            {
                lever.Interacted += OnInteract;
            }
        }

        private void OnInteract(string playerID)
        {
            foreach (var lever in levers)
            {
                if (!lever.Activated) return;
            }

            foreach (IFlag flag in flags)
            {
                flag.RaiseFlag();
            }
        }

    }

}

