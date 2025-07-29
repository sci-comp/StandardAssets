using DialogueManagerRuntime;
using Godot;
using System;

namespace Game
{
    public partial class DialogueActor : InteractableArea
    {
        [ExportCategory("Interactable")]
        [Export] public string _Title = "";
        [Export] public string _Details = "";

        [ExportCategory("Dialogue")]
        [Export] public string ActorID = "";
        [Export] public string ActorInstanceID = "";
        [Export] public string StartTitle = "";
        [Export] public Resource DialogueResource;
        [Export] public Marker3D PlayerLocation;

        [ExportCategory("Animation")]
        [Export] public ActorAnimationController AnimationController;

        [ExportCategory("Quest Visibility")]
        [Export] public bool DisableForQuest = false;
        [Export] public bool EnableForQuest = false;
        [Export] public bool SpecificQuestStateOnly = false;
        [Export] public string QuestID = "";
        [Export] public QuestProgress RequiredProgress = QuestProgress.Unknown;

        public bool Active = false;
        public CameraAngles CameraAngles;

        public override string Title => _Title;
        public override string Details => _Details;

        private bool useQuestVisibility = false;
        private float interactCooldown = 1.2f;
        private float interactTimer = 0.0f;

        protected PointOfInterest poi;
        private SFX sfx;
        private CameraAngle currentCameraAngle;
        private Journal journal;

        public static event Action<string, DialogueActor> ActorSpawned;
        public static event Action<string> ActorDestroyed;
        public static event Action<Vector3, float> DialogueStarted;

        public override void _Ready()
        {
            sfx = GetNode<SFX>("/root/SFX");
            poi = GetNode<PointOfInterest>("PointOfInterest");

            CameraAngles = GetNodeOrNull<CameraAngles>("CameraAngles");
            if (CameraAngles == null)
            {
                GD.PushWarning("[DialogueActor] Missing CameraAngles node: ", Name);
            }

            AnimationController = GetNodeOrNull<ActorAnimationController>("ActorAnimationController");

            useQuestVisibility = EnableForQuest || DisableForQuest ? true : false;

            if (useQuestVisibility)
            {
                journal = GetNode<Journal>("/root/Journal");

                if (EnableForQuest && DisableForQuest)
                {
                    GD.PrintErr("[DialogueActor] Invalid settings: (EnableForQuest && DisableForQuest) == true");
                }

                journal.QuestProgressUpdated += QuestProgressUpdated;
                CallDeferred(nameof(UpdateActorVisibility));
            }
            DialogueManager.DialogueEnded += OnDialogueEnded;
            BodyEntered += OnAreaEntered;
            BodyExited += OnAreaExited;

            ActorSpawned?.Invoke(string.IsNullOrEmpty(ActorInstanceID) ? ActorID : ActorInstanceID, this);
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;
            interactTimer += dt;
        }

        public override void _ExitTree()
        {
            ActorDestroyed?.Invoke(string.IsNullOrEmpty(ActorInstanceID) ? ActorID : ActorInstanceID);
            DialogueManager.DialogueEnded -= OnDialogueEnded;

            if (useQuestVisibility && journal != null)
            {
                journal.QuestProgressUpdated -= QuestProgressUpdated;
            }
        }

        public override void Interact(string playerID)
        {
            if (interactTimer < interactCooldown)
            {
                return;
            }
            interactTimer = 0.0f;

            if (DialogueResource == null)
            {
                GD.PrintErr("[DialogueActor] DialogueResource == null");
                return;
            }

            GD.Print($"[DialogueActor] Requesting a conversation with {ActorID}");

            Vector3 playerPosition;
            float playerYaw;

            if (PlayerLocation != null)
            {
                playerPosition = PlayerLocation.GlobalPosition;
                playerYaw = PlayerLocation.GlobalRotation.Y;
            }
            else
            {
                Vector3 forward = -GlobalTransform.Basis.Z;
                playerPosition = GlobalPosition - (2 * forward);

                Vector3 directionToNPC = (GlobalPosition - playerPosition).Normalized();
                playerYaw = Mathf.Atan2(directionToNPC.X, -directionToNPC.Z);
            }

            DialogueStarted?.Invoke(playerPosition, playerYaw);

            if (StartTitle != "")
            {
                DialogueManager.ShowDialogueBalloon(DialogueResource, StartTitle);
            }
            else
            {
                DialogueManager.ShowDialogueBalloon(DialogueResource);
            }

            currentCameraAngle = CameraAngles.DefaultAngle;
            CameraAngles.SetCameraPriority(currentCameraAngle);

            AnimationController?.Pause();
            base.Interact();
        }

        private void QuestProgressUpdated(string questID, QuestProgress newProgress)
        {
            if (questID == QuestID)
            {
                UpdateActorVisibility();
            }
        }

        private void UpdateActorVisibility()
        {
            if (!useQuestVisibility)
            {
                return;
            }

            bool shouldBeVisible = true;

            if (EnableForQuest)
            {
                if (SpecificQuestStateOnly)
                {
                    shouldBeVisible = journal.GetQuestProgress(QuestID) == RequiredProgress;
                }
                else
                {
                    shouldBeVisible = journal.GetQuestProgress(QuestID) >= RequiredProgress;
                }
            }
            else if (DisableForQuest)
            {
                if (SpecificQuestStateOnly)
                {
                    shouldBeVisible = journal.GetQuestProgress(QuestID) != RequiredProgress;
                }
                else
                {
                    shouldBeVisible = journal.GetQuestProgress(QuestID) < RequiredProgress;
                }
            }

            SetActorVisibility(shouldBeVisible);
        }

        private void SetActorVisibility(bool visible)
        {
            Visible = visible;
            Monitoring = visible;
            Monitorable = visible;

            if (visible)
            {
                poi?.ShowOnCompass();
            }
            else
            {
                poi?.HideFromCompass();
            }
        }

        protected void OnDialogueEnded(Resource dialogueResource)
        {
            CameraAngles.SetCameraPriority(currentCameraAngle, 0);
            AnimationController?.Resume();
        }

        private void OnAreaEntered(Node3D area)
        {
            Active = true;
        }

        private void OnAreaExited(Node3D area)
        {
            Active = false;
        }

    }

}

