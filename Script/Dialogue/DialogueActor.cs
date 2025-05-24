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

        [ExportCategory("Animation")]
        [Export] public ActorAnimationController AnimationController;

        public override string Title => _Title;
        public override string Details => _Details;

        private float interactCooldown = 1.2f;
        private float interactTimer = 0.0f;
        private CameraAngles cameraAngles;
        protected PointOfInterest poi;
        private SFX sfx;
        private CameraAngle currentCameraAngle;

        public static event Action<string, DialogueActor> ActorSpawned;
        public static event Action<string> ActorDestroyed;

        public override void _Ready()
        {
            sfx = GetNode<SFX>("/root/SFX");
            poi = GetNode<PointOfInterest>("PointOfInterest");

            cameraAngles = GetNodeOrNull<CameraAngles>("CameraAngles");
            if (cameraAngles == null)
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

            if (StartTitle != "")
            {
                DialogueManager.ShowDialogueBalloon(DialogueResource, StartTitle);
            }
            else
            {
                DialogueManager.ShowDialogueBalloon(DialogueResource);
            }

            currentCameraAngle = cameraAngles.DefaultAngle;
            cameraAngles.SetCameraPriority(currentCameraAngle);

            AnimationController?.Pause();
            base.Interact();
        }

        protected void OnDialogueEnded(Resource dialogueResource)
        {
            cameraAngles.SetCameraPriority(currentCameraAngle, 0);
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
            cameraAngles.SetCameraPriority(currentCameraAngle, 0);
            cameraAngles.SetCameraPriority(angle);
            currentCameraAngle = angle;
        }

    }

}

