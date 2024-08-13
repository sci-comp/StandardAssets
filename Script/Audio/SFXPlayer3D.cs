using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public partial class SFXPlayer3D : Node
    {
        private Preferences prefs;

        public float MasterVolume { get; set; } = 0.7f;
        public Dictionary<string, SoundGroup3D> SoundGroups = new();

        private MeshInstance3D debugSphere;

        public override void _Ready()
        {
            prefs = GetNode<Preferences>("/root/Preferences");
            debugSphere = GetNode<MeshInstance3D>("DebugSphere");

            if (prefs == null)
            {
                GD.PrintErr("[SFXPlayer3D] Null reference to a child node");
            }
            else
            {
                PossibleSFX3D possibleSFX3D = new();
                possibleSFX3D.Initialize(this);
                SoundGroups = possibleSFX3D.GetSoundGroups();
                foreach (SoundGroup3D soundGroup in SoundGroups.Values)
                {
                    soundGroup.Initialize(this);
                }
            }

            GD.Print(String.Format("[SFXPlayer3D] Ready with {0} sound groups", SoundGroups.Count));
        }

        public void PlaySound(string soundGroupName, Vector3 location)
        {
            if (SoundGroups.TryGetValue(soundGroupName, out SoundGroup3D soundGroup))
            {
                AudioStreamPlayer3D source = soundGroup.GetAvailableSource();

                if (source != null)
                {
                    GD.Print("[SFXPlayer3D] Playing: " + soundGroupName);
                    source.Position = location;
                    source.Play();
                    //debugSphere.Position = location;
                    //GD.Print("[SFXPlayer3D] location: ", location);
                }
            }
            else
            {
                GD.Print("[SFXPlayer3D] Requested a sound group that does not exist: " + soundGroupName);
            }
        }

    }

}

