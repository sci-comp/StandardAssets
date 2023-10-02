using Godot;

public partial class Debug : Node
{
    [Export] public Label label;

    public override void _Ready()
    {
        GetTree().TreeChanged += OnTreeChanged;
    }

    private void OnTreeChanged()
    {
        label.Text = "Current scene: " + GetTree().CurrentScene.Name;
    }
}

