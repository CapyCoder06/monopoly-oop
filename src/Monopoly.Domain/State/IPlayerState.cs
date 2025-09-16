namespace Monopoly.Domain.State;

public interface IPlayerState { }

public class NormalState : IPlayerState { }
public class InJailState : IPlayerState
{
    public int TurnsLeft { get; init; } = 3;
}
public class BankruptState : IPlayerState { }
