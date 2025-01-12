using Godot;

namespace Game
{
    public partial class Sky3DTimeSaver : Node
    {
        private Node timeOfDay;
        private SaveManager saveManager;

        public override void _Ready()
        {
            timeOfDay = GetNode<Node>("Sky3D/TimeOfDay");
            saveManager = GetNode<SaveManager>("/root/SaveManager");

            if (timeOfDay == null || saveManager == null)
            {
                GD.PrintErr("[Sky3DTimeSaver] Null refs");
            }

            saveManager.BeforeSave += OnBeforeSave;
            RestoreTimeOfDay();
        }

        public void RestoreTimeOfDay()
        {
            int restoredTime = saveManager.GetTimeOfDay();
            
            timeOfDay.Call("set_from_unix_timestamp", restoredTime);
            GD.Print("[Sky3DTimeSaver] Restored time: ", restoredTime);
        }

        public void SaveTimeOfDay()
        {
            int savedTime = (int)timeOfDay.Call("get_unix_timestamp");
            saveManager.SetTimeOfDay(savedTime);
            GD.Print("[Sky3DTimeSaver] Saved time: ", savedTime);
        }

        private void OnBeforeSave()
        {
            SaveTimeOfDay();
        }

    }

}

