/*
// tests/Monopoly.Domain.Tests/Moc4.cs
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

// Domain
using Monopoly.Domain.Abstractions;   // Card (base)
using Monopoly.Domain.Core;           // Board, GameContext, Deck, InMemoryWallet, Player
using Monopoly.Domain.Cards;          // PayCard, ReceiveCard, MoveCard, GetOutOfJailCard
using Monopoly.Domain.Events;         // CardResolved, MovedByCard, GotOutOfJailCardGranted, FundsChanged
using Monopoly.Domain.Tiles;          // Tile, ChanceTile, QuestionTile

public class Moc4
{
    // Helper: tạo GameContext nhanh
    private static GameContext MakeCtx(Board board)
    {
        var bus = new InMemoryDomainEventBus();
        var wallet = new InMemoryWallet(bus);
        return new GameContext(board, bus, wallet);
    }

    // Helper: tạo Board đúng ctor (Board(List<Tile>))
    private static Board MakeBoard(params Tile[] tiles)
        => new Board(new List<Tile>(tiles));

    // ─────────────────────────────────────────────────────────────────────
    // 1) RÚT TUẦN TỰ — Deck xoay vòng: 1,2,1,2
    // Chance Deck: [Receive 50], [Pay 15]
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void Chance_Deck_Rotates_Sequential_Draws()
    {
        // Arrange
        var c1 = new ReceiveCard("Receive 50", "Bank pays you dividend of $50", 50);
        var c2 = new PayCard("Pay 15", "Pay poor tax of $15", 15);

        var chanceDeck = new Deck<Card>(new Card[] { c1, c2 });
        var chance = new ChanceTile(index: 0, deck: chanceDeck, name: "Chance");
        var board  = MakeBoard(chance);

        var ctx = MakeCtx(board);
        var p   = new Player("P1") { Position = 0 };

        // Act: đứng vào Chance 4 lần
        for (int i = 0; i < 4; i++)
            chance.OnLand(ctx, p, lastDiceSum: 0);

        // Assert: CardResolved theo thứ tự 1,2,1,2
        var resolvedTitles = ctx.Bus.DequeueAll()
            .OfType<CardResolved>()
            .Select(e => e.Title)
            .ToArray();

        Assert.Equal(new[] { "Receive 50", "Pay 15", "Receive 50", "Pay 15" }, resolvedTitles);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2) MOVECARD TRIGGER ONLAND — Move tới Question rồi Question rút tiếp
    // Chance Deck: [Move to index 1]
    // Question Deck (index 1): [Pay 10]
    // Kỳ vọng: có MovedByCard(From=0, To=1) và CardResolved cuối cùng = "Question Pay 10"
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void MoveCard_Advances_To_Question_Then_Question_Draws()
    {
        // Arrange
        var questionTop = new PayCard("Question Pay 10", "Pay 10", 10);
        var questionDeck = new Deck<Card>(new Card[] { questionTop });

        var moveToQuestion = new MoveCard(
            "Advance to Question(1)",
            "Go to tile index 1",
            MoveMode.Absolute,
            index: 1,
            withGoBonusPolicy: true
        );
        var chance   = new ChanceTile(index: 0, deck: new Deck<Card>(new Card[] { moveToQuestion }), name: "Chance");
        var question = new QuestionTile(index: 1, deck: questionDeck, name: "Question");

        var board = MakeBoard(chance, question);
        var ctx   = MakeCtx(board);
        var p     = new Player("P1") { Position = 0 };

        // Act: đứng vào Chance → Move → OnLand Question → Question bốc thẻ
        chance.OnLand(ctx, p, lastDiceSum: 0);

        // Assert
        var events = ctx.Bus.DequeueAll().ToList();

        var moved = events.OfType<MovedByCard>().FirstOrDefault();
        Assert.NotNull(moved);
        Assert.Equal(0, moved!.From);
        Assert.Equal(1, moved.To);
        Assert.Equal("Advance to Question(1)", moved.Reason);

        // CardResolved cuối cùng là từ QuestionDeck (Pay 10)
        var lastResolved = events.OfType<CardResolved>().LastOrDefault();
        Assert.NotNull(lastResolved);
        Assert.Equal("Question Pay 10", lastResolved!.Title);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 3) GET OUT OF JAIL — cấp thẻ, không dùng ngay
    // Question Deck: [Get Out of Jail]
    // Kỳ vọng: player.JailCard tăng +1, có event GotOutOfJailCardGranted & CardResolved
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void GetOutOfJail_Grants_Inventory_And_Publishes_Events()
    {
        // Arrange
        var getOut = new GetOutOfJailCard("Get Out of Jail", "Keep until needed");
        var questionDeck = new Deck<Card>(new Card[] { getOut });
        var question = new QuestionTile(index: 0, deck: questionDeck, name: "Question");
        var board = MakeBoard(question);

        var ctx = MakeCtx(board);
        var p   = new Player("P1") { Position = 0 };
        int before = p.JailCard;

        // Act
        question.OnLand(ctx, p, lastDiceSum: 0);

        // Assert: tồn kho tăng
        Assert.Equal(before + 1, p.JailCard);

        var events = ctx.Bus.DequeueAll().ToList();

        Assert.Contains(events, e =>
            e is GotOutOfJailCardGranted g &&
            g.PlayerId == p.Id &&
            g.CountInHand == p.JailCard);

        Assert.Contains(events, e =>
            e is CardResolved r &&
            r.Title == "Get Out of Jail" &&
            r.PlayerId == p.Id);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 4) PASS GO BONUS — đi từ index 1 về 0 → nhận +200
    // Không cần GoTile; chỉ cần logic ApplyMoveCard trong GameContext cộng bonus.
    // ─────────────────────────────────────────────────────────────────────
    [Fact]
    public void MoveCard_Should_Grant_Bonus_When_Passing_GO()
    {
        // Arrange: board 2 ô cho tối giản
        var noop = new QuestionTile(index: 0, deck: new Deck<Card>(Array.Empty<Card>()), name: "Noop-0");
        var noop1 = new QuestionTile(index: 1, deck: new Deck<Card>(Array.Empty<Card>()), name: "Noop-1");

        var board = MakeBoard(noop, noop1);
        var ctx = MakeCtx(board);

        var player = new Player("P1", startingCash: 1500) { Position = 1 }; // từ 1 về 0 → qua GO
        var card = new MoveCard("Advance to GO", "Go to 0", MoveMode.Absolute, index: 0, withGoBonusPolicy: true);

        // Act
        card.Resolve(ctx, ctx.Bus, player);
        var events = ctx.Bus.DequeueAll().ToList();

        // Assert
        Assert.Equal(0, player.Position);
        Assert.Equal(1700, player.Cash); // 1500 + 200 bonus

        Assert.Contains(events, e =>
            e is FundsChanged fc && fc.Amount == 200 && fc.Reason.Contains("Passed GO"));

        Assert.Contains(events, e =>
            e is MovedByCard m && m.PassedGo && m.GoBonus == 200);
    }
}
*/
