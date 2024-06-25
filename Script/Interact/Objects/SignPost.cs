using Godot;

namespace Game
{
    public partial class Signpost : Node3D, IInspectable
    {
        [Export] public string _Title = "Signpost";
        [Export] public string _Details = "";

        public string Title => _Title;
        public string Details => _Details;

        public virtual void Inspect() { }

        public virtual void Select() { }

        public virtual void Deselect() { }

    }

}

