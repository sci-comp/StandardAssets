using Godot;
using System.Collections.Generic;

public partial class SFXPlayer : Node
{
    public Dictionary<string, AudioStreamPlayer> SoundGroups = new();

    public override void _Ready()
    {
        foreach (Node child in GetChildren())
        {
            if (child is AudioStreamPlayer)
            {
                SoundGroups[child.Name] = child as AudioStreamPlayer;
            }
        }

        GD.Print(string.Format("Sfx player ready with {0} sound groups", SoundGroups.Count));
    }

    public void PlaySound(string soundGroupName)
    {
        if (SoundGroups.TryGetValue(soundGroupName, out AudioStreamPlayer player))
        {
            if (player != null)
            {
                GD.Print("Playing: " + soundGroupName);
                player.Play();
            }
        }
        else
        {
            GD.Print("Requested a sound group that does not exist: " + soundGroupName);
        }
    }

}

