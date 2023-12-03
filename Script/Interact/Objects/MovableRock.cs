using Godot;

public class MovableRock : IInteractable
{
    [Export] string name = "Rock";
    [Export] string details = "Push";

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
    }

}

