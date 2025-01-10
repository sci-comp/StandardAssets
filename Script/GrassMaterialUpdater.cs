using Godot;
using System.Collections.Generic;

namespace Game
{
    [Tool]
    public partial class GrassMaterialUpdater : Node
    {
        private float timer = 0.0f;
        private DirectionalLight3D sun;
        private DirectionalLight3D moon;
        private List<MultiMeshInstance3D> grassNodes;
        private Node sky3D;
        private bool initialized = false;
        public void Initialize(Node _sky3D, List<MultiMeshInstance3D> _grassNodes)
        {
            sky3D = _sky3D;
            grassNodes = _grassNodes;
            if (grassNodes != null && grassNodes.Count > 0)
            {
                initialized = true;
            }
            else
            {
                GD.PrintErr("[GrassMaterialUpdater] grassNodes null or empty");
            }

            sun = sky3D.GetNode<DirectionalLight3D>("SunLight");
            moon = sky3D.GetNode<DirectionalLight3D>("MoonLight");
            GD.Print("[GrassMaterialUpdater] Ready");
        }

        public override void _Process(double delta)
        {
            timer += (float)delta;
            if (timer >= (float)sky3D.Get("update_interval"))
            {
                timer = 0.0f;
                UpdateShaderParameters();
            }
        }

        private void UpdateShaderParameters()
        {
            foreach (MultiMeshInstance3D grassNode in grassNodes)
            {
                Material material = grassNode.MaterialOverride;

                if (material == null && grassNode.Multimesh != null && grassNode.Multimesh.Mesh != null)
                {
                    material = grassNode.Multimesh.Mesh.SurfaceGetMaterial(0);
                }

                if (material != null)
                {
                    SetShaderParams(material);
                }
            }
        }

        private void SetShaderParams(Material material)
        {
            if (material is ShaderMaterial shaderMaterial)
            {
                bool sunVisible = (bool)sun.Get("visible");
                bool moonVisible = (bool)moon.Get("visible");
                if (sunVisible)
                {
                    Transform3D sunTransform = (Transform3D)sun.Get("global_transform");
                    Vector3 sunDir = -sunTransform.Basis.Z;
                    shaderMaterial.SetShaderParameter("light_direction", sunDir);
                    shaderMaterial.SetShaderParameter("light_color", sun.Get("light_color"));
                    shaderMaterial.SetShaderParameter("light_energy", sun.Get("light_energy"));
                }
                else if (moonVisible)
                {
                    Transform3D moonTransform = (Transform3D)moon.Get("global_transform");
                    Vector3 moonDir = -moonTransform.Basis.Z;
                    shaderMaterial.SetShaderParameter("light_direction", moonDir);
                    shaderMaterial.SetShaderParameter("light_color", moon.Get("light_color"));
                    shaderMaterial.SetShaderParameter("light_energy", moon.Get("light_energy"));
                }
            }
        }

    }


}
