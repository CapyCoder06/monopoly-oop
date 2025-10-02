using System;
using Monopoly.Domain.Abstractions;
using Monopoly.Domain.Core;
using Monopoly.Domain.Events;
using Monopoly.Domain.Tiles;

namespace Monopoly.Domain.Cards
{
    public enum MoveMode { Absolute, Relative, Nearest }
    public enum NearestKind { Railroad, Utility, Chance, Community, Tax }

    public sealed class MoveCard : Card
    {
        public MoveMode Mode { get; }
        public int? Index { get; }          // dùng khi Absolute
        public int? Offset { get; }         // dùng khi Relative
        public NearestKind? Kind { get; }   // dùng khi Nearest
        public bool WithGoBonusPolicy { get; }   // có cộng 200$ khi vượt GO không
        public bool SkipOnLandIfJail { get; }    // “Go to Jail” thì không gọi OnLand

        public MoveCard(
            string title,
            string description,
            MoveMode mode,
            int? index = null,
            int? offset = null,
            NearestKind? kind = null,
            bool withGoBonusPolicy = true,
            bool skipOnLandIfJail = true
        ) : base(title, description)
        {
            Mode = mode;
            Index = index;
            Offset = offset;
            Kind = kind;
            WithGoBonusPolicy = withGoBonusPolicy;
            SkipOnLandIfJail = skipOnLandIfJail;

            // Validate tham số theo mode để tránh null/runtime bug
            switch (Mode)
            {
                case MoveMode.Absolute:
                    if (Index is null) throw new ArgumentNullException(nameof(index), "Absolute mode requires index.");
                    break;
                case MoveMode.Relative:
                    if (Offset is null) throw new ArgumentNullException(nameof(offset), "Relative mode requires offset.");
                    break;
                case MoveMode.Nearest:
                    if (Kind is null) throw new ArgumentNullException(nameof(kind), "Nearest mode requires kind.");
                    break;
            }
        }

        public override void Resolve(GameContext ctx, IDomainEventBus bus, Player player)
        {
            if (ctx?.Board?.Tiles == null || ctx.Board.Tiles.Count == 0)
                throw new InvalidOperationException("Board is not initialized.");

            int n = ctx.Board.Tiles.Count;
            int targetIndex = player.Position;

            switch (Mode)
            {
                case MoveMode.Absolute:
                    targetIndex = Normalize(Index!.Value, n);
                    break;

                case MoveMode.Relative:
                    targetIndex = Normalize(player.Position + Offset!.Value, n);
                    break;

                case MoveMode.Nearest:
                    targetIndex = ctx.FindNearest(tile => Kind!.Value switch
                    {
                        NearestKind.Railroad  => tile is RailroadTile,
                        NearestKind.Utility   => tile is UtilityTile,
                        _ => false
                    }, player.Position);
                    break;
            }

            // Di chuyển (tính PassedGO + bonus, publish MovedByCard)
            var (from, to, passedGo, bonus) = ctx.ApplyMoveCard(
                player,
                targetIndex,
                withGoBonusPolicy: WithGoBonusPolicy,
                goBonusAmount: 200,
                reason: Title
            );

            // Nếu là “Go to Jail” → không gọi OnLand jail (tuỳ rule bạn thiết kế)
            var dest = ctx.Board.Tiles[to];
            if (SkipOnLandIfJail && dest is JailTile) return;

            // Gọi OnLand ô đích (lastDiceSum=0 vì di chuyển bởi card)
            dest.OnLand(ctx, player, lastDiceSum: 0);
        }

        private static int Normalize(int idx, int size)
        {
            int m = idx % size;
            if (m < 0) m += size;
            return m;
        }
    }
}
