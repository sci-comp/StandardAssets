using Godot;

[GlobalClass]
public partial class WeightedAnimation : Resource
{
    [Export] public string AnimationName { get; set; } = "";
    [Export] public float Weight { get; set; } = 1.0f;

    public WeightedAnimation(string name, float weight)
    {
        AnimationName = name;
        Weight = weight;
    }

}

