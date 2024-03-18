using Godot;
using System;
using System.Collections.Generic;

public partial class SFXPlayer3D : Node
{
    private PossibleSFX3D possibleSounds;
    private Preferences prefs;
    private SFXPlayer3DDisplay audioDisplay;

    public float MasterVolume { get; set; } = 0.7f;
    public Dictionary<string, SoundGroup3D> SoundGroups = new();

    public override void _Ready()
    {
        audioDisplay = GetNode<SFXPlayer3DDisplay>("Display");
        prefs = GetNode<Preferences>("/root/Preferences");
        possibleSounds = GetNode<PossibleSFX3D>("Display/SoundGroups/PossibleSounds");

        if (audioDisplay == null || prefs == null || possibleSounds == null)
        {
            GD.PrintErr("Null reference to a child node in SFXPlayer3D");
        }
        else
        {
            possibleSounds.Initialize(this);
            SoundGroups = possibleSounds.GetSoundGroups();
            audioDisplay.Initialize(this);
        }

        GD.Print(String.Format("3D sfx player ready with {0} sound groups", SoundGroups.Count));
    }

    public void PlaySound(string soundGroupName, Vector3 location)
    {
        if (SoundGroups.TryGetValue(soundGroupName, out SoundGroup3D soundGroup))
        {

            (AudioStreamPlayer3D source, SoundGroup3D sourceSoundGroup) = soundGroup.GetAvailableSource();

            if (source != null)
            {
                GD.Print("Playing: " + soundGroupName);
                source.Position = location;
                source.Play();
            }
        }
        else
        {
            GD.Print("Requested a sound group that does not exist: " + soundGroupName);
        }
    }

    public void UpdateSoundGroupDisplay(SoundGroup3D _soundGroup)
    {
        if (audioDisplay.Visible)
        {
            audioDisplay.UpdateSoundGroup(_soundGroup);
        }
    }

}

