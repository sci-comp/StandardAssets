using Godot;
using static Godot.BaseMaterial3D;

public partial class Signpost : Node3D, IInspectable
{
    public string Title { get; set; } = "Sign Post";
    public string Details { get; set; } = "This is a sign post.";

    public void Select()
    {
        //GD.Print("Sign post selected.");
    }

    public void Deselect()
    {
        //GD.Print("Sign post deselected.");
    }

    public void Inspect()
    {
        //GD.Print("Sign post inspected.");
    }

}

