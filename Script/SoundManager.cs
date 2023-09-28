using Godot;
using System.Collections.Generic;

public partial class SoundManager : Singleton<SoundManager>
{
    [Export] public Godot.Collections.Array<SoundBusInfo> SoundBusInfoArray;
    [Export] public Godot.Collections.Array<SoundGroup> SoundGroupsArraySFX;
    [Export] public Godot.Collections.Array<SoundGroup> SoundGroupsArrayUI;
    [Export] public Godot.Collections.Array<SoundGroup> SoundGroupsArrayVoice;

    private readonly Dictionary<string, SoundGroup> SoundGroups = new();
    private readonly Dictionary<SoundBUS, SoundBusInfo> AllBusInfo = new();

    public override void _Ready()
    {
        foreach (var info in SoundBusInfoArray)
        {
            AllBusInfo[info.SoundBus] = info;
        }

        List<SoundGroup> allSoundGroups = new();
        allSoundGroups.AddRange(SoundGroupsArraySFX);
        allSoundGroups.AddRange(SoundGroupsArrayUI);
        allSoundGroups.AddRange(SoundGroupsArrayVoice);

        foreach (var soundGroup in allSoundGroups)
        {
            SoundGroups[soundGroup.Name] = soundGroup;
        }
    }

    public void HandleAudioSourceStopped(SoundGroup soundGroup, AudioStreamPlayer3D src)
    {
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

            if (busInfo.ActiveSources.Count >= busInfo.VoiceLimit && busInfo.ActiveSources.Count > 0)
            {
                AudioStreamPlayer3D activeSource = busInfo.ActiveSources[0];
                busInfo.ActiveSourcesSoundGroup[0].Stop(activeSource);
            }

            (AudioStreamPlayer3D source, SoundGroup sourceSoundGroup) = soundGroup.GetAvailableSource();

            if (source != null)
            {
                busInfo.ActiveSources.Add(source);
                busInfo.ActiveSourcesSoundGroup.Add(sourceSoundGroup);

                source.Position = location;
                source.Play();
            }
        }
    }

}
