using Godot;

namespace Game
{
    public partial class Epigraph : Panel
    {
        [Export] public string voicePath = "res://Audio/Dialogue";

        private AudioStreamPlayer audioPlayer;
        private Label epigraphText;
        private LevelManager levelManager;
        private Resources resources;
        private TextureRect textureRect;

        public override void _Ready()
        {
            resources = GetNode<Resources>("/root/Resources");
            textureRect = GetNode<TextureRect>("TextureRect");
            epigraphText = GetNode<Label>("Panel/MarginContainer/Label");
            levelManager = GetNode<LevelManager>("/root/LevelManager");
            audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

            if (levelManager.LevelInfo.TryGetValue(levelManager.LevelIDAfterEpigraph, out LevelInfo levelInfo))
            {
                if (epigraphText != null && !string.IsNullOrEmpty(levelInfo.Epigraph))
                {
                    epigraphText.Text = levelInfo.Epigraph;
                }

                if (textureRect != null && !string.IsNullOrEmpty(levelInfo.Epigraph))
                {
                    if (resources.EpigraphTexture.TryGetValue(levelManager.LevelIDAfterEpigraph, out Texture2D tex))
                    {
                        textureRect.Texture = tex;
                    }
                }

                PlayVoice();

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

        private void PlayVoice()
        {
            string locale = TranslationServer.GetLocale().Split('_')[0];
            string path;
            
            path = $"{voicePath}/{locale}/Epigraph/{levelManager.LevelIDAfterEpigraph}.ogg";

            if (ResourceLoader.Exists(path))
            {
                AudioStream stream = GD.Load<AudioStream>(path);
                audioPlayer.Stream = stream;
                audioPlayer.Play();
            }
            else
            {
                GD.Print("[Epigraph] Path does not exist: ", path);
            }
        }

    }

}

