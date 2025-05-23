using Godot;

namespace Game
{
    public partial class CameraAngles : Node
    {
        [Export] public Node3D[] Angles { get; set; } = [];

        public override void _Ready()
        {
            if (Angles.Length == 0)
            {
                GD.PushWarning($"[CameraAngles] No camera angles found");
            }
        }

        public Node3D GetAngle(string name)
        {
            for (int i = 0; i < Angles.Length; i++)
            {
                if (Angles[i].Name == name)
                {
                    return Angles[i];
                }
            }
            return null;
        }
    }

}

