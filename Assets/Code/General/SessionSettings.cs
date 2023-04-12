using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SessionSettings
{
    List<SessionPlayer> players = new List<SessionPlayer>();

    public SessionPlayer AddPlayer(PlayerInput playerInput)
    {
        SessionPlayer player = new SessionPlayer(playerInput);
        players.Add(player);
        return player;
    }

    public bool SelectFighter(SessionPlayer sessionPlayer, FighterSelection fighterSelection)
    {
        sessionPlayer.SelectedFighter = fighterSelection;
        return true;
    }

    public bool SelectFighter(int playerIndex, FighterSelection fighterSelection)
    {
        SessionPlayer currentPlayer = players.Find(player => player.PlayerIndex == playerIndex);
        if (currentPlayer == null)
            return false;

        currentPlayer.SelectedFighter = fighterSelection;
        return true;
    }

    public void SpawnAllPlayers(Vector3[] spawnPositions)
    {
        if (spawnPositions == null || spawnPositions.Length == 0)
            spawnPositions = new Vector3[] { Vector3.zero };

        for (int i = 0; i < players.Count; i++)
        {
            players[i].SpawnPlayer(spawnPositions[i % spawnPositions.Length]);
        }
    }

    public void DestroyAllPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].DestroyPlayer();
        }
    }
}
