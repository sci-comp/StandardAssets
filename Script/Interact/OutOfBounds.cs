using Godot;

public partial class OutOfBounds : Area3D
{
    private Node3D playerResetPosition;

    public override void _Ready()
    {
        playerResetPosition = GetNode<Node3D>("SpawnPoint");
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is CharacterBody3D characterBody)
        {
            characterBody.GlobalTransform = new Transform3D(characterBody.GlobalTransform.Basis, playerResetPosition.GlobalTransform.Origin);
        }
    }

}

