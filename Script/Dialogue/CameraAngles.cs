using Godot;

namespace Game
{
    public enum CameraAngle
    {
        Bird,
        Closeup,
        CloseupLeft,
        CloseupRight,
        ExtremeCloseup,
        Full,
        FullLeft,
        FullRight,
        HighAngle,
        HighAngleLeft,
        HighAngleRight,
        LowAngle,
        LowAngleLeft,
        LowAngleRight,
        Medium,
        MediumLeft,
        MediumRight,
        OverLeft,
        OverRight,
        ProfileLeft,
        ProfileRight,
        Reverse,
        Worm
    }

    public partial class CameraAngles : Node3D
    {
        [Export] public Node3D[] Angles { get; set; } = [];
        [Export] public CameraAngle DefaultAngle { get; set; } = CameraAngle.Closeup;

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
            GD.Print("[CameraAngle] Angle not found: ", name);
            return null;
        }

        public Node3D GetAngle(CameraAngle angle)
        {
            string angleName = angle.ToString();
            return GetAngle(angleName);
        }

        public void SetCameraPriority(CameraAngle angle, int priority = 10)
        {
            Node3D camera = GetAngle(angle);
            camera?.Set("priority", priority);
        }

        public void ResetCameraPriorities()
        {
            foreach (Node3D camera in Angles)
            {
                camera.Set("priority", 0);
            }
        }


    }

}

