using Godot;
using System.Collections.Generic;

namespace Game
{
    public partial class Compass : Panel
    {
        [Export] public bool BeginVisible = false;

        private TextureRect north;
        private TextureRect east;
        private TextureRect south;
        private TextureRect west;
        private Label label;
        private Label labelDistance;

        public bool UseMetricUnits = false;
        private const float VerticalThreshold = 4.0f;

        private bool isEnabled = true;
        private Camera3D mainCamera;
        private const int MaxPOIs = 32;
        private readonly List<PointOfInterest> listOfPointsOfInterest = [];

        private CameraBridge cameraBridge;
        private LevelManager levelManager;
        private PlayerSpawner playerSpawner;

        private const float fadePercentage = 0.30f;
        private const float HighlightThresholdDegrees = 5.0f;

        public override void _Ready()
        {
            cameraBridge = GetNode<CameraBridge>("/root/CameraBridge");
            levelManager = GetNode<LevelManager>("/root/LevelManager");
            playerSpawner = GetNode<PlayerSpawner>("/root/PlayerSpawner");

            if (cameraBridge == null)
            {
                GD.Print("[Compass] Camera bridge is null");
            }
            else
            {
                mainCamera = cameraBridge.MainCamera;

                if (mainCamera == null)
                {
                    GD.PrintErr("[Compass] Camera is null");
                }
            }

            north = GetNode<TextureRect>("North");
            east = GetNode<TextureRect>("East");
            south = GetNode<TextureRect>("South");
            west = GetNode<TextureRect>("West");
            label = GetNode<Label>("Label");
            labelDistance = GetNode<Label>("LabelDistance");

            label.Visible = false;
            labelDistance.Visible = false;

            PointOfInterest.POISpawned += OnPOISpawned;
            PointOfInterest.POIDestroyed += OnPOIDestroyed;
            levelManager.BeginUnloadingLevel += OnBeginUnloadingLevel;

            Visible = BeginVisible;
            SetVisibility(true);
            GD.PrintRich($"[Compass] [color={ColorsHex.MediumSeaGreen}]Ready[/color]");
        }

        public override void _Process(double delta)
        {
            UpdateCompass();
        }

        public override void _ExitTree()
        {
            PointOfInterest.POISpawned -= OnPOISpawned;
            PointOfInterest.POIDestroyed -= OnPOIDestroyed;
            levelManager.BeginUnloadingLevel -= OnBeginUnloadingLevel;
        }

        public void SetVisibility(bool visible)
        {
            Visible = visible;
        }

        private void OnBeginUnloadingLevel(string nextLevel, string nextSpawnpoint)
        {
            GD.Print("[Compass] On begin unloading level, setting visible to true");
            Visible = false;
        }
        
        private void OnPOIDestroyed(PointOfInterest poi)
        {
            GD.Print("[Compass] Removing PoI... ", poi.DisplayName);
            if (listOfPointsOfInterest.Contains(poi))
            {
                listOfPointsOfInterest.Remove(poi);
                poi.IconRepresentation?.QueueFree();
            }
        }

        private void OnPOISpawned(PointOfInterest poi)
        {
            var poiIcon = new TextureRect
            {
                Texture = poi.IconTexture,
                Name = poi.Name + "_Icon",
                Visible = false,
                StretchMode = TextureRect.StretchModeEnum.KeepAspect,
                SelfModulate = new("f5d7b7")
            };
            AddChild(poiIcon);

            //
            float verticalOffset = (Size.Y / 2) - (poiIcon.Size.Y / 2);
            poiIcon.Position = new Vector2(poiIcon.Position.X, verticalOffset);

            poi.IconRepresentation = poiIcon;
            listOfPointsOfInterest.Add(poi);
            GD.Print("[Compass] PoI added: ", poi.DisplayName);
        }

