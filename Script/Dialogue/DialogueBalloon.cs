using Game;
using Godot;
using Godot.Collections;
using System;

namespace DialogueManagerRuntime
{
    public partial class DialogueBalloon : CanvasLayer
    {
        [Export] public string NextAction = "x";
        [Export] public string SkipAction = "b";

        private bool willHideBalloon = false;
        private Array<Variant> temporaryGameStates = [];
        private Control balloon;
        private Resource resource;
        private RichTextLabel characterLabel;
        private RichTextLabel dialogueLabel;
        private TextureRect indicator;
        private VBoxContainer responsesMenu;
        private SFX sfxPlater3D;
        private bool isWaitingForInput = false;
        private bool IsWaitingForInput
        {
            get
            {
                return isWaitingForInput;
            }
            set
            {
                indicator.Visible = value;
                isWaitingForInput = value;
            }
        }

        DialogueLine dialogueLine;
        DialogueLine DialogueLine
        {
            get => dialogueLine;
            set
            {
                IsWaitingForInput = false;
                balloon.FocusMode = Control.FocusModeEnum.All;
                balloon.GrabFocus();

                if (value == null)
                {
                    QueueFree();
                    return;
                }

                dialogueLine = value;
                UpdateDialogue();
            }
        }

        public static event Action<string, string> ActorSFXRequested;
        public static event Action<string, string> ActorGestureRequested;
        public static event Action<Resource> DialogueStarted;

        public override void _UnhandledInput(InputEvent @event)
        {
            // Only the balloon is allowed to handle input while it's showing
            GetViewport().SetInputAsHandled();
        }

        public override void _ExitTree()
        {
            DialogueManager.Mutated -= OnMutated;
        }

        public async void Start(Resource dialogueResource, string title, Array<Variant> extraGameStates = null)
        {
            GD.Print("[DialogueBalloon] Starting...");

            balloon = GetNode<Control>("%Balloon");
            characterLabel = GetNode<RichTextLabel>("%CharacterLabel");
            dialogueLabel = GetNode<RichTextLabel>("%DialogueLabel");
            responsesMenu = GetNode<VBoxContainer>("%ResponsesMenu");
            indicator = GetNode<TextureRect>("%Indicator");

            balloon.Hide();
            indicator.Hide();

            balloon.GuiInput += (@event) =>
            {
                if ((bool)dialogueLabel.Get("is_typing"))
                {
                    bool mouseWasClicked = @event is InputEventMouseButton && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left && @event.IsPressed();
                    bool skipButtonWasPressed = @event.IsActionPressed(SkipAction);

                    if (mouseWasClicked || skipButtonWasPressed)
                    {
                        GetViewport().SetInputAsHandled();
                        dialogueLabel.Call("skip_typing");
                        return;
                    }
                }

                if (!IsWaitingForInput)
                {
                    return;
                }

                if (dialogueLine.Responses.Count > 0)
                {
                    return;
                }

                GetViewport().SetInputAsHandled();

                if (@event is InputEventMouseButton && @event.IsPressed() && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left)
                {
                    Next(dialogueLine.NextId);
                }
                else if (@event.IsActionPressed(NextAction) && GetViewport().GuiGetFocusOwner() == balloon)
                {
                    Next(dialogueLine.NextId);
                }
            };

            if (string.IsNullOrEmpty((string)responsesMenu.Get("next_action")))
            {
                responsesMenu.Set("next_action", NextAction);
            }

            responsesMenu.Connect("response_selected", Callable.From((DialogueResponse response) => { Next(response.NextId); }));

            DialogueManager.Mutated += OnMutated;

            GD.Print("[DialogueBalloon] Ready");

            temporaryGameStates = extraGameStates ?? [];
            IsWaitingForInput = false;
            resource = dialogueResource;
            DialogueLine = await DialogueManager.GetNextDialogueLine(resource, title, temporaryGameStates);
            DialogueStarted?.Invoke(resource);

            GD.Print("[DialogueBalloon] Started");
        }

        public async void Next(string nextId)
        {
            DialogueLine = await DialogueManager.GetNextDialogueLine(resource, nextId, temporaryGameStates);
        }

        private async void UpdateDialogue()
        {
            if (!IsNodeReady())
            {
                await ToSignal(this, SignalName.Ready);
            }

            // Process tags
            foreach (string tag in dialogueLine.Tags)
            {
                GD.Print("[DialogueBalloon] Requesting a gesture, dialogueLine.Character, tag: ", dialogueLine.Character, tag);
                ActorGestureRequested?.Invoke(ActorNameToID(dialogueLine.Character), tag);
            }

            // Set up the character name
            characterLabel.Visible = !string.IsNullOrEmpty(dialogueLine.Character);
            characterLabel.Text = Tr(dialogueLine.Character, "dialogue");

            // Set up the dialogue
            dialogueLabel.Hide();
            dialogueLabel.Set("dialogue_line", dialogueLine);

            // Set up the responses
            responsesMenu.Hide();
            responsesMenu.Set("responses", dialogueLine.Responses);

            // Type out the text
            balloon.Show();
            willHideBalloon = false;
            dialogueLabel.Show();
            if (!string.IsNullOrEmpty(dialogueLine.Text))
            {
                dialogueLabel.Call("type_out");
                await ToSignal(dialogueLabel, "finished_typing");
            }

            // Wait for input
            if (dialogueLine.Responses.Count > 0)
            {
                balloon.FocusMode = Control.FocusModeEnum.None;
                responsesMenu.Show();
            }
            else if (!string.IsNullOrEmpty(dialogueLine.Time))
            {
                if (!float.TryParse(dialogueLine.Time, out float time))
                {
                    time = dialogueLine.Text.Length * 0.02f;
                }
                await ToSignal(GetTree().CreateTimer(time), "timeout");
                Next(dialogueLine.NextId);
            }
            else
            {
                IsWaitingForInput = true;
                balloon.FocusMode = Control.FocusModeEnum.All;
                balloon.GrabFocus();
            }

            GD.Print("[DialogueBalloon] Updated dialogue");
        }

        private void OnMutated(Dictionary _mutation)
        {
            IsWaitingForInput = false;
            willHideBalloon = true;
            GetTree().CreateTimer(0.1f).Timeout += () =>
            {
                if (willHideBalloon)
                {
                    willHideBalloon = false;
                    balloon.Hide();
                }
            };
            GD.Print("[DialogueBalloon] Mutated");
        }

        private string ActorNameToID(string actorName)
        {
            // This could be expanded to include all actors in the game, though,
            // maybe there is a better options for implementation
            switch (actorName)
            {
                case "Skeleton Guard": return "Skeleton_Guard";
            }
            return "actorName";
        }
    }

}

