using Godot;

namespace Game
{
    public partial class Epigraph : Panel
    {
        private Resources resources;
        private TextureRect textureRect;
        private Label textLabel;
        private LevelManager levelManager;

        public override void _Ready()
        {
            resources = GetNode<Resources>("/root/Resources");
            textureRect = GetNode<TextureRect>("TextureRect");
            textLabel = GetNode<Label>("Panel/MarginContainer/Label");
            levelManager = GetNode<LevelManager>("/root/LevelManager");
           
            LevelInfo targetLevelInfo = levelManager.GetLevelInfo(levelManager.levelIDAfterEpigraph);

            if (textLabel != null && !string.IsNullOrEmpty(targetLevelInfo.Epigraph))
            {
                textLabel.Text = targetLevelInfo.Epigraph;
            }

            if (textureRect != null && !string.IsNullOrEmpty(targetLevelInfo.Epigraph))
            {
                textureRect.Texture = resources.GetEpigraphTexture(levelManager.levelIDAfterEpigraph);
            }

            Mouse.SetConfinedHidden();

            SetProcess(true);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey eventKey && eventKey.Pressed ||
                @event is InputEventMouseButton eventMouse && eventMouse.Pressed ||
                @event is InputEventJoypadButton eventButton && eventButton.Pressed)
            {
                levelManager.ReturningFromEpigraph();
                GetViewport().SetInputAsHandled();
            }
        }

    }

}

