namespace Monopoly.Application.DTO;

public record PlayerVM(Guid Id, string Name, int Position, int Cash);
public record BoardVM(int Size);
public record TileVM(int Index, string Kind, string Name);
public record ToastVM(string Message, string Level = "info");
public record PlayerMovedUiEvent(Guid PlayerId, int NewPosition, int DiceSum, bool IsDouble);
public record TurnEndedUiEvent(Guid PlayerId);