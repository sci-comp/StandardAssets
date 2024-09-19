using Godot;
using System;

namespace Game
{
    public partial class PlayerSpawner : Node
    {
        [Export] public string PlayerPath = "res://Prefab/Player.tscn";
        [Export] public string PlayerID = "Player1";

        private LevelManager levelManager;
        private SaveManager saveManager;
        private PackedScene playerPackedScene;

        public event Action<CharacterHub> PlayerSpawned;

        public override void _Ready()
        {
            levelManager = GetNode<LevelManager>("/root/LevelManager");
            saveManager = GetNode<SaveManager>("/root/SaveManager");

            levelManager.LevelLoaded += OnLevelLoaded;
            playerPackedScene = GD.Load<PackedScene>(PlayerPath);

            #region Null checks

            if (playerPackedScene == null)
            {
                GD.Print("[PlayerSpawner] Player is null");
            }

            if (levelManager == null)
            {
                GD.Print("[PlayerSpawner] levelManager is null");
            }

            #endregion

            if (levelManager.CurrentLevelName != null && levelManager.CurrentLevelName != "")
            {
                OnLevelLoaded();
            }

            GD.Print("[PlayerSpawner] Ready");
        }

        private void OnLevelLoaded()
        {
            if (levelManager.CurrentLevelInfo != null && levelManager.CurrentLevelInfo.PlayerExistsInLevel)
            {
                Marker3D _spawnpoint = null;

                if (saveManager.GetSpawnpoint() != "")
                {
                    _spawnpoint = saveManager.FindSpawnpoint();

                    if (_spawnpoint == null)
                    {
                        GD.PrintErr("[PlayerSpawner] Requested spawnpoint not found: " + saveManager.GetSpawnpoint());
                    }
                    else
                    {
                        GD.Print("[PlayerSpawner] Requested spawnpoint found");
                    }
                }

                _spawnpoint ??= (Marker3D)levelManager.CurrentLevel.FindChild("SP_" + levelManager.CurrentLevel.Name);

                if (_spawnpoint == null)
                {
                    GD.PrintErr("[PlayerSpawner] No spawnpoint found in level: " + levelManager.CurrentLevel.Name);
                    return;
                }
                else
                {
                    GD.Print("[PlayerSpawner] Default spawnpoint found");
                }

                // Give time for Autoloads to run their Ready methods when starting from a debug scene
                CallDeferred("SpawnPlayerAtEndOfFrame", _spawnpoint);
            }

        }

        private void SpawnPlayerAtEndOfFrame(Node3D _spawnpoint)
        {
            // Instantiate player
            Node characterInstance = playerPackedScene.Instantiate();
            levelManager.CurrentLevel.AddChild(characterInstance);
            
            if (characterInstance is CharacterHub characterHub)
            {
                characterHub.SetCharacterPosition(_spawnpoint.Position);
                PlayerSpawned?.Invoke(characterHub);
            }
            else
            {
                GD.PrintErr("[PlayerSpawner] playerInstance is not of type CharacterController");
            }

            GD.Print("[PlayerSpawner] Player instantiated at spawnpoint: " + _spawnpoint.Name);
        }

    }

}

