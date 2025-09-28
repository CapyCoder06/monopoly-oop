using System;
using System.Linq;
using Xunit;
using Monopoly.Domain.Core;
using Monopoly.Domain.Tiles;
using Monopoly.Domain.Strategy;
using Monopoly.Domain.Events;
using Monopoly.Domain.Abstractions;

namespace Monopoly.Domain.Tests
{
    public class RentTests
    {
        private static Board NewBoard(int size = 40)
        {
            var tiles = Enumerable.Repeat<Tile>(default!, size).ToList();
            return new Board(tiles);
        }

        private static GameContext NewCtx(Board b, out InMemoryDomainEventBus bus)
        {
            bus = new InMemoryDomainEventBus();
            return new GameContext(b, bus);
        }

        // -----------------------
        // Property: thường & monopoly ×2
        // -----------------------
        [Fact]
        public void PropertyRent_NoMonopoly_ReturnsBaseRent()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out _);

            var owner = Guid.NewGuid();
            var p = new PropertyTile(index: 6, name: "Oriental Ave",
                                     color: PropertyColor.LightBlue,
                                     price: 100, baseRent: 6);
            p.OwnerId = owner;

            // Đặt ô lên board nếu GameContext dùng board để tính full-set
            board.Tiles[6] = p;

            var strat = new PropertyRentStrategy(p.BaseRent);
            var rent = strat.CalculateRent(ctx, p, lastDiceSum: 0);

