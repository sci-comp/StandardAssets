using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SoundGroup : Node
{
    [Export] public SoundBUS SoundBus;
    [Export] public Vector2 VaryPitch = new(0.95f, 1.05f);
    [Export] public Vector2 VaryVolume = new(0.94f, 1.0f);

    private Queue<AudioStreamPlayer3D> AvailableSources = new();
    private List<AudioStreamPlayer3D> ActiveSources = new();

    public override void _Ready()
    {
        foreach (AudioStreamPlayer3D child in GetChildren().Cast<AudioStreamPlayer3D>())
        {
            child.Connect("finished", new Callable(this, nameof(OnAudioFinished)));
            AvailableSources.Enqueue(child);
        }
    }

    public void Stop(AudioStreamPlayer3D src)
    {
        src.Stop();
        ActiveSources.Remove(src);
        AvailableSources.Enqueue(src);
        SoundManager.Instance.HandleAudioSourceStopped(this, src);
    }

    public (AudioStreamPlayer3D, SoundGroup) GetAvailableSource()
    {
        AudioStreamPlayer3D src;

        if (AvailableSources.Count > 0)
        {
            src = AvailableSources.Dequeue();
            src.Playing = true;
            ActiveSources.Add(src);
        }
        else if (ActiveSources.Count > 0)
        {
            src = ActiveSources[0];
            src.Stop();
            SoundManager.Instance.HandleAudioSourceStopped(this, src);
            ActiveSources.RemoveAt(0);
            ActiveSources.Add(src);
        }
        else
        {
            GD.PrintErr("No active or available audio sources. This is not logical.");
            return (null, this);
        }

        src.PitchScale = (float)GD.RandRange(VaryPitch.X, VaryPitch.Y);
        src.VolumeDb = (float)GD.RandRange(VaryVolume.X, VaryVolume.Y);

        return (src, this);
    }

    private void OnAudioFinished(AudioStreamPlayer3D src)
    {
        src.Playing = false;
        ActiveSources.Remove(src);
        AvailableSources.Enqueue(src);
        SoundManager.Instance.HandleAudioSourceStopped(this, src);
    }

}

