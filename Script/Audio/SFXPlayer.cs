using Godot;
using System.Collections.Generic;

public partial class SFXPlayer : Node
{
    private AudioStreamPlayer uiBack;
    private AudioStreamPlayer uiError;
    private AudioStreamPlayer uiLight;
    private AudioStreamPlayer uiNegative;
    private AudioStreamPlayer uiPositive;
    private SFXPlayerDisplay display;

    public Dictionary<string, AudioStreamPlayer> Players;

    public override void _Ready()
    {
        uiBack = GetNode<AudioStreamPlayer>("UIBack");
        uiError = GetNode<AudioStreamPlayer>("UIError");
        uiLight = GetNode<AudioStreamPlayer>("UILight");
        uiNegative = GetNode<AudioStreamPlayer>("UINegative");
        uiPositive = GetNode<AudioStreamPlayer>("UIPositive");
        display = GetNode<SFXPlayerDisplay>("Display");

        Players = new()
        {
            { "ui_back", uiBack },
            { "ui_error", uiError },
            { "ui_light", uiLight },
            { "ui_negative", uiNegative },
            { "ui_positive", uiPositive }
        };

        display.Initialize(this);
    }

    public void PlaySound(string soundGroupName)
    {
        if (Players.TryGetValue(soundGroupName, out AudioStreamPlayer player))
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

