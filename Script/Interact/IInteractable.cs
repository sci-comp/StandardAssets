using System;

namespace Game
{
    public interface IInteractable : IInspectable
    {
        public event Action Interacted;

        public void Interact();
    }

}

