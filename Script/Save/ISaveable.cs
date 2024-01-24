using Godot;

public interface ISaveable
{
	public SaveData Save(params Variant[] parameters);
	public void Load(SaveData data, params Variant[] parameters);
	public StringName GetLoadKey(params Variant[] parameters);
}

