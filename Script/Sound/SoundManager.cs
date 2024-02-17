using Godot;
using System.Collections.Generic;

public partial class SoundManager : Singleton<SoundManager>
{
    private Game game;

    public float MasterVolume { get; set; } = 0.7f;

    private Godot.Collections.Array<SoundGroup> SoundGroupsSFX;
    private Godot.Collections.Array<SoundGroup> SoundGroupsUI;
    private Godot.Collections.Array<SoundGroup> SoundGroupsVoice;
    private PossibleSounds possibleSounds;

    public readonly Dictionary<SoundBUS, SoundBusInfo> AllBusInfo = new();
    public Dictionary<string, SoundGroup> SoundGroups = new();

    public override void _Ready()
    {
        game = GetNode<Game>("..");

        possibleSounds = GetNode<PossibleSounds>("Display/PossibleSounds");

        AllBusInfo[SoundBUS.Ambient] = new SoundBusInfo(8, game.Preferences.AmbientVolume);
        AllBusInfo[SoundBUS.Environment] = new SoundBusInfo(2, game.Preferences.EnvironmentVolume);
        AllBusInfo[SoundBUS.Music] = new SoundBusInfo(12, game.Preferences.MusicVolume);
        AllBusInfo[SoundBUS.SFX] = new SoundBusInfo(12, game.Preferences.SFXVolume);
        AllBusInfo[SoundBUS.UI] = new SoundBusInfo(3, game.Preferences.UIVolume);
        AllBusInfo[SoundBUS.Voice] = new SoundBusInfo(4, game.Preferences.VoiceVolume);

        SoundGroups = possibleSounds.GetSoundGroups();
    }

    public void HandleAudioSourceStopped(SoundGroup soundGroup, AudioStreamPlayer3D src)
    {
        GD.Print("TODO: redraw relevant display labels");

        // if (display is open)
        // {
        // redraw: relevant bus group
        // redraw: relevant sound group
        // }

        SoundBusInfo busInfo = AllBusInfo[soundGroup.SoundBus];

        if (busInfo.ActiveSources.Count > 0)
        {
            busInfo.ActiveSources.Remove(src);
            busInfo.ActiveSourcesSoundGroup.Remove(soundGroup);
        }
    }

    public void PlaySound(string soundGroupName, Vector3 location)
    {
        if (SoundGroups.TryGetValue(soundGroupName, out SoundGroup soundGroup))
        {
            SoundBusInfo busInfo = AllBusInfo[soundGroup.SoundBus];

            //GD.Print("Playing a new sound for bus: " + busInfo);
            GD.Print("Active Sources / Voice Limit: " + busInfo.ActiveSources.Count + " / " + busInfo.VoiceLimit);

            if (busInfo.ActiveSources.Count >= busInfo.VoiceLimit && busInfo.ActiveSources.Count > 0)
            {
                GD.Print("Exceeding bus voice limit, stopping a source");
                AudioStreamPlayer3D activeSource = busInfo.ActiveSources[0];
                busInfo.ActiveSourcesSoundGroup[0].Stop(activeSource);
            }

            (AudioStreamPlayer3D source, SoundGroup sourceSoundGroup) = soundGroup.GetAvailableSource();

            if (source != null)
            {
                GD.Print("Playing...");
                busInfo.ActiveSources.Add(source);
                busInfo.ActiveSourcesSoundGroup.Add(sourceSoundGroup);

                source.Position = location;
                source.Play();
            }
        }
    }

}

