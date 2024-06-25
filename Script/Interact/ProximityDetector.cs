using Godot;

namespace Game
{
    public partial class ProximityDetector : Area3D
    {
        [Export] public string InteractButton = "interact";

        private IInspectable currentSelection;
        private Label labelTitle;
        private Label labelDetails;

        public bool SelectionExists => currentSelection != null;

        public override void _Ready()
        {
            labelTitle = GetNode<Label>("Title");
            labelDetails = GetNode<Label>("Details");

            if (labelTitle == null || labelDetails == null)
            {
                GD.PrintErr("A label reference is null in ProximityDetector");
            }

            AreaEntered += OnAreaEntered;
            AreaExited += OnAreaExited;
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;

            DisableUI();
        }

        private void OnAreaEntered(Area3D area)
        {
            if (area is IInspectable inspectable)
            {
                OnEntered(inspectable);
            }
        }

        private void OnAreaExited(Area3D area)
        {
            if (area is IInspectable inspectable)
            {
                OnExited(inspectable);
            }
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is IInspectable inspectable)
            {
                OnEntered(inspectable);
            }
        }

        private void OnBodyExited(Node3D body)
        {
            if (body is IInspectable inspectable)
            {
                OnExited(inspectable);
            }
        }

        private void OnEntered(IInspectable inspectable)
        {
            inspectable.Inspect();
            inspectable.Select();
            currentSelection = inspectable;

            EnableUI(inspectable.Title, inspectable.Details);
        }

        private void OnExited(IInspectable inspectable)
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
                if (@event.IsActionPressed(InteractButton))
                {
                    _interactable.Interact();
                }
            }
        }

    }

}

