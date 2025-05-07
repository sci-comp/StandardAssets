using DialogueManagerRuntime;
using Godot;
using System.Collections.Generic;
using Toolbox;

namespace Game
{
    public partial class ActorAnimationController : AnimationTree
    {
        [Export] public float AverageTime { get; set; } = 20.0f;
        [Export] public float MinimumTime { get; set; } = 10.0f;
        [Export] public float MaximumTime { get; set; } = 30.0f;
        [Export] public Godot.Collections.Array<WeightedAnimation> IdleAnimations = [];

        private AnimationTree animationTree;
        private AnimationNodeStateMachine machine;
        private AnimationNodeStateMachinePlayback machinePlayback;
        private NextEventTimer timer;
        private DialogueActor dialogueActor;
        private readonly List<string> supportedGestures = ["greet", "hit"];

        public void Initialize(DialogueActor _dialogueActor)
        {
            dialogueActor = _dialogueActor;

            if (animationTree == null)
            {
                GD.PrintErr("[ActorAnimationController] AnimationTree is null on initialization");
                SetProcess(false);
                return;
            }

            machine = (AnimationNodeStateMachine)animationTree.TreeRoot;
            machinePlayback = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");

            timer = new NextEventTimer();
            AddChild(timer);
            timer.t_next_min = MinimumTime;
            timer.t_next_max = MaximumTime;
            timer.t_next_average = AverageTime;
            timer.loop = true;
            timer.EventTriggered += OnTimerTriggered;

            DialogueBalloon.ActorGestureRequested += OnGestureRequested;
        }

        public override void _ExitTree()
        {
            if (timer != null)
            {
                timer.EventTriggered -= OnTimerTriggered;
            }

            DialogueBalloon.ActorGestureRequested -= OnGestureRequested;
        }

        public void Pause()
        {
            timer?.Pause();
        }

        public void Resume()
        {
            timer?.Resume();
        }

        private void OnTimerTriggered()
        {
            string nextAnim = ChooseRandomAnimation();
            if (!string.IsNullOrEmpty(nextAnim))
            {
                machinePlayback.Travel(nextAnim);
            }
        }

        private string ChooseRandomAnimation()
        {
            if (IdleAnimations.Count == 0)
            {
                return string.Empty;
            }

            var names = new List<string>();
            var weights = new List<int>();

            foreach (WeightedAnimation anim in IdleAnimations)
            {
                names.Add(anim.AnimationName);
                weights.Add((int)(anim.Weight * 100));
            }

            return MathLib.Choice(names, weights);
        }

        private void OnGestureRequested(string actorName, string gestureName)
        {
            if (dialogueActor != null && actorName == dialogueActor.ActorID && supportedGestures.Contains(gestureName))
            {
                GD.Print($"[ActorAnimationController] Gesture requested for actor {actorName}, requesting: {gestureName}");
                machinePlayback.Travel(gestureName);
            }
        }

    }

}

