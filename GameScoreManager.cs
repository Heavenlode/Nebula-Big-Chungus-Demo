using Godot;
using Nebula;
using System.Collections.Generic;

public partial class GameScoreManager : NetNode
{
    [Export]
    public PelletSpawner PelletSpawner;
    public List<Player> Players { get; } = new();
    public override void _NetworkProcess(int tick)
    {
        base._NetworkProcess(tick);

        if (Network.IsClient)
        {
            return;
        }

        CheckPelletCollisions();
        CheckPlayerCollisions();
    }

    private void CheckPelletCollisions()
    {
        if (PelletSpawner == null) return;

        var pelletPositions = PelletSpawner.PelletPositions;

        foreach (var player in Players)
        {
            var playerPos = player.GetWorldPosition();
            float collisionRadius = player.GetCollisionRadius();

            for (int i = 0; i < pelletPositions.Length; i++)
            {
                var pelletPos = pelletPositions[i];
                float distanceSquared = (playerPos.X - pelletPos.X) * (playerPos.X - pelletPos.X)
                                      + (playerPos.Z - pelletPos.Z) * (playerPos.Z - pelletPos.Z);

                if (distanceSquared < collisionRadius)
                {
                    player.Score++;
                    PelletSpawner.RespawnPellet(i);
                }
            }
        }
    }

    private void CheckPlayerCollisions()
    {
        List<Player> playersToDespawn = null;

        for (int i = 0; i < Players.Count; i++)
        {
            var player1 = Players[i];
            if (playersToDespawn?.Contains(player1) == true) continue;

            for (int j = i + 1; j < Players.Count; j++)
            {
                var player2 = Players[j];
                if (player1.Network.IsQueuedForDespawn || player2.Network.IsQueuedForDespawn) continue;

                if (playersToDespawn?.Contains(player2) == true) continue;

                var pos1 = player1.GetWorldPosition();
                var pos2 = player2.GetWorldPosition();

                float radius1 = player1.GetCollisionRadius();
                float radius2 = player2.GetCollisionRadius();

                float distanceSquared = (pos1.X - pos2.X) * (pos1.X - pos2.X)
                                      + (pos1.Z - pos2.Z) * (pos1.Z - pos2.Z);
                float combinedRadius = radius1 + radius2;

                if (distanceSquared < combinedRadius)
                {
                    // Collision detected - bigger player eats smaller one
                    Player bigger, smaller;
                    if (player1.Score >= player2.Score)
                    {
                        bigger = player1;
                        smaller = player2;
                    }
                    else
                    {
                        bigger = player2;
                        smaller = player1;
                    }

                    // Add 50% of smaller's score to bigger
                    bigger.Score += smaller.Score / 2;

                    // Queue the smaller player for despawn
                    playersToDespawn ??= new List<Player>();
                    playersToDespawn.Add(smaller);
                }
            }
        }

        // Despawn players that have been eaten
        if (playersToDespawn != null)
        {
            foreach (var player in playersToDespawn)
            {
                player.Network.Despawn();
                Players.Remove(player);
            }
        }
    }
}
