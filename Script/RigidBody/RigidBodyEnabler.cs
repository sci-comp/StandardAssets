using Godot;

public partial class RigidBodyEnabler : RigidBody3D
{
    public void Used()
    {
        FreezeMode = FreezeModeEnum.Kinematic;
    }
}
