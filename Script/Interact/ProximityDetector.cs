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
        private ProgressBar progressBar;
        private Timer interactionTimer;
        
        private bool actionInProgress = false;
        private bool idle = false;
        private ITimedInteractable currentTimedInteractable;

        private bool InformationIsDisplayed = false;
        private float t_checkEvery = 1.0f;
        private float t_current = 0.0f;

        public bool SelectionExists => (currentlySelectedStaticBody != null) || (currentlySelectedArea != null);

        public void Initialize()
        {
            labelTitle = GetNode<Label>("Title");
            labelDetails = GetNode<Label>("Details");
            progressBar = GetNode<ProgressBar>("ProgressBar");

            if (labelTitle == null || labelDetails == null || progressBar == null)
            {
                GD.PrintErr("A label reference is null in ProximityDetector");
            }

            interactionTimer = new Timer();
            AddChild(interactionTimer);
            interactionTimer.OneShot = true;
            interactionTimer.Timeout += OnInteractionTimerTimeout;

            AreaEntered += OnAreaEntered;
            AreaExited += OnAreaExited;
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;

            DisableUI();
        }

        public override void _ExitTree()
        {
            if (interactionTimer != null)
            {
                interactionTimer.Timeout -= OnInteractionTimerTimeout;
                interactionTimer.QueueFree();
            }

            AreaEntered -= OnAreaEntered;
            AreaExited -= OnAreaExited;
            BodyEntered -= OnBodyEntered;
            BodyExited -= OnBodyExited;
        }

        public override void _UnhandledInput(InputEvent inputEvent)
        {
            if (inputEvent.IsActionPressed(InteractButton))
            {
                GetViewport().SetInputAsHandled();

                if (currentlySelectedArea is ITimedInteractable timedInteractable && !actionInProgress)
                {
                    StartTimedInteraction(timedInteractable);
                }
                else if (currentlySelectedStaticBody is Interactable interactable)
                {
                    interactable.Interact(PlayerID);
                }
                else if (currentlySelectedArea is InteractableArea interactableArea)
                {
                    interactableArea.Interact(PlayerID);
                }
            }
        }

        public override void _Process(double delta)
        {
            if (t_current > t_checkEvery)
            {
                t_current = 0.0f;

                if (!SelectionExists && InformationIsDisplayed)
                {
                    DisableUI();
                    SelectNext();
                    GD.Print("[ProximityDetector] Selected area was freed, selected next");
                }
            }

            t_current += (float)delta;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (actionInProgress && interactionTimer.TimeLeft > 0)
            {
                progressBar.Value = interactionTimer.WaitTime - interactionTimer.TimeLeft;
            }
        }

        public void StartTimedInteraction(ITimedInteractable interactable)
        {
            if (interactable.HasInteractionDuration)
            {
                if (idle)
                {
                    actionInProgress = true;
                    currentTimedInteractable = interactable;
                    progressBar.Visible = true;
                    progressBar.MaxValue = interactable.InteractionDuration;
                    progressBar.Value = 0;
                    interactionTimer.WaitTime = interactable.InteractionDuration;
                    interactionTimer.Start();
                }
            }
            else
            {
                interactable.CompleteInteraction(PlayerID);
            }
        }

        private void OnInteractionTimerTimeout()
        {
            if (currentTimedInteractable != null)
            {
                currentTimedInteractable.CompleteInteraction(PlayerID);
                progressBar.Visible = false;
                actionInProgress = false;
                currentTimedInteractable = null;
            }
            else
            {
                GD.Print("[ProximityDetector] currentTimedInteractable is already null?");
            }
        }

        public void InterruptInteraction()
        {
            if (actionInProgress)
            {
                interactionTimer.Stop();
                progressBar.Value = 0;
                progressBar.Visible = false;
                actionInProgress = false;
                currentTimedInteractable = null;
            }
        }

        private void DisableUI()
        {
            labelTitle.Text = "";
            labelDetails.Text = "";
            labelTitle.Visible = false;
            labelDetails.Visible = false;
            progressBar.Visible = false;
            InformationIsDisplayed = false;
        }

        private void EnableUI(string _name, string _details)
        {
            labelTitle.Text = _name;
            labelDetails.Text = _details;
            labelTitle.Visible = true;
            labelDetails.Visible = true;
            InformationIsDisplayed = true;
            t_current = 0.0f;  // Let the information linger for instant pickup items
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
                    currentlySelectedStaticBody = null;
                    DisableUI();
                    SelectNext();
                }
            }
        }

        // This method is called by CharacterController every frame
        public void SetIdle(bool flag)
        {
            if (idle != flag)
            {
                idle = flag;
            }

            if (!idle && actionInProgress)
            {
                GD.Print("[ProximityDetector] Interrupting interaction...");
                InterruptInteraction();
            }
        }
    }

}

