using Monopoly.Domain.State;
using Monopoly.Domain.Abstractions;
namespace Monopoly.Domain.Core;

public class Player
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public int Position { get; set; }
    public int Cash { get; set; }
    public IPlayerState CurrentState { get; set; } = new NormalState();

    public IWallet Wallet { get; set; } 
    public int JailCard { get; private set; } = 0;
    public void GrantJailCard() => JailCard++;


    public Player(string name, int startingCash = 1500)
    {
        Name = name;
        Cash = startingCash;
        Position = 0;
    }

    public void Move(int steps, int boardSize) => Position = (Position + steps) % boardSize;
    public void Pay(int amount) => Cash -= amount;
    public void Receive(int amount) => Cash += amount;
    public bool TryDebit(int amount)
    {
        if (Cash < amount) return false;
        Cash -= amount;
        return true;
    }
    public bool TryConsumeJailCard()
    {
        if (JailCard < 1) return false; 
        JailCard--;
        return true;
    } 
}