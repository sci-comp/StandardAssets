using System;

namespace Game
{
    public interface IFlag
    {
        public event Action OnFlagRaised;
        public event Action OnFlagLowered;
        
        public abstract void RaiseFlag();

        public abstract void LowerFlag();
    }

}

