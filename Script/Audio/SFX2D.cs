using Godot;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// Autoload singleton accessible at /root/SFX2D
    /// </summary>
    public partial class SFX2D : Node
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

            GD.PrintRich($"[SFX2D] [color={ColorsHex.MediumSeaGreen}]Ready[/color] with {SoundGroups.Count} sound groups");
        }

        public void PlaySound(string soundGroupName)
        {
            if (SoundGroups.TryGetValue(soundGroupName, out AudioStreamPlayer player))
            {
                if (player != null)
                {
                    GD.Print("[SFX2D] Playing: " + soundGroupName);
                    player.Play();
                }
            }
            else
            {
                GD.Print("[SFX2D] Requested a sound group that does not exist: " + soundGroupName);
            }
        }

    }

}

