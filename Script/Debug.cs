using Godot;

public partial class Debug : CanvasGroup
{
    [Export] public Label label;

    public override void _Ready()
    {
        GetTree().TreeChanged += OnTreeChanged;

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

    private void OnTreeChanged()
    {
        label.Text = "Current scene: " + GetTree().CurrentScene.Name;
    }

}

