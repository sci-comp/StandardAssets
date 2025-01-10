using Godot;
using System.Collections.Generic;

namespace Game
{
    [Tool]
    public partial class EnvironmentController : Node
    {
        private bool skyIsEnbalbed = false;
        private LevelManager levelManager;
        private Node sky3D;
        private GrassMaterialUpdater grassMaterialUpdater;
        private SaveManager saveManager;

        public override void _Ready()
        {
            grassMaterialUpdater = GetNode<GrassMaterialUpdater>("Grass/MaterialUpdater");
            sky3D = GetNode<Node>("Sky3D");

            List<MultiMeshInstance3D> grassNodes = new();
            foreach (Node child in GetChildren())
            {
                if (child is MultiMeshInstance3D grassNode)
                {
                    grassNodes.Add(grassNode);
                }
            }

            if (grassNodes == null || grassNodes.Count == 0)
            {
                GD.PrintErr("[GrassMaterialUpdater] Grass nodes not found");
            }
            else
            {
                grassMaterialUpdater.Initialize(sky3D, grassNodes);
            }

            if (!Engine.IsEditorHint())
            {
                saveManager = GetNode<SaveManager>("/root/SaveManager");
                levelManager = GetNode<LevelManager>("/root/LevelManager");
                levelManager.BeginUnloadingLevel += OnBeginUnloadingLevel;
                bool _skyEnabled = levelManager.CurrentLevelInfo.EnableSky;
                SetSky(_skyEnabled);
                if (_skyEnabled)
                {
                    SetCurrentTime(saveManager.GetTime());
                }
                GD.Print("[EnvironmentController] Ready");
            }
        }

        public float GetCurrentTime()
        {
            return (float)sky3D.Get("current_time");
        }

        public void SetCurrentTime(float time)
        {
            sky3D.Call("set_current_time", time);
        }

        public void SetSky3D(bool toggle)
        {
            SetSky(toggle);
            SetLights(toggle);
            SetFog(toggle);
            SetClouds(toggle);
        }

        public void SetSky(bool toggle)
        {
            sky3D.Call("set_sky_enabled", toggle);
        }

        public void SetLights(bool toggle)
        {
            sky3D.Call("set_lights_enabled", toggle);
        }

        public void SetFog(bool toggle)
        {
            sky3D.Call("set_fog_enabled", toggle);
        }

        public void SetClouds(bool toggle)
        {
            sky3D.Call("set_clouds_enabled", toggle);
        }

        private void OnBeginUnloadingLevel(string levelID, string spawnpoint)
        {
            // Save time to SaveManager
        }

    }

}

