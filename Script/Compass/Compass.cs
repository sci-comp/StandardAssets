using Godot;
using System.Collections.Generic;

public partial class Compass : Camera3D
{
    private TextureRect northIndicator;
    private TextureRect eastIndicator;
    private TextureRect southIndicator;
    private TextureRect westIndicator;

    // Using TextureRect for lineContainer to utilize Size
    private Control lineContainer;
    private Camera3D playerCamera;
    private const int MaxPOIs = 32;

    public override void _Ready()
    {
        playerCamera = this;

        northIndicator = GetNode<TextureRect>("Compass/LineContainer/NorthIndicator");
        eastIndicator = GetNode<TextureRect>("Compass/LineContainer/EastIndicator");
        southIndicator = GetNode<TextureRect>("Compass/LineContainer/SouthIndicator");
        westIndicator = GetNode<TextureRect>("Compass/LineContainer/WestIndicator");

        lineContainer = GetNode<Control>("Compass");

        if (lineContainer == null)
        {
            GD.Print("null lineContainer");
        }

        // Register POIs by finding all nodes in the "POI" group
        var pois = GetTree().GetNodesInGroup("POI");
        foreach (Node poiNode in pois)
        {
            if (poiNode is PointOfInterest poi && poi.IconTexture != null)
            {
                var poiIcon = new TextureRect
                {
                    Texture = poi.IconTexture,
                    Name = poi.Name + "_Icon",
                    Visible = false,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspect
                };
                lineContainer.AddChild(poiIcon);
                poi.IconRepresentation = poiIcon;
            }
        }
    }

    public override void _Process(double delta)
    {
        UpdateCompass();
    }

    private void UpdateCompass()
    {
        var playerOrientation = playerCamera.GlobalTransform.Basis.GetEuler().Y;
        UpdateIndicatorPosition(northIndicator, playerOrientation, 0);
        UpdateIndicatorPosition(westIndicator, playerOrientation, Mathf.Pi / 2);
        UpdateIndicatorPosition(southIndicator, playerOrientation, Mathf.Pi);
        UpdateIndicatorPosition(eastIndicator, playerOrientation, 3 * Mathf.Pi / 2);

        var pois = GetTree().GetNodesInGroup("POI");
        int count = 0;
        foreach (Node poiNode in pois)
        {
            if (poiNode is PointOfInterest poi && count < MaxPOIs)
            {
                Vector3 directionToPOI = (poi.GlobalTransform.Origin - playerCamera.GlobalTransform.Origin).Normalized();
                float poiDirectionAngle = Mathf.Atan2(directionToPOI.X, directionToPOI.Z) + Mathf.Pi;
                poiDirectionAngle = Mathf.PosMod(poiDirectionAngle, Mathf.Pi * 2);
                UpdateIndicatorPosition(poi.IconRepresentation, playerOrientation, poiDirectionAngle);
                count++;
            }
        }
    }


    private void UpdateIndicatorPosition(TextureRect indicator, float playerOrientation, float directionAngle)
    {
        playerOrientation = Mathf.PosMod(playerOrientation, Mathf.Pi * 2);
        float angleDifference = Mathf.Wrap(directionAngle - playerOrientation, -Mathf.Pi, Mathf.Pi);

        bool isInFrontOfPlayer = Mathf.Abs(angleDifference) <= Mathf.Pi / 2;

        if (isInFrontOfPlayer)
        {
            float normalizedPosition = angleDifference / (Mathf.Pi / 2);
            float positionX = normalizedPosition * (lineContainer.Size.X / 2) + (lineContainer.Size.X / 2) - (indicator.Size.X / 2);
            indicator.Position = new Vector2(positionX, indicator.Position.Y);
            indicator.Visible = true;
        }
        else
        {
            indicator.Visible = false;
        }
    }

}

