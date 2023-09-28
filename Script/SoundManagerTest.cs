using Godot;
using System;

public partial class SoundManagerTest : Node
{
    [Export] private NodePath SoundManagerPath;
    private SoundManager soundManager;

    private Label SFXBusText;

    public override void _Ready()
    {
        soundManager = GetNode<SoundManager>(SoundManagerPath);
        SFXBusText = GetNode<Label>("YourPathToLabel");

        Button btnBubbles = GetNode<Button>("YourPathToBubblesButton");
        Button btnVelcro = GetNode<Button>("YourPathToVelcroButton");

        btnBubbles.Connect("pressed", new Callable(this, nameof(PlayBubbles)));
        btnVelcro.Connect("pressed", new Callable(this, nameof(PlayVelcro)));
    }

    public void _Process(float delta)
    {
        SoundBusInfo sfxBusInfo = soundManager.SoundBusInfoArray[0];
        textComponent.Text = $"Bus: SFX\n" +
                             $"Active Voices: {sfxBusInfo.ActiveSources.Count}/{sfxBusInfo.VoiceLimit}\n" +
                             $"Volume: {sfxBusInfo.Volume}";
    }

    private void PlayBubbles()
    {
        soundManager.PlaySound("sfx_bubbles", new Vector3(0,0,0));
    }

    private void PlayVelcro()
    {
        soundManager.PlaySound("sfx_velcro", new Vector3(0, 0, 0));
    }

}
