using System;

namespace Game
{

    public abstract partial class Interactable : Inspectable
    {
        public event Action Interacted;

        public virtual void Interact()
        {
            Interacted?.Invoke();
        }

    }

}

