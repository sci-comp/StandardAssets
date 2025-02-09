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
           
            if (levelManager.LevelInfo.TryGetValue(levelManager.levelIDAfterEpigraph, out LevelInfo levelInfo))
            {
                if (textLabel != null && !string.IsNullOrEmpty(levelInfo.Epigraph))
                {
                    textLabel.Text = levelInfo.Epigraph;
                }

                if (textureRect != null && !string.IsNullOrEmpty(levelInfo.Epigraph))
                {
                    if (resources.EpigraphTexture.TryGetValue(levelManager.levelIDAfterEpigraph, out Texture2D tex))
                    {
                        textureRect.Texture = tex;
                    }
                }
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

