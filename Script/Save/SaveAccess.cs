using Godot;
using System;
using System.Collections.Generic;

public sealed partial class SaveAccess
{
	private readonly HashSet<SaveData> _fileData;
	private Func<FileAccess> _initFileAccess;

	private SaveAccess(FileAccess readAccess)
	{
		if (readAccess != null)
		{
			_fileData = GetFileData(readAccess);
		}
		else
		{
			_fileData = new();
		}
	}

	// Opens a SaveAccess to a file.  Note: This will always successfully return a SaveAccess, even if the file does not exist (in that case, a new file will be created when Commit() is called)
	public static SaveAccess Open(string filePath)
	{
		FileAccess readAccess = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);

		SaveAccess saveAccess = new(readAccess);
		readAccess?.Dispose();

		saveAccess._initFileAccess = () => FileAccess.Open(filePath, FileAccess.ModeFlags.WriteRead);

		return saveAccess;
	}

	// Saves all ISaveable children of root (recursively). Make sure to call Commit() for data to be stored in the file.
	public void SaveTree(Node root)
	{
		RunInChildrenRecursive(root, (ISaveable node) => { SaveObject(node); });
	}

	// Saves an ISaveable object. Make sure to call Commit() for data to be stored in the file.
	public void SaveObject(ISaveable saveObject)
	{
        SaveDataMethod(saveObject.Save());
	}

	// Saves SaveData, make sure to call Commit() for data to be stored in the file.
	public void SaveDataMethod(SaveData saveData)
	{
		if (_fileData.Contains(saveData))
		{
			_fileData.Remove(saveData);
			_fileData.Add(saveData);
		}

		_fileData.Add(saveData);
	}

	// Loads all ISaveable children of root (recursively), if data cannot be found for an object, it will not be loaded.
	public void LoadTree(Node root)
	{
		RunInChildrenRecursive(root, (ISaveable node) => { LoadObject(node); });
	}

	// Loads an ISaveable object, unless no data exists for the object.
	public void LoadObject(ISaveable loadObject)
	{
		if(loadObject == null)
			return;

		SaveData loadedData = LoadData(loadObject.GetLoadKey());

		if(loadedData != null)
			loadObject.Load(loadedData);
	}

	// Returns the SaveData with the specified key. Returns null if no data with the given key exists.
	public SaveData LoadData(StringName key)
	{
		if (_fileData.TryGetValue(new SaveData(key), out SaveData data))
			return data;

		return null;
	}

	// Deletes the SaveData with the specified key. Returns whether the SaveData was successfully removed.
	public bool RemoveData(StringName key)
	{
		return _fileData.Remove(new SaveData(key));
	}

	// Deletes all stored SaveData.
	public void Clear()
	{
		_fileData.Clear();
	}

	// Commits all changes to the file. Only call Commit() when you actually need
	// it, there could be a significant performance impact from repeated commits.
	public void Commit()
	{
		FileAccess fileAccess = _initFileAccess.Invoke();

		foreach (SaveData data in _fileData)
			if (data != null)
				fileAccess.StoreLine(data.ToJson());

		fileAccess.Dispose();
	}

	// Saves a tree, but instead of adding it to a file, returns it as a HashSet of SaveData.
	public static HashSet<SaveData> SaveTreeToSaveData(Node root)
	{
		HashSet<SaveData> saveData = new();

		RunInChildrenRecursive(root, (ISaveable node) => saveData.Add(node.Save()));

		return saveData;
	}

	// Loads a tree, but instead of getting data from a file, loads from a HashSet of SaveData.
	public static void LoadTreeFromSaveData(Node root, HashSet<SaveData> saveData)
	{
		RunInChildrenRecursive(root, (ISaveable node) =>
		{
			if (saveData.TryGetValue(new SaveData(node.GetLoadKey()), out SaveData data))
				node.Load(data);

			node.Load(null);
		});
	}

	private static HashSet<SaveData> GetFileData(FileAccess fileAccess)
	{
		HashSet<SaveData> fileData = new();

		while (!fileAccess.EofReached())
		{
			string line = fileAccess.GetLine();

			if (string.IsNullOrEmpty(line))
				continue;

			fileData.Add(SaveData.FromJson(line));
		}

		return fileData;
	}

	private static void RunInChildrenRecursive<T>(Node parent, Action<T> action)
	{
		RunInChildrenRecursive(parent, (Node node) => { if (node is T t) action.Invoke(t); });
	}

	private static void RunInChildrenRecursive(Node parent, Action<Node> action)
	{
		RunInChildren(parent);

		void RunInChildren(Node parentNode)
		{
			foreach (Node node in parentNode.GetChildren())
			{
				if (node.GetChildCount() > 0)
					RunInChildren(node);

				action.Invoke(node);
			}
		}
	}
}
























/* 
 * 

    // Opens a SaveAccess to a file that reads and writes a compressed file.  Note: This will always successfully return a SaveAccess, even if the file does not exist (in that case, a new file will be created when Commit() is called)
	public static SaveAccess OpenCompressed(string filePath, FileAccess.CompressionMode compressionMode = FileAccess.CompressionMode.Fastlz)
	{
		FileAccess readAccess = FileAccess.OpenCompressed(filePath, FileAccess.ModeFlags.Read, compressionMode);

		SaveAccess saveAccess = new(readAccess);
		readAccess?.Dispose();

		saveAccess._initFileAccess = () => FileAccess.OpenCompressed(filePath, FileAccess.ModeFlags.WriteRead, compressionMode);

		return saveAccess;
	}

	// Opens a SaveAccess to a file that reads and writes to an encrypted file using a binary key.
	// Note: The provided key must be 32 bytes long.
	// Note: This will always successfully return a SaveAccess, even if the file does not exist (in that case, a new file will be created when Commit() is called)
	public static SaveAccess OpenEncrypted(string filePath, byte[] key)
	{
		FileAccess readAccess = FileAccess.OpenEncrypted(filePath, FileAccess.ModeFlags.Read, key);

		SaveAccess saveAccess = new(readAccess);
		readAccess?.Dispose();

		saveAccess._initFileAccess = () => FileAccess.OpenEncrypted(filePath, FileAccess.ModeFlags.WriteRead, key);

		return saveAccess;
	}

	// Opens a SaveAccess to a file that reads and writes to an encrypted file using a string password.  Note: This will always successfully return a SaveAccess, even if the file does not exist (in that case, a new file will be created when Commit() is called)
	public static SaveAccess OpenEncryptedWithPass(string filePath, string pass)
	{
		FileAccess readAccess = FileAccess.OpenEncryptedWithPass(filePath, FileAccess.ModeFlags.Read, pass);

		SaveAccess saveAccess = new(readAccess);
		readAccess?.Dispose();

		saveAccess._initFileAccess = () => FileAccess.OpenEncryptedWithPass(filePath, FileAccess.ModeFlags.WriteRead, pass);

		return saveAccess;
	}

 */