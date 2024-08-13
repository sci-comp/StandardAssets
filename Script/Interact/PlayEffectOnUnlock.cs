using Godot;

namespace Game
{
    public partial class PlayEffectOnUnlock : Node3D, IFlagListener
    {
        [Export] public bool playParticles1 = false;
        [Export] public bool playParticles2 = false;
        [Export] public bool playSound1 = false;
        [Export] public bool playSound2 = false;
        [Export] public bool waitToPlay = false;
        [Export] public float waitDuration = 0.0f;
        [Export] public string SoundGroup1 = "Locked";
        [Export] public string SoundGroup2 = "Locked";
        [Export] public GpuParticles3D particles1;
        [Export] public GpuParticles3D particles2;

        private SFXPlayer3D sfxPlayer3D;

        public void Initialize(IFlag flag)
        {
            flag.OnFlagRaised += OnFlagRaised;
            sfxPlayer3D = GetNode<SFXPlayer3D>("/root/SFXPlayer3D");
        }

        private async void OnFlagRaised()
        {
            if (waitToPlay)
            {
                await ToSignal(GetTree().CreateTimer(waitDuration), "timeout");
            }

            if (playSound1)
            {
                sfxPlayer3D.PlaySound(SoundGroup1, GlobalPosition);
            }

            if (playSound2)
            {
                sfxPlayer3D.PlaySound(SoundGroup2, GlobalPosition);
            }

            if (playParticles1 && particles1 != null)
            {
                particles1.Restart();
                particles1.Emitting = true;
            }

            if (playParticles2 && particles2 != null)
            {
                particles2.Restart();
                particles2.Emitting = true;
            }
        }

    }

}

