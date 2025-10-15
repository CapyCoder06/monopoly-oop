using Monopoly.CrossBridge;
using Monopoly.UnityAdapter;
using UnityEngine;
using System.Collections.Generic;

public class GameController : MonoBehaviour, IGameBridge
{
    private IDiceBridge _dice;
    private IEventBridge _eventBus;

    private Dictionary<string, PlayerToken> _players = new();
    private string _currentPlayer = "P1";

    private void Awake()
    {
        _dice = new UnityDiceAdapter();
        _eventBus = new UnityEventBus();

        FindObjectOfType<GameBridgeProxy>().Init(this);
        Debug.Log("✅ GameController initialized and linked to GameBridgeProxy.");
    }

    public void RegisterPlayer(PlayerToken token)
    {
        if (!_players.ContainsKey(token.PlayerId))
            _players[token.PlayerId] = token;
    }

    public void StartNewGame()
    {
        _currentPlayer = "P1";
        Debug.Log("🎯 Game started! Player P1 begins.");
    }

    public void RollDice(string playerSlotId)
    {
        if (playerSlotId != _currentPlayer)
        {
            Debug.Log($"⏳ Not {playerSlotId}'s turn!");
            return;
        }

        var (d1, d2, sum, isDouble) = _dice.Roll();
        Debug.Log($"🎲 {playerSlotId} rolled {d1} + {d2} = {sum}");

        if (_players.TryGetValue(playerSlotId, out var token))
        {
            token.MoveToTile(token.Position + sum);
        }

        _eventBus.Publish("DiceRolled", new { playerSlotId, d1, d2, sum, isDouble });

        // Nếu không phải double thì đổi lượt
        if (!isDouble)
            SwitchTurn();
    }

    private void SwitchTurn()
    {
        _currentPlayer = (_currentPlayer == "P1") ? "P2" : "P1";
        Debug.Log($"🔁 Next turn: {_currentPlayer}");
    }

    public void EndTurn() { /* có thể để trống */ }
}
