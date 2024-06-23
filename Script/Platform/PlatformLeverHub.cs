using Godot;
using System.Collections.Generic;

namespace Game
{
    public partial class PlatformLeverHub : Node
    {
        private readonly List<Lever> levers = new();
        private readonly List<IActivatedPlatform> platforms = new();

        public override void _Ready()
        {
            Toolbox.FindAndPopulate(this, levers);
            Toolbox.FindAndPopulate(this, platforms);

            foreach (Lever lever in levers)
            {
                lever.Interacted += OnInteract;
            }
        }

        private void OnInteract()
        {
            foreach (IActivatedPlatform platform in platforms)
            {
                platform.Activate();
            }
        }

    }

}

