using Monopoly.CrossBridge;
using UnityEngine;

namespace Monopoly.UnityAdapter
{
    public class GameBridgeProxy : MonoBehaviour
    {
        public static IGameBridge? Bridge;

        public void Init(IGameBridge bridge)
        {
            Bridge = bridge;
            Debug.Log("✅ GameBridgeProxy initialized");
        }

        public void RollDice(string slotId)
        {
            Bridge?.RollDice(slotId);
        }

        public void StartNewGame()
        {
            Bridge?.StartNewGame();
        }

        public void EndTurn()
        {
            Bridge?.EndTurn();
        }
    }
}
