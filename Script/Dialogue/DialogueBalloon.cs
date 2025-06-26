using Game;
using Godot;
using Godot.Collections;
using System;
using System.Threading.Tasks;

namespace DialogueManagerRuntime
{
    public partial class DialogueBalloon : CanvasLayer
    {
        [Export] public string NextAction = "a";
        [Export] public string SkipAction = "x";
        [Export] public AudioStreamPlayer audioStreamPlayer;
        [Export] public string voicePath = "res://Audio/Dialogue";
        [Export] public bool showPlayerLines = false;

        private string currentDialogueTitle = "default";

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

        private Control.GuiInputEventHandler inputHandler;

        public override void _UnhandledInput(InputEvent @event)
        {
            // Only the balloon is allowed to handle input while it's showing
            GetViewport().SetInputAsHandled();
        }

        public override void _ExitTree()
        {
            DialogueManager.Mutated -= OnMutated;

            if (balloon != null && inputHandler != null)
            {
                balloon.GuiInput -= inputHandler;
            }
        }

        private async void DisplayLineAndNext(DialogueResponse response)
        {
            if (showPlayerLines)
            {
                await DisplayLine("Player", response.Text, response.TranslationKey);
            }
            Next(response.NextId);
        }

        public async void Start(Resource dialogueResource, string title, Array<Variant> extraGameStates = null)
        {
            GD.Print("[DialogueBalloon] Starting, title: ", title);

            currentDialogueTitle = string.IsNullOrEmpty(title) ? "default" : title;

            balloon = GetNode<Control>("%Balloon");
            characterLabel = GetNode<RichTextLabel>("%CharacterLabel");
            dialogueLabel = GetNode<RichTextLabel>("%DialogueLabel");
            responsesMenu = GetNode<VBoxContainer>("%ResponsesMenu");
            indicator = GetNode<TextureRect>("%Indicator");

            balloon.Hide();
            indicator.Hide();

            inputHandler = (@event) =>
            {
                if ((bool)dialogueLabel.Get("is_typing"))
                {
                    bool mouseWasClicked = @event is InputEventMouseButton && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left && @event.IsPressed();
                    bool skipButtonWasPressed = @event.IsActionPressed(SkipAction);

                    if (mouseWasClicked || skipButtonWasPressed)
                    {
                        GetViewport().SetInputAsHandled();
                        dialogueLabel.Call("skip_typing");
                        if (audioStreamPlayer.Playing)
                        {
                            audioStreamPlayer.Stop();
                            audioStreamPlayer.EmitSignal(AudioStreamPlayer.SignalName.Finished);
                        }
                        return;
                    }
                }

                if (!IsWaitingForInput)
                {
                    return;
                }

                GetViewport().SetInputAsHandled();

                if (@event is InputEventMouseButton && @event.IsPressed()
                && (@event as InputEventMouseButton).ButtonIndex == MouseButton.Left)
                {
                    Next(dialogueLine.NextId);
                }
                else if (@event.IsActionPressed(NextAction) && GetViewport().GuiGetFocusOwner() == balloon)
                {
                    Next(dialogueLine.NextId);
                }
            };

            balloon.GuiInput += inputHandler;

            if (string.IsNullOrEmpty((string)responsesMenu.Get("next_action")))
            {
                responsesMenu.Set("next_action", NextAction);
            }

            responsesMenu.Connect("response_selected", Callable.From((DialogueResponse response) => {
                DisplayLineAndNext(response);
            }));

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

            GD.Print("dialogueLine.Type: ", dialogueLine.Type);

            foreach (string tag in dialogueLine.Tags)
            {
                GD.Print("[DialogueBalloon] Requesting a gesture, dialogueLine.Character, tag: ", dialogueLine.Character, tag);
            }

            responsesMenu.Hide();
            responsesMenu.Set("responses", dialogueLine.Responses);

            balloon.Show();
            willHideBalloon = false;
            dialogueLabel.Show();

            await DisplayLine(dialogueLine.Character, dialogueLine.Text, dialogueLine.TranslationKey);

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

        private void PlayVoice(string actorId, string translationId, bool isNarrator = false)
        {
            if (translationId == "")
            {
                GD.Print($"[DialogueBalloon] Empty translationId: {translationId}");
                return;
            }

            string locale = TranslationServer.GetLocale().Split('_')[0];
            string path;
            if (actorId == "")
            {
                if (isNarrator)
                {
                    path = $"{voicePath}/Narrator/{locale}/{currentDialogueTitle}/{translationId}.ogg";
                }
                else
                {
                    path = $"{voicePath}/Player/{locale}/{currentDialogueTitle}/{translationId}.ogg";
                }
            }
            else
            {
                path = $"{voicePath}/{locale}/{actorId}/{currentDialogueTitle}/{translationId}.ogg";
            }

            if (ResourceLoader.Exists(path))
            {
                var stream = GD.Load<AudioStream>(path);
                audioStreamPlayer.Stream = stream;
                audioStreamPlayer.Play();
            }
            else
            {
                GD.Print("[DialogueBalloon] Voice file path does not exist: ", path);
            }
        }

        private async Task DisplayLine(string character, string text, string translationKey)
        {
            characterLabel.Visible = !string.IsNullOrEmpty(character);
            characterLabel.Text = Tr(character, "dialogue");

            // Temporarily modify dialogueLine for the label
            string originalCharacter = dialogueLine.Character;
            string originalText = dialogueLine.Text;
            string originalTranslationKey = dialogueLine.TranslationKey;

            string processedText = BB.ProcessItemReplacements(text);

            dialogueLine.Character = character;
            dialogueLine.Text = processedText;
            dialogueLine.TranslationKey = translationKey;

            dialogueLabel.Set("dialogue_line", dialogueLine);
            dialogueLabel.Call("type_out");

            PlayVoice(character, translationKey);

            if (audioStreamPlayer.Playing)
            {
                await ToSignal(audioStreamPlayer, AudioStreamPlayer.SignalName.Finished);
            }
            else if (!string.IsNullOrEmpty(processedText))
            {
                await ToSignal(dialogueLabel, "finished_typing");
            }

            // Restore original values
            dialogueLine.Character = originalCharacter;
            dialogueLine.Text = originalText;
            dialogueLine.TranslationKey = originalTranslationKey;
        }

    }

}