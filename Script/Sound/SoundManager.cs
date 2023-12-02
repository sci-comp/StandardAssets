using Godot;
using System.Collections.Generic;

public enum SoundBUS
{
    Ambient,
    Environment,
    SFX,
    UI,
    Voice
}

public  class SoundBusInfo
{
    public SoundBUS SoundBus;
    public float Volume = 1.0f;
    public int VoiceLimit;
    public List<AudioStreamPlayer3D> ActiveSources = new();
    public List<SoundGroup> ActiveSourcesSoundGroup = new();

    public SoundBusInfo(SoundBUS _soundBus, int _voiceLimit = 1, float _volume = 1.0f)
    {
        Volume = _volume;
        VoiceLimit = _voiceLimit;
        ActiveSources = new();
        ActiveSourcesSoundGroup = new();
    }
}

public partial class SoundManager : Singleton<SoundManager>
{
    [Export] public Godot.Collections.Array<SoundGroup> SoundGroupsArraySFX;
    [Export] public Godot.Collections.Array<SoundGroup> SoundGroupsArrayUI;
    [Export] public Godot.Collections.Array<SoundGroup> SoundGroupsArrayVoice;

    public readonly Dictionary<SoundBUS, SoundBusInfo> AllBusInfo = new();

    private readonly Dictionary<string, SoundGroup> SoundGroups = new();

    public override void _Ready()
    {

        AllBusInfo[SoundBUS.SFX] = new SoundBusInfo(SoundBUS.SFX, 12);
        AllBusInfo[SoundBUS.UI] = new SoundBusInfo(SoundBUS.UI, 3);
        AllBusInfo[SoundBUS.Voice] = new SoundBusInfo(SoundBUS.Voice, 4);

        List<SoundGroup> allSoundGroups = new();
        allSoundGroups.AddRange(SoundGroupsArraySFX);
        allSoundGroups.AddRange(SoundGroupsArrayUI);
        allSoundGroups.AddRange(SoundGroupsArrayVoice);

        foreach (var soundGroup in allSoundGroups)
        {
            GD.Print("adding soundGroup, soundGroup.SoundBus: " + soundGroup.SoundBus);

            SoundGroups[soundGroup.Name] = soundGroup;
        }

        GD.Print("Total number of sound groups: " + SoundGroups.Count);
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

            GD.Print("Playing a new sound for bus: " + busInfo.SoundBus);
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
