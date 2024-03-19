public interface ITriggeredPlatform
{
    public bool CanBeTriggered();
    public bool Enabled();
    public void Enable();
    public void Disable();
    public void Trigger();
}

