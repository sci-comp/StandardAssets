using Godot;

public partial class Game : Node
{

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.P)
        {
            GetTree().Quit();
        }
    }
}
