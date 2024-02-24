using Godot;

public partial class Preferences : Node
{
    private readonly string savePath = "user://preferences.tres";

    public PreferencesResource Data { get; set; } = new();

    public override void _Ready()
    {
        //Input.MouseMode = Input.MouseModeEnum.Captured;

        if (ResourceLoader.Exists(savePath))
        {
            GD.Print(savePath);
            Data = (PreferencesResource)ResourceLoader.Load(savePath);
            GD.Print("Preferences loaded");
        }
        else
        {
            GD.Print("Preferences not found, using defaults");
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent.IsActionPressed("exit"))
        {
            GetTree().Quit();
        }
    }

    public void SavePreferences()
    {
        ResourceSaver.Save(Data, savePath);
        GD.Print("Saved player preferences to: " + savePath);
    }

}

