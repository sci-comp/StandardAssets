using Godot;
using Toolbox;

namespace Game
{
    public partial class Sky3DTimeSaver : Node
    {
        private LevelManager levelManager;
        private Node sky3D;
        private SaveManager saveManager;

        public override void _Ready()
        {
            sky3D = GetNode<Node>("Sky3D");
            saveManager = GetNode<SaveManager>("/root/SaveManager");
            levelManager = GetNode<LevelManager>("/root/LevelManager");
            levelManager.BeginUnloadingLevel += OnBeginUnloadingLevel;
            RestoreTimeOfDay();
        }

        public float GetCurrentTimeOfDay()
        {
            return (float)sky3D.Get("current_time");
        }

        public void RestoreTimeOfDay()
        {
            float restoredTime = saveManager.GetTimeOfDay();
            sky3D.Call("set_current_time", restoredTime);
            GD.Print("[EnvironmentController] Restored time: ", restoredTime);
        }

        public void SaveTimeOfDay()
        {
            float savedTime = (float)sky3D.Get("current_time");
            saveManager.SetTimeOfDay(savedTime);
        }

        private void OnBeginUnloadingLevel(string levelID, string spawnpoint)
        {
            SaveTimeOfDay();
        }

    }

}

