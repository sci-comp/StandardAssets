using Godot;
using System;

namespace Game
{
    public partial class PointOfInterest : Marker3D
    {
        [Export] public Texture2D IconTexture;
        [Export] public string DisplayName = "";

        [ExportCategory("Distance")]
        [Export] public bool UseDistance = false;
        [Export] public float MinDistance = 0.0f;
        [Export] public float MaxDistance = 100.0f;
        public TextureRect IconRepresentation { get; set; }
        private bool isVisibleOnCompass = true;

        public static event Action<PointOfInterest> POISpawned;
        public static event Action<PointOfInterest> POIDestroyed;

        public override void _Ready()
        {
            AddToGroup("POI");
            GD.Print("[PointOfInterest] Ready: ", Name);
            GetTree().ProcessFrame += OnFirstProcess;
        }

        private void OnFirstProcess()
        {
            GetTree().ProcessFrame -= OnFirstProcess;
            POISpawned?.Invoke(this);
        }

        public override void _ExitTree()
        {
            POIDestroyed?.Invoke(this);
        }

        public void HideFromCompass()
        {
            if (isVisibleOnCompass)
            {
                isVisibleOnCompass = false;
                if (IconRepresentation != null)
                {
                    IconRepresentation.Visible = false;
                }
                GD.Print($"[PointOfInterest] {DisplayName} hidden from compass");
            }
        }

        public void ShowOnCompass()
        {
            if (!isVisibleOnCompass)
            {
                isVisibleOnCompass = true;
                GD.Print($"[PointOfInterest] {DisplayName} shown on compass");
                // Actual visibility will be updated on next compass update
            }
        }

        public bool IsVisibleOnCompass()
        {
            return isVisibleOnCompass;
        }

        public bool IsInRange(Vector3 cameraPosition)
        {
            if (!UseDistance)
            {
                return true;
            }

            float distance = GlobalPosition.DistanceTo(cameraPosition);
            return distance >= MinDistance && distance <= MaxDistance;
        }

        public float GetOpacityForDistance(Vector3 cameraPosition, float fadePercentage)
        {
            float distance = GlobalPosition.DistanceTo(cameraPosition);

            if (distance <= MaxDistance)
            {
                return 1.0f;
            }

            float fadeDistance = MaxDistance * fadePercentage;
            float maxVisibleDistance = MaxDistance + fadeDistance;

            if (distance >= maxVisibleDistance)
            {
                return 0.0f;
            }

            float fadeProgress = (distance - MaxDistance) / fadeDistance;
            return 1.0f - fadeProgress;
        }

        public bool IsInRangeWithFade(Vector3 cameraPosition, float fadePercentage)
        {
            float distance = GlobalPosition.DistanceTo(cameraPosition);
            float maxVisibleDistance = MaxDistance + (MaxDistance * fadePercentage);
            return distance >= MinDistance && distance <= maxVisibleDistance;
        }

    }

}

