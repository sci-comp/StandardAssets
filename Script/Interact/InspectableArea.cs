using Godot;

public abstract partial class InspectableArea : Area3D
{
    public virtual string Title => "";
    public virtual string Details => "";

    public virtual void Inspect()
    {
        // Do nothing
    }

    public virtual void Select()
    {
        // Do nothing
    }

    public virtual void Deselect()
    {
        // Do nothing
    }

}

