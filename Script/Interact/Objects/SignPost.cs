using Godot;

public partial class SignPost : Node3D, IInspectable
{
    [Export] string title = "Sign Post";
    [Export] string details = "This is a sign post.";

    public string Title => title;
    public string Details => details;

    public void Select()
    {
        GD.Print("Sign post selected.");
    }

    public void Deselect()
    {
        GD.Print("Sign post deselected.");
    }

    public void Inspect()
    {
        GD.Print("Sign post inspected.");
    }

}