        private void UpdateCompass()
        {
            if (mainCamera == null)
            {
                GD.PrintErr("[Compass] Cannot find camera");
                return;
            }

            label.Visible = false;
            labelDistance.Visible = false;

            var playerOrientation = mainCamera.GlobalTransform.Basis.GetEuler().Y;
            var cameraPosition = mainCamera.GlobalTransform.Origin;

            UpdateIndicatorPosition(north, playerOrientation, 0);
            UpdateIndicatorPosition(west, playerOrientation, Mathf.Pi / 2);
            UpdateIndicatorPosition(south, playerOrientation, Mathf.Pi);
            UpdateIndicatorPosition(east, playerOrientation, 3 * Mathf.Pi / 2);

            if (Visible && listOfPointsOfInterest.Count > 0)
            {
                int count = 0;
                foreach (PointOfInterest poi in listOfPointsOfInterest)
                {
                    // Skip POIs that are hidden from the compass
                    if (!poi.IsVisibleOnCompass() || 
                        poi.IconRepresentation == null ||
                        !poi.IsInRangeWithFade(cameraPosition, fadePercentage))
                    {
                        if (poi.IconRepresentation != null)
                        {
                            poi.IconRepresentation.Visible = false;
                        }

                        continue;
                    }

                    if (count < MaxPOIs)
                    {
                        float opacity = poi.GetOpacityForDistance(cameraPosition, fadePercentage);
                        if (opacity <= 0.0f)
                        {
                            poi.IconRepresentation.Visible = false;
                            continue;
                        }
                        Color modulate = poi.IconRepresentation.SelfModulate;
                        modulate.A = opacity;
                        poi.IconRepresentation.SelfModulate = modulate;

                        Vector3 directionToPOI = (poi.GlobalTransform.Origin - mainCamera.GlobalTransform.Origin).Normalized();
                        float poiDirectionAngle = Mathf.Atan2(directionToPOI.X, directionToPOI.Z) + Mathf.Pi;
                        poiDirectionAngle = Mathf.PosMod(poiDirectionAngle, Mathf.Pi * 2);
                        float distance = cameraPosition.DistanceTo(poi.GlobalTransform.Origin);
                        UpdateIndicatorPosition(poi.IconRepresentation, playerOrientation, poiDirectionAngle, poi, distance);
                        count++;
                    }
                    else
                    {
                        poi.IconRepresentation.Visible = false;
                    }
                }
            }
        }

        private void UpdateIndicatorPosition(TextureRect indicator, float playerOrientation, float directionAngle, PointOfInterest poi=null, float distance=0.0f)
        {
            playerOrientation = Mathf.PosMod(playerOrientation, Mathf.Pi * 2);
            float angleDifference = Mathf.Wrap(directionAngle - playerOrientation, -Mathf.Pi, Mathf.Pi);
            bool isInFrontOfPlayer = Mathf.Abs(angleDifference) <= Mathf.Pi / 2;
            bool isHighlighted = Mathf.Abs(angleDifference) <= Mathf.DegToRad(HighlightThresholdDegrees);

            if (isInFrontOfPlayer)
            {
                float normalizedPosition = -1.0f * angleDifference / (Mathf.Pi / 2);
                float scaledPosition = normalizedPosition * Size.X / 2;
                float offset = (Size.X / 2) - (indicator.Size.X / 2);
                indicator.Position = new Vector2(scaledPosition + offset, indicator.Position.Y);
                indicator.Visible = true;

                if (poi != null && isHighlighted)
                {
                    label.Visible = true;
                    label.Text = poi.DisplayName;

                    float opacity = indicator.SelfModulate.A;
                    bool showLabels = opacity > 0.5f;

                    if (showLabels)
                    {
                        if (distance > 0.0f)
                        {
                            float verticalDifference = poi.GlobalTransform.Origin.Y - mainCamera.GlobalTransform.Origin.Y;
                            string verticalIndicator = "";

                            if (verticalDifference > VerticalThreshold)
                            {
                                verticalIndicator = " (above)";
                            }
                            else if (verticalDifference < -VerticalThreshold)
                            {
                                verticalIndicator = " (below)";
                            }

                            string distanceText;
                            if (UseMetricUnits)
                            {
                                distanceText = $"{Mathf.Round(distance)}m{verticalIndicator}";
                            }
                            else
                            {
                                float distanceInFeet = distance * 3.28084f;
                                distanceText = $"{Mathf.Round(distanceInFeet)}'{verticalIndicator}";
                            }

                            labelDistance.Visible = true;
                            labelDistance.Text = distanceText;
                        }
                    }
                }
            }
            else
            {
                indicator.Visible = false;
            }
        }

    }

}

