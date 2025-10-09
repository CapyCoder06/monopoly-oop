// Domain/Tiles/FreeParkingTile.cs
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;

namespace Monopoly.Domain.Tiles
{
    public sealed class FreeParkingTile : Tile
    {
        public FreeParkingTile(int index, string name = "Free Parking", Guid? tileId = null)
            : base(index, name, tileId ?? Guid.NewGuid())
        {
        }

        public override void OnLand(GameContext ctx, Player player, int lastDiceSum)
        {
        }
    }
}
