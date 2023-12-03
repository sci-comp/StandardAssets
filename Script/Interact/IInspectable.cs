public interface IInspectable
{
    string Name { get; }
    string Details { get; }
    void Inspect();
    void Select();
    void Deselect();
}
