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

public partial class SoundBusInfo : Resource
{
    [Export] public SoundBUS SoundBus;
    [Export] public float Volume = 1.0f;
    [Export] public int VoiceLimit;

    public List<AudioStreamPlayer3D> ActiveSources = new();
    public List<SoundGroup> ActiveSourcesSoundGroup = new();
}
