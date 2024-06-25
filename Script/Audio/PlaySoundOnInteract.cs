using Godot;
using System;

namespace Game
{
    public partial class PlaySoundOnInteract : Node3D, IInteractable
    {
        [Export] public string SoundName = "DoorOpen";
        [Export] public string _Title = "Object";
        [Export] public string _Details = "Unidentified";

        private SFXPlayer3D player;

        public string Title => _Title;
        public string Details => _Details;

        public event Action Interacted;

        public override void _Ready()
        {
            player = GetNode<SFXPlayer3D>("/root/SFXPlayer3D");
        }

        public void Interact()
        {
            player.PlaySound(SoundName, Position);
            Interacted?.Invoke();
        }

        public void Inspect() { }

        public void Select() { }

        public void Deselect() { }

    }

}

