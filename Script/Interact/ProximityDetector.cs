using Godot;

namespace Game
{
    public partial class ProximityDetector : Area3D
    {
        [Export] public string InteractButton = "interact";
        [Export] public string PlayerID = "Player1";

        private Inspectable currentlySelectedStaticBody;
        private InspectableArea currentlySelectedArea;
        private Label labelTitle;
        private Label labelDetails;

        public bool SelectionExists => (currentlySelectedStaticBody != null) || (currentlySelectedArea != null);

        public void Initialize()
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

        public override void _UnhandledInput(InputEvent inputEvent)
        {
            if (inputEvent.IsActionPressed(InteractButton))
            {
                GetViewport().SetInputAsHandled();

                if (currentlySelectedStaticBody is Interactable interactable)
                {
                    interactable.Interact(PlayerID);
                }
                else if (currentlySelectedArea is InteractableArea interactableArea)
                {
                    interactableArea.Interact(PlayerID);
                }
            }
        }

        private void DisableUI()
        {
            labelTitle.Text = "";
            labelDetails.Text = "";
            labelTitle.Visible = false;
            labelDetails.Visible = false;
        }

        private void EnableUI(string _name, string _details)
        {
            labelTitle.Text = _name;
            labelDetails.Text = _details;
            labelTitle.Visible = true;
            labelDetails.Visible = true;
        }

        private void SelectNext()
        {
            var bodies = GetOverlappingBodies();
            foreach (var nextBody in bodies)
            {
                if (nextBody is Inspectable nextInspectable)
                {
                    nextInspectable.Select();
                    EnableUI(nextInspectable.Title, nextInspectable.Details);
                    return;
                }
            }

            var areas = GetOverlappingAreas();
            foreach (var nextArea in areas)
            {
                if (nextArea is InspectableArea nextInspectable)
                {
                    nextInspectable.Select();
                    EnableUI(nextInspectable.Title, nextInspectable.Details);
                    return;
                }
            }
        }

        // Events

        private void OnAreaEntered(Area3D area)
        {
            if (area is InspectableArea inspectableArea)
            {
                inspectableArea.Inspect();
                inspectableArea.Select();
                currentlySelectedArea = inspectableArea;
                EnableUI(inspectableArea.Title, inspectableArea.Details);
            }
        }

        private void OnAreaExited(Area3D area)
        {
            if (area is InspectableArea inspectable)
            {
                if (inspectable == currentlySelectedArea)
                {
                    inspectable.Deselect();
                    currentlySelectedArea = null;
                    DisableUI();
                    SelectNext();
                }
            }
        }

        private void OnBodyEntered(Node3D body)
        {
            if (body is Inspectable inspectable)
            {
                inspectable.Inspect();
                inspectable.Select();
                currentlySelectedStaticBody = inspectable;
                EnableUI(inspectable.Title, inspectable.Details);
            }
        }

        private void OnBodyExited(Node3D body)
        {
            if (body is Inspectable inspectable)
            {
                if (inspectable == currentlySelectedStaticBody)
                {
                    inspectable.Deselect();
                    currentlySelectedArea = null;
                    DisableUI();
                    SelectNext();
                }
            }
        }

    }

}

