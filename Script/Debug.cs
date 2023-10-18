using Godot;

public partial class Debug : CanvasGroup
{
    [Export] public Label CurrentSceneLabel;
    [Export] public Label PerformanceLabel;
    [Export] public LevelManager LevelManager;

    public override void _Ready()
    {
        LevelManager.LevelLoaded += OnSceneLoaded;
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
        CurrentSceneLabel.Text = "Current scene: " + LevelManager.CurrentLevelName;
    }

    public override void _Process(double _delta)
    {
        float delta = (float) _delta;

        float fps = (float) Engine.GetFramesPerSecond();

        float mspf = 1000.0f / fps;
        int totalObjects = (int) RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalObjectsInFrame);

        float totalPrimitives = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalPrimitivesInFrame) * 0.001f;
        int totalDrawCalls = (int) RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalDrawCallsInFrame);

        PerformanceLabel.Text = string.Format(
            "{0} FPS ({1:0.##} mspf)\nCurrently rendering:\n{2} objects\n{3:0.###}K primitive indices\n{4} draw calls",
            fps, mspf, totalObjects, totalPrimitives, totalDrawCalls
        );
    }

}

