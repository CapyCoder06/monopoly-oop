namespace Monopoly.Domain.Core;

public class Player
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public int Position { get; private set; }
    public int Cash { get; private set; }

    public Player(string name, int startingCash = 1500)
    {
        Name = name;
        Cash = startingCash;
        Position = 0;
    }

    public void Move(int steps, int boardSize) => Position = (Position + steps) % boardSize;
    public void Pay(int amount) => Cash -= amount;
    public void Receive(int amount) => Cash += amount;
}
