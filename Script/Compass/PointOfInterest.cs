using Godot;
using System;


namespace Game
{
    public partial class PointOfInterest : Node3D
    {
        [Export] public Texture2D IconTexture;
        [Export] public string DisplayName = "";

        public TextureRect IconRepresentation { get; set; }

        public static event Action<PointOfInterest> POISpawned;
        public static event Action<PointOfInterest> POIDestroyed;

        public override void _Ready()
        {
            AddToGroup("POI");
            POISpawned?.Invoke(this);
            GD.Print("[PointOfInterest] Ready: ", Name);
            GetTree().ProcessFrame += OnFirstProcess;
        }

        private void OnFirstProcess()
        {
            GetTree().ProcessFrame -= OnFirstProcess;
            POISpawned?.Invoke(this);
        }

    }

}

