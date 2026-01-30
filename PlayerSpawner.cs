using System.Collections.Generic;
using Godot;
using Nebula;

public partial class PlayerSpawner : NetNode
{
    [Export]
    public PackedScene CharacterScene;

    [Export]
    public Control StartScrenn;
    
    [Export]
    public Control ScoreContainer;

    private int playerCount = 0;

    public Dictionary<UUID, Player> PlayerCharacters { get; } = new();


    [NetFunction(Source = NetFunction.NetworkSources.Client, ExecuteOnCaller = false)]
    public void JoinGame()
    {
        var callerId = NetRunner.Instance.GetPeerId(Network.CurrentWorld.NetFunctionContext.Caller);
        
        // Check if player already exists and is still valid
        if (PlayerCharacters.TryGetValue(callerId, out var existingPlayer) && IsInstanceValid(existingPlayer))
        {
            return;
        }
        
        var newPlayer = CharacterScene.Instantiate<Player>();
        Network.CurrentWorld.Spawn(newPlayer, inputAuthority: Network.CurrentWorld.NetFunctionContext.Caller);
        PlayerCharacters[callerId] = newPlayer;
    }

    public void OnPlayerDespawn()
    {
        StartScrenn.Visible = true;
        ScoreContainer.Visible = false;
    }

    public void _OnPlay()
    {
        StartScrenn.Visible = false;
        ScoreContainer.Visible = true;
        JoinGame();
    }

    public void _OnExit()
    {
        GetTree().Quit();
    }
}