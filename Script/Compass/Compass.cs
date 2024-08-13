using Godot;
using System.Collections.Generic;

namespace Game
{
    public partial class Compass : Control
    {
        private TextureRect northIndicator;
        private TextureRect eastIndicator;
        private TextureRect southIndicator;
        private TextureRect westIndicator;

        private Camera3D mainCamera;
        private const int MaxPOIs = 32;

        private readonly List<PointOfInterest> listOfPointsOfInterest = new();

        private CameraBridge cameraBridge;
        private LevelManager levelManager;

        public override void _Ready()
        {
            cameraBridge = GetNode<CameraBridge>("/root/CameraBridge");
            levelManager = GetNode<LevelManager>("/root/LevelManager");
            levelManager.BeginUnloadingLevel += OnBeginUnloadingLevel;

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
            
            northIndicator = GetNode<TextureRect>("NorthIndicator");
            eastIndicator = GetNode<TextureRect>("EastIndicator");
            southIndicator = GetNode<TextureRect>("SouthIndicator");
            westIndicator = GetNode<TextureRect>("WestIndicator");

            PointOfInterest.POISpawned += OnPOISpawned;
            PointOfInterest.POIDestroyed += OnPOIDestroyed;
        }

        public override void _Process(double delta)
        {
            UpdateCompass();
        }

        public override void _ExitTree()
        {
            PointOfInterest.POISpawned -= OnPOISpawned;
            PointOfInterest.POIDestroyed -= OnPOIDestroyed;
        }

        private void OnBeginUnloadingLevel()
        {
            Visible = false;
        }

        private void OnLevelLoaded()
        {
            if (levelManager.CurrentLevelInfo.PlayerExistsInLevel)
            {
                Visible = true;
            }
            else
            {
                Visible = false;
            }
        }

        private void OnPOIDestroyed(PointOfInterest poi)
        {
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
                StretchMode = TextureRect.StretchModeEnum.KeepAspect
            };
            AddChild(poiIcon);
            poi.IconRepresentation = poiIcon;
            listOfPointsOfInterest.Add(poi);
        }

        private void UpdateCompass()
        {
            var playerOrientation = mainCamera.GlobalTransform.Basis.GetEuler().Y;
            UpdateIndicatorPosition(northIndicator, playerOrientation, 0);
            UpdateIndicatorPosition(westIndicator, playerOrientation, Mathf.Pi / 2);
            UpdateIndicatorPosition(southIndicator, playerOrientation, Mathf.Pi);
            UpdateIndicatorPosition(eastIndicator, playerOrientation, 3 * Mathf.Pi / 2);

            int count = 0;
            foreach (PointOfInterest poi in listOfPointsOfInterest)
            {
                if (count < MaxPOIs)
                {
                    Vector3 directionToPOI = (poi.GlobalTransform.Origin - mainCamera.GlobalTransform.Origin).Normalized();
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
                float normalizedPosition = -1.0f * angleDifference / (Mathf.Pi / 2);
                float positionX = normalizedPosition * (Size.X / 2) + (Size.X / 2) - (indicator.Size.X / 2);
                indicator.Position = new Vector2(positionX, indicator.Position.Y);
                indicator.Visible = true;
            }
            else
            {
                indicator.Visible = false;
            }
        }

    }

}

