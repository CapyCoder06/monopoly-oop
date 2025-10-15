using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Tiles;
using Monopoly.Domain.Events; 

namespace Monopoly.Domain.Core;

public sealed class GameContext
{
    public Board Board { get; }
    public IDomainEventBus Bus { get; }
    public IWallet Wallet { get; }
    public GameContext(Board board, IDomainEventBus bus, IWallet wallet)
    {
        Board = board;
        Bus = bus;
        Wallet = wallet;
    }

    public bool OwnerHasFullColorSet(Guid OwnerId, PropertyColor color)
    {
        for (int i = 0; i < Board.Tiles.Count; i++)
        {
            var tile = Board.Tiles[i];
            if (tile is PropertyTile propertyTile && propertyTile.Color == color)
            {
                if (propertyTile.OwnerId != OwnerId)
                    return false;
            }
        }
        return true;
    }

    public int CountOwnerRailroads(Guid OwnerId)
    {
        int count = 0;
        for (int i = 0; i < Board.Tiles.Count; i++)
        {
            var tile = Board.Tiles[i];
            if (tile is RailroadTile railroadTile && railroadTile.OwnerId == OwnerId)
            {
                count++;
            }
        }
        return count;
    }
    public int CountOwnerUtilities(Guid OwnerId)
    {
        int count = 0;
        for (int i = 0; i < Board.Tiles.Count; i++)
        {
            var tile = Board.Tiles[i];
            if (tile is UtilityTile utilityTile && utilityTile.OwnerId == OwnerId)
            {
                count++;
            }
        }
        return count;
    }
}
