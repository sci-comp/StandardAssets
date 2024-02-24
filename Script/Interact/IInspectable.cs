public interface IInspectable
{
    public string Title { get; }
    public string Details { get; }
    public void Inspect();
    public void Select();
    public void Deselect();
}

