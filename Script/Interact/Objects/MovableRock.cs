using Godot;

public partial class MovableRock : RigidBody3D, IInteractable
{
    [Export] string name = "Rock";
    [Export] string details = "Push";

    private bool alreadyPushed = false;

    public string Name => name;
    public string Details => details;

    public void Select()
    {
        GD.Print("Movable rock selected.");
    }

    public void Deselect()
    {
        GD.Print("Movable rock deselected.");
    }

    public void Inspect()
    {
        GD.Print("Movable rock inspected.");
    }

    public void Interact()
    {
        GD.Print("Movable rock interacted with.");
        if (!alreadyPushed)
        {
            Freeze = false;
            alreadyPushed = true;
        }
        
    }

}

