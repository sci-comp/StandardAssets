using DialogueManagerRuntime;
using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    public partial class DialogueActor : InteractableArea
    { 
        [ExportCategory("Interactable")]
        [Export] public string _Title = "";
        [Export] public string _Details = "";

        [ExportCategory("Dialogue")]
        [Export] public string ActorID = "";
        [Export] public string StartTitle = "";
        [Export] public Resource DialogueResource;
        [Export] public Marker3D PlayerLocation;

        [ExportCategory("Animation")]
        [Export] public ActorAnimationController AnimationController;

        public CameraAngles CameraAngles;

        public override string Title => _Title;
        public override string Details => _Details;

        private float interactCooldown = 1.2f;
        private float interactTimer = 0.0f;
        
        protected PointOfInterest poi;
        private SFX sfx;
        private CameraAngle currentCameraAngle;

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

            DialogueBalloon.ActorSFXRequested += OnSFXRequested;
            DialogueManager.DialogueEnded += OnDialogueEnded;
            ActorSpawned?.Invoke(ActorID, this);
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;
            interactTimer += dt;
        }

        public override void _ExitTree()
        {
            ActorDestroyed?.Invoke(ActorID);
            DialogueBalloon.ActorSFXRequested -= OnSFXRequested;
            DialogueManager.DialogueEnded -= OnDialogueEnded;
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
                playerPosition = GlobalPosition - forward;

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

        protected void OnDialogueEnded(Resource dialogueResource)
        {
            CameraAngles.SetCameraPriority(currentCameraAngle, 0);
            AnimationController?.Resume();
        }

        protected void OnSFXRequested(string actorName, string sfxName)
        {
            if (ActorID == actorName)
            {
                sfx.PlaySound(sfxName, Position);
            }
        }

        public void SetCameraAngle(CameraAngle angle)
        {
            CameraAngles.SetCameraPriority(currentCameraAngle, 0);
            CameraAngles.SetCameraPriority(angle);
            currentCameraAngle = angle;
        }

    }

}

