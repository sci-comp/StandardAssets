using Godot;

public partial class Debug : CanvasGroup
{
    [Export] public Label label;
    [Export] public SceneManager sceneManager;

    public override void _Ready()
    {
        sceneManager.SceneLoaded += OnSceneLoaded;
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey eventKey && eventKey.Pressed)
        {
            if (eventKey.Keycode == Key.Key0)
            {
                Visible = !Visible;
            }
        }
    }

    private void OnSceneLoaded()
    {
        label.Text = "Current scene: " + sceneManager.CurrentSceneName;
    }

}

