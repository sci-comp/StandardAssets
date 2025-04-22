using Godot;

namespace Game
{
    public partial class LevelLifecycleTracker : Node3D
    {
        private int length = 64;

        public override void _EnterTree()
        {
            GD.PrintRich($"[color={ColorsHex.SkyBlue}]{new('-', length)}[/color]");
            string msg = TextFormatting.Bars($"{Name} Enter Tree", length);
            GD.PrintRich($"[color={ColorsHex.SkyBlue}]{msg}[/color]");
        }

        public override void _Ready()
        {
            string msg = TextFormatting.Bars($"{Name} Ready", length);
            GD.PrintRich($"[color={ColorsHex.MediumSeaGreen}]{msg}[/color]");
        }

        public override void _ExitTree()
        {
            string msg = TextFormatting.Bars($"{Name} Exit Tree", length);
            GD.PrintRich($"[color={ColorsHex.Salmon}]{msg}[/color]");
            GD.PrintRich($"[color={ColorsHex.Salmon}]{new('-', length)}[/color]");
        }
    }

}
