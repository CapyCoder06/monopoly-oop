namespace Monopoly.CrossBridge
{
    public interface IGameBridge
    {
        void StartNewGame();
        void RollDice(string playerSlotId);
        void EndTurn();
    }

    public interface IDiceBridge
    {
        (int d1, int d2, int sum, bool isDouble) Roll();
    }

    public interface IEventBridge
    {
        void Publish(string eventName, object payload);
    }

    public class PlayerDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Money { get; set; }
        public int Position { get; set; }
    }
}
