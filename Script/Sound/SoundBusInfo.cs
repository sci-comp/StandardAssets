using Godot;
using System.Collections.Generic;

public enum SoundBUS
{
    Ambient,
    Environment,
    Music,
    SFX,
    UI,
    Voice
}

public class SoundBusInfo
{
    public float Volume = 1.0f;
    public int VoiceLimit;
    public List<AudioStreamPlayer3D> ActiveSources = new();
    public List<SoundGroup> ActiveSourcesSoundGroup = new();

    public SoundBusInfo(int _voiceLimit = 1, float _volume = 1.0f)
    {
        Volume = _volume;
        VoiceLimit = _voiceLimit;
        ActiveSources = new();
        ActiveSourcesSoundGroup = new();
    }
}

