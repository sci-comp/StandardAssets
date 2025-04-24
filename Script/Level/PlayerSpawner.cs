using Godot;
using System;

namespace Game
{
    public partial class PlayerSpawner : Node
    {
        [Export] public PackedScene Player;
        [Export] public string PlayerID = "Player1";

        public bool PlayerExists { get; set; } = false;
        public CharacterHub CharacterHub { get; set; }

        private LevelManager levelManager;
        private SaveManager saveManager;
        private PackedScene playerPackedScene;

        public override void _Ready()
        {
            levelManager = GetNode<LevelManager>("/root/LevelManager");
            saveManager = GetNode<SaveManager>("/root/SaveManager");

            levelManager.LevelLoaded += OnLevelLoaded;
            levelManager.BeginUnloadingLevel += OnBeginUnloadingLevel;
            //playerPackedScene = GD.Load<PackedScene>(PlayerPath);
            playerPackedScene = Player;

            if (playerPackedScene == null)
            {
                GD.Print("[PlayerSpawner] Player is null");
            }

            if (levelManager.CurrentLevelID != null && levelManager.CurrentLevelID != "")
            {
                OnLevelLoaded();
            }

            GD.PrintRich($"[PlayerSpawner] [color={ColorsHex.MediumSeaGreen}]Ready[/color]");
        }

        private void OnBeginUnloadingLevel(string levelID, string spawnpoint)
        {
            PlayerExists = false;
            CharacterHub = null;
        }

        private void OnLevelLoaded()
        {
            if (levelManager.CurrentLevelInfo != null && levelManager.CurrentLevelInfo.PlayerExistsInLevel)
            {
                if (saveManager.SpawnpointName != "")
                {
                    Marker3D _spawnpoint = saveManager.FindSpawnpoint();

                    if (_spawnpoint != null)
                    {
                        // Give time for Autoloads to run their Ready methods when starting from a debug scene
                        CallDeferred("SpawnPlayerAtEndOfFrame", _spawnpoint);
                    }
                }
            }
        }

        private void SpawnPlayerAtEndOfFrame(Node3D _spawnpoint)
        {
            Node characterInstance = playerPackedScene.Instantiate();
            levelManager.CurrentLevel.AddChild(characterInstance);
            
            if (characterInstance is CharacterHub characterHub)
            {
                characterHub.SetCharacterPosition(_spawnpoint.Position);
                characterHub.SetCharacterRotation(_spawnpoint.Rotation);
                CharacterHub = characterHub;
            }
            else
            {
                GD.PrintErr("[PlayerSpawner] playerInstance is not of type CharacterHub");
            }

            GD.Print("[PlayerSpawner] Player instantiated at spawnpoint: " + _spawnpoint.Name);
        }

    }

}

