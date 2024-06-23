using Godot;

namespace Game
{
    public partial class Gateway : Area3D
    {
        private Vector3 SpawnPosition;
        private Gateway otherGateway;

        public bool WaitingOnPlayerToExitPlatform = false;

        public void Initialize(Gateway _otherGateway)
        {
            SpawnPosition = new Vector3(Position.X, Position.Y + 1f, Position.Z);
            otherGateway = _otherGateway;

            BodyExited += OnBodyExit;
        }

        public void ActivateGateway(CharacterBody3D character)
        {
            if (otherGateway != null && character != null)
            {
                otherGateway.WaitingOnPlayerToExitPlatform = true;
                character.Position = otherGateway.SpawnPosition;
            }
        }

        private void OnBodyExit(Node3D body)
        {
            WaitingOnPlayerToExitPlatform = false;
        }

    }

}

