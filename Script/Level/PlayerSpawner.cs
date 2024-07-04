using Godot;

namespace Game
{
    public partial class PlayerSpawner : Node
    {
        [Export] public string PlayerPath = "res://Prefab/Player.tscn";

        private LevelManager levelManager;
        private PackedScene player;

        public override void _Ready()
        {
            levelManager = GetNode<LevelManager>("/root/LevelManager");

            levelManager.LevelLoaded += OnLevelLoaded;
            player = GD.Load<PackedScene>(PlayerPath);

            #region Null checks

            if (player == null)
            {
                GD.Print("[Player Spawner] Player is null");
            }

            if (levelManager == null)
            {
                GD.Print("[Player Spawner] levelManager is null");
            }

            #endregion

            if (levelManager.CurrentLevelName != null && levelManager.CurrentLevelName != "")
            {
                OnLevelLoaded();
            }

            GD.Print("[Player Spawner] Ready");
        }

        private void OnLevelLoaded()
        {
            if (levelManager.CurrentLevelInfo != null && levelManager.CurrentLevelInfo.PlayerExistsInLevel)
            {
                Node3D _spawnpoint = null;

                if (levelManager.RequestedSpawnpoint != "")
                {
                    _spawnpoint = (Node3D)levelManager.CurrentLevel.FindChild(levelManager.RequestedSpawnpoint);

                    if (_spawnpoint == null)
                    {
                        GD.PrintErr("[Player Spawner] Requested spawnpoint not found: " + levelManager.RequestedSpawnpoint);
                    }
                    else
                    {
                        GD.Print("[Player Spawner] Requested spawnpoint found");
                    }
                }

                _spawnpoint ??= (Node3D)levelManager.CurrentLevel.FindChild("SP_" + levelManager.CurrentLevel.Name);

                if (_spawnpoint == null)
                {
                    GD.PrintErr("[Player Spawner] No spawnpoint found in level: " + levelManager.CurrentLevel.Name);
                    return;
                }
                else
                {
                    GD.Print("[Player Spawner] Default spawnpoint found");
                }

                // Instantiate player
                CharacterBody3D playerInstance = (CharacterBody3D)player.Instantiate();
                levelManager.CurrentLevel.AddChild(playerInstance);
                playerInstance.Position = _spawnpoint.Position;
                GD.Print("[Player Spawner] Player instantiated at spawnpoint: " + _spawnpoint.Name);

            }

        }

    }

}

