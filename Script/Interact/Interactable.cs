using System;

public abstract partial class Interactable : Inspectable
{
    public event Action Interacted;

    public virtual void Interact()
    {
        Interacted?.Invoke();
    }

}