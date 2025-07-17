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
        [Export] public NodePath[] AnglePaths { get; set; } = [];
        [Export] public CameraAngle DefaultAngle { get; set; } = CameraAngle.Closeup;

        private Node3D[] _angles;

        public override void _Ready()
        {
            if (Engine.IsEditorHint())
            {
                return;
            }

            // Resolve paths to actual nodes at runtime only
            _angles = new Node3D[AnglePaths.Length];
            for (int i = 0; i < AnglePaths.Length; i++)
            {
                _angles[i] = GetNode<Node3D>(AnglePaths[i]);
            }

            if (_angles.Length == 0)
            {
                GD.PushWarning($"[CameraAngles] No camera angles found");
            }
        }

        public Node3D GetAngle(string name)
        {
            if (Engine.IsEditorHint())
            {
                return null;
            }

            for (int i = 0; i < _angles.Length; i++)
            {
                if (_angles[i].Name == name)
                    return _angles[i];
            }
            GD.Print("[CameraAngle] Angle not found: ", name);
            return null;
        }

        public Node3D GetAngle(CameraAngle angle)
        {
            if (Engine.IsEditorHint())
            {
                return null;
            }

            string angleName = angle.ToString();
            return GetAngle(angleName);
        }

        public void SetCameraPriority(CameraAngle angle, int priority = 10)
        {
            if (Engine.IsEditorHint())
            {
                return;
            }

            Node3D camera = GetAngle(angle);
            camera?.Set("priority", priority);
        }

        public void ResetCameraPriorities()
        {
            if (Engine.IsEditorHint())
            {
                return;
            }

            foreach (Node3D camera in _angles)
            {
                camera.Set("priority", 0);
            }
        }


    }

}

