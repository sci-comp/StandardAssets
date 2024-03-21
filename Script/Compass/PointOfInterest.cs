using Godot;
using System;

public partial class PointOfInterest : Area3D
{
    [Export] public Texture2D IconTexture;

    public TextureRect IconRepresentation { get; set; }

    public static event Action<PointOfInterest> POISpawned;
    public static event Action<PointOfInterest> POIDestroyed;

    public override void _Ready()
    {
        AddToGroup("POI");
        POISpawned?.Invoke(this);
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D other)
    {
        POIDestroyed?.Invoke(this);
        QueueFree();
    }

}

