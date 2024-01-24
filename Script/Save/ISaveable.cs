using Godot;

public interface ISaveable
{
	public SaveData Save();
	public void Load(SaveData data);
	public StringName GetLoadKey();
}