            Assert.Equal(6, rent);
        }

        [Fact]
        public void PropertyRent_Monopoly_DoublesBaseRent()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out _);

            var owner = Guid.NewGuid();

            var p1 = new PropertyTile(6, "Oriental Ave",   PropertyColor.LightBlue, 100, 6) { OwnerId = owner };
            var p2 = new PropertyTile(8, "Vermont Ave",    PropertyColor.LightBlue, 100, 6) { OwnerId = owner };
            var p3 = new PropertyTile(9, "Connecticut Ave",PropertyColor.LightBlue, 120, 8) { OwnerId = owner };

            board.Tiles[6] = p1;
            board.Tiles[8] = p2;
            board.Tiles[9] = p3;

            var strat = new PropertyRentStrategy(p1.BaseRent);
            var rent  = strat.CalculateRent(ctx, p1, lastDiceSum: 0);

            Assert.Equal(p1.BaseRent * 2, rent);
        }

        // -----------------------
        // Railroad: 1..4 tuyến cho cùng owner (25/50/100/200)
        // -----------------------
        [Theory]
        [InlineData(1, 25)]
        [InlineData(2, 50)]
        [InlineData(3, 100)]
        [InlineData(4, 200)]
        public void RailroadRent_ByCount_ReturnsExpected(int count, int expected)
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out _);

            var owner = Guid.NewGuid();
            var rrIdx = new[] { 5, 15, 25, 35 };

            // Tạo tối đa 4 RR, gán OwnerId cho 'count' cái đầu
            for (int i = 0; i < 4; i++)
            {
                var rr = new RailroadTile(rrIdx[i], $"RR{i+1}", price: 200);
                if (i < count) rr.OwnerId = owner;
                board.Tiles[rrIdx[i]] = rr;
            }

            // Tính rent trên 1 ô đang owned
            var target = (RailroadTile)board.Tiles[rrIdx[0]];
            var strat  = new RailRoadRentStrategy();
            var rent   = strat.CalculateRent(ctx, target, lastDiceSum: 0);

            Assert.Equal(expected, rent);
        }

        // -----------------------
        // Utility: dice 7 → 28 (1 utility) / 70 (2 utilities)
        // -----------------------
        [Fact]
        public void UtilityRent_SingleUtility_Dice7_Returns28()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out _);

            var owner = Guid.NewGuid();

            var u1 = new UtilityTile(12, "Electric Company", price: 150) { OwnerId = owner };
            var u2 = new UtilityTile(28, "Water Works",      price: 150); // chưa sở hữu

            board.Tiles[12] = u1;
            board.Tiles[28] = u2;

            var strat = new UtilityRentStrategy();
            var rent  = strat.CalculateRent(ctx, u1, lastDiceSum: 7);

            Assert.Equal(28, rent); // 7 * 4
        }

        [Fact]
        public void UtilityRent_BothUtilities_Dice7_Returns70()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out _);

            var owner = Guid.NewGuid();

            var u1 = new UtilityTile(12, "Electric Company", price: 150) { OwnerId = owner };
            var u2 = new UtilityTile(28, "Water Works",      price: 150) { OwnerId = owner };

            board.Tiles[12] = u1;
            board.Tiles[28] = u2;

            var strat = new UtilityRentStrategy();
            var rent  = strat.CalculateRent(ctx, u1, lastDiceSum: 7);

            Assert.Equal(70, rent); // 7 * 10
        }

        // -----------------------
        // OnLand: Unowned → LandUnownedProperty ; Owned → RentDue đúng số tiền
        // -----------------------
        [Fact]
        public void OnLand_Unowned_Raises_LandUnownedProperty()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out var bus);

            var p = new PropertyTile(1, "Med Ave", PropertyColor.LightBlue, price: 100, baseRent: 6);
            board.Tiles[1] = p;

            var player = new Player("A");
            p.OnLand(ctx, player, lastDiceSum: 0);

            var evts = bus.DequeueAll().ToArray();
            Assert.Single(evts);
            var e = Assert.IsType<LandUnownedProperty>(evts[0]);
            Assert.Equal(p.TileId, e.TileId);
            Assert.Equal(p.Price,  e.Price);
        }

        [Fact]
        public void OnLand_OwnedByOther_Raises_RentDue_WithCorrectAmount()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out var bus);

            var ownerId = Guid.NewGuid();
            var p = new PropertyTile(1, "Med Ave", PropertyColor.LightBlue, price: 100, baseRent: 6) { OwnerId = ownerId };
            board.Tiles[1] = p;

            var player = new Player("A"); // player.Id khác ownerId
            p.OnLand(ctx, player, lastDiceSum: 0);

            var evts = bus.DequeueAll().ToArray();
            Assert.Single(evts);
            var e = Assert.IsType<RentDue>(evts[0]);
            Assert.Equal(player.Id, e.PlayerId);
            Assert.Equal(ownerId,   e.OwnerId);
            Assert.Equal(6,         e.Amount); // no monopoly → base rent
            Assert.Equal(p.TileId,  e.TileId);
        }

        [Fact]
        public void OnLand_Owned_Railroad_RentByCount()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out var bus);

            var owner = Guid.NewGuid();

            var rr1 = new RailroadTile(5,  "RR1", 200) { OwnerId = owner };
            var rr2 = new RailroadTile(15, "RR2", 200) { OwnerId = owner };
            board.Tiles[5]  = rr1;
            board.Tiles[15] = rr2;

            var player = new Player("B");
            rr1.OnLand(ctx, player, lastDiceSum: 0);

            var e = Assert.IsType<RentDue>(Assert.Single(bus.DequeueAll()));
            Assert.Equal(50, e.Amount); // owner có 2 RR → 50
        }

        [Fact]
        public void OnLand_Owned_Utility_UsesDiceSum()
        {
            var board = NewBoard();
            var ctx   = NewCtx(board, out var bus);

            var owner = Guid.NewGuid();
            var u1 = new UtilityTile(12, "Electric", 150) { OwnerId = owner };
            var u2 = new UtilityTile(28, "Water",    150) { OwnerId = owner };
            board.Tiles[12] = u1;
            board.Tiles[28] = u2;

            var player = new Player("C");
            u1.OnLand(ctx, player, lastDiceSum: 7);

            var e = Assert.IsType<RentDue>(Assert.Single(bus.DequeueAll()));
            Assert.Equal(70, e.Amount); // both utilities → 7 * 10
        }
    }
}
