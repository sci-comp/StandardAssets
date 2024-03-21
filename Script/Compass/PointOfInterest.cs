using Godot;
using System;

public partial class PointOfInterest : Node3D
{
    [Export] public Texture2D IconTexture;

    public TextureRect IconRepresentation { get; set; }

    public override void _Ready()
    {
        AddToGroup("POI");
    }
}
