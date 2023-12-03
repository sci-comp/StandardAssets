using Godot;

public partial class ProximityDetector : Area3D
{
    private IInspectable currentSelection;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is IInspectable inspectable)
        {
            inspectable.Inspect();
            inspectable.Select();
            currentSelection = inspectable;
            GD.Print("Selecting: " + inspectable.Name);
        }
        else
        {
            GD.Print("An unfiltered object entered the proximity detection area.");
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is IInspectable inspectable)
        {
            if (inspectable == currentSelection)
            {
                inspectable.Deselect();

                var bodies = GetOverlappingBodies();
                foreach (var next_body in bodies)
                {
                    if (next_body is IInspectable next_inspectable)
                    {
                        next_inspectable.Select();
                    }
                }
            }
        }
        else
        {
            GD.Print("An unfiltered object exited the proximity detection area.");
        }
    }

}

