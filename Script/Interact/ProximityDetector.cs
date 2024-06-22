using Godot;

public partial class ProximityDetector : Area3D
{
    [Export] public string InteractButton = "a";

    private Inspectable currentSelection;
    private Label labelTitle;
    private Label labelDetails;

    public bool SelectionExists => currentSelection != null;

    public override void _Ready()
    {
        labelTitle = GetNode<Label>("Title");
        labelDetails = GetNode<Label>("Details");

        if (labelTitle == null ||  labelDetails == null)
        {
            GD.PrintErr("A label reference is null in ProximityDetector");
        }

        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        DisableUI();
    }

    private void OnAreaEntered(Node3D body)
    {
        OnBodyEntered(body);
    }

    private void OnAreaExited(Node3D body)
    {
        OnBodyExited(body);
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Inspectable inspectable)
        {
            inspectable.Inspect();
            inspectable.Select();
            currentSelection = inspectable;

            EnableUI(inspectable.Title, inspectable.Details);
        }
        else
        {
            GD.Print("An unfiltered object entered the proximity detection area: " + body.Name);
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is Inspectable inspectable)
        {
            if (inspectable == currentSelection)
            {
                inspectable.Deselect();
                currentSelection = null;

                DisableUI();
                    
                var bodies = GetOverlappingBodies();
                foreach (var next_body in bodies)
                {
                    if (next_body is Inspectable next_inspectable)
                    {
                        next_inspectable.Select();

                        EnableUI(inspectable.Title, inspectable.Details);
                    }
                }
            }
        }
        else
        {
            GD.Print("An unfiltered object exited the proximity detection area: " + body.Name);
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
        if (currentSelection is Interactable _interactable) 
        {
            if (@event.IsActionPressed(InteractButton))
            {
                _interactable.Interact();
            }
        }
    }

}

