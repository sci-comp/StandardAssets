using Godot;

public partial class ProximityDetector : Area3D
{
    private IInspectable currentSelection;

    private Label labelTitle;
    private Label labelDetails;

    public override void _Ready()
    {
        labelTitle = GetNode<Label>("../../HUD/Title");
        labelDetails = GetNode<Label>("../../HUD/Details");

        DisableUI();

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node3D body)
    {
        GD.Print("We've bumped into: " + body.Name);

        if (body is IInspectable inspectable)
        {
            inspectable.Inspect();
            inspectable.Select();
            currentSelection = inspectable;

            GD.Print("Selecting: " + inspectable.Title);

            EnableUI(inspectable.Title, inspectable.Details);
        }
        else
        {
            GD.Print("An unfiltered object entered the proximity detection area.");
        }
    }

    private void OnBodyExited(Node3D body)
    {
        GD.Print("We've exited area: " + body.Name);

        if (body is IInspectable inspectable)
        {
            if (inspectable == currentSelection)
            {
                inspectable.Deselect();
                currentSelection = null;

                DisableUI();
                    
                var bodies = GetOverlappingBodies();
                foreach (var next_body in bodies)
                {
                    if (next_body is IInspectable next_inspectable)
                    {
                        next_inspectable.Select();

                        EnableUI(inspectable.Title, inspectable.Details);
                    }
                }
            }
        }
        else
        {
            GD.Print("An unfiltered object exited the proximity detection area.");
        }
    }

    private void EnableUI(string _name, string _details)
    {
        labelTitle.Text = _name;
        labelDetails.Text = _details;
        labelTitle.Visible = true;
        labelDetails.Visible = true;
    }

    private void DisableUI()
    {
        labelTitle.Text = "";
        labelDetails.Text = "";
        labelTitle.Visible = false;
        labelDetails.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (currentSelection is IInteractable _interactable) 
        {
            if (@event.IsActionPressed("jump"))
            {
                _interactable.Interact();
            }
        }
    }

}

