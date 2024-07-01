using System;

public abstract partial class InteractableArea : InspectableArea
{
    public event Action Interacted;

    public virtual void Interact()
    {
        Interacted?.Invoke();
    }

}

