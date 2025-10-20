using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Monopoly.Domain.Abstractions; 
using Monopoly.Domain.Core;         
using Monopoly.Domain.Tiles;        

namespace Monopoly.Domain.Factory
{
    public static class TileFactory
    {
        private sealed class BoardConfigDto
        {
            [JsonPropertyName("size")] public int Size { get; set; }
            [JsonPropertyName("tiles")] public List<TileDto> Tiles { get; set; } = new();
        }

        private sealed class TileDto
        {
            [JsonPropertyName("index")] public int Index { get; set; }
            [JsonPropertyName("kind")] public string Kind { get; set; } = "";
            [JsonPropertyName("name")] public string? Name { get; set; }

            [JsonPropertyName("color")] public string? Color { get; set; }
            [JsonPropertyName("price")] public int? Price { get; set; }
            [JsonPropertyName("baseRent")] public int? BaseRent { get; set; }

            [JsonPropertyName("amount")] public int? Amount { get; set; }
            [JsonPropertyName("target")] public int? Target { get; set; }
        }

        public sealed class ValidationException : Exception
        {
            public ValidationException(string msg) : base(msg) { }
        }

        public static Board LoadFromJsonString(string jsonContent, Deck<Card> chanceDeck, Deck<Card> communityDeck, bool requireFullBoard = true)
        {
            var cfg = JsonSerializer.Deserialize<BoardConfigDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new ValidationException("Invalid JSON: cannot parse BoardConfig");

            return LoadFromJson(cfg, chanceDeck, communityDeck, requireFullBoard);
        }

        public static Board LoadFromJson(string path, Deck<Card> chanceDeck, Deck<Card> communityDeck, bool requireFullBoard = true)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Board config not found: {path}");

            var json = File.ReadAllText(path);
            return LoadFromJsonString(json, chanceDeck, communityDeck, requireFullBoard);
        }

        private static Board LoadFromJson(BoardConfigDto cfg, Deck<Card> chanceDeck, Deck<Card> communityDeck, bool requireFullBoard)
        {
            BasicValidate(cfg);

            var tiles = new Tile[cfg.Size];

            foreach (var t in cfg.Tiles)
            {
                var kind = (t.Kind ?? "").Trim();
                var name = t.Name ?? kind;

                Tile created = kind switch
                {
                    "Go" => new GoTile(t.Index, name),
                    "Jail" => new JailTile(t.Index, name),
                    "GoToJail" => new GoToJailTile(t.Index, Require(t.Target, "target", kind, t.Index), name),
                    "FreeParking" => new FreeParkingTile(t.Index, name),
                    "Tax" => new TaxTile(t.Index, name, Require(t.Amount, "amount", kind, t.Index)),
                    "Property" => CreateProperty(t, name),
                    "Railroad" => CreateRailroad(t, name),
                    "Utility" => CreateUtility(t, name),
                    "Chance" => new ChanceTile(t.Index, chanceDeck, name),
                    "CommunityChest" => new QuestionTile(t.Index, communityDeck, name),
                    "Question" => new QuestionTile(t.Index, communityDeck, name),
                    _ => throw new ValidationException($"Unknown kind '{kind}' at index {t.Index}")
                };

                if (tiles[t.Index] != null)
                    throw new ValidationException($"Duplicate tile at index {t.Index}");

                tiles[t.Index] = created;
            }

            PostValidate(cfg, tiles, requireFullBoard);
            return new Board(tiles.ToList());
        }

        private static void BasicValidate(BoardConfigDto cfg)
        {
            if (cfg.Size <= 0) throw new ValidationException("size must be > 0");

            var seen = new HashSet<int>();
            foreach (var t in cfg.Tiles)
            {
                if (t.Index < 0 || t.Index >= cfg.Size)
                    throw new ValidationException($"index {t.Index} out of range [0..{cfg.Size - 1}]");
                if (!seen.Add(t.Index))
                    throw new ValidationException($"duplicate index {t.Index}");
            }

            int goCnt = cfg.Tiles.Count(x => x.Kind.Equals("Go", StringComparison.OrdinalIgnoreCase));
            int jailCnt = cfg.Tiles.Count(x => x.Kind.Equals("Jail", StringComparison.OrdinalIgnoreCase));
            int g2jCnt = cfg.Tiles.Count(x => x.Kind.Equals("GoToJail", StringComparison.OrdinalIgnoreCase));
            if (goCnt != 1) throw new ValidationException($"Go tiles = {goCnt}, expected 1");
            if (jailCnt != 1) throw new ValidationException($"Jail tiles = {jailCnt}, expected 1");
            if (g2jCnt != 1) throw new ValidationException($"GoToJail tiles = {g2jCnt}, expected 1");

            foreach (var p in cfg.Tiles.Where(x => x.Kind.Equals("Property", StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrWhiteSpace(p.Color))
                    throw new ValidationException($"Property at index {p.Index} missing 'color'");
                if (!ColorGroupExtensions.TryParse(p.Color, out _))
                    throw new ValidationException($"Property at index {p.Index} invalid color '{p.Color}'");
                if (p.Price is null || p.Price <= 0)
                    throw new ValidationException($"Property at index {p.Index} must have positive 'price'");
                if (p.BaseRent is null || p.BaseRent < 0)
                    throw new ValidationException($"Property at index {p.Index} must have non-negative 'baseRent'");
            }

            foreach (var tax in cfg.Tiles.Where(x => x.Kind.Equals("Tax", StringComparison.OrdinalIgnoreCase)))
                if (tax.Amount is null || tax.Amount < 0)
                    throw new ValidationException($"Tax at index {tax.Index} must have non-negative 'amount'");
            foreach (var rr in cfg.Tiles.Where(x => x.Kind.Equals("Railroad", StringComparison.OrdinalIgnoreCase)))
                if (rr.Price is null || rr.Price <= 0)
                    throw new ValidationException($"Railroad at index {rr.Index} must have positive 'price'");
            foreach (var ut in cfg.Tiles.Where(x => x.Kind.Equals("Utility", StringComparison.OrdinalIgnoreCase)))
                if (ut.Price is null || ut.Price <= 0)
                    throw new ValidationException($"Utility at index {ut.Index} must have positive 'price'");
        }

        private static void PostValidate(BoardConfigDto cfg, Tile[] tiles, bool requireFullBoard)
        {
            if (requireFullBoard)
            {
                for (int i = 0; i < cfg.Size; i++)
                    if (tiles[i] is null)
                        throw new ValidationException($"Missing tile for index {i}");
            }

            var jailIdx = cfg.Tiles.FirstOrDefault(x => x.Kind.Equals("Jail", StringComparison.OrdinalIgnoreCase))?.Index;
            var g2j = cfg.Tiles.FirstOrDefault(x => x.Kind.Equals("GoToJail", StringComparison.OrdinalIgnoreCase));
            if (jailIdx is int jIdx && g2j?.Target is int tgt && tgt != jIdx)
                throw new ValidationException($"GoToJail.target ({tgt}) must equal Jail.index ({jIdx})");
        }

        private static int Require(int? v, string field, string kind, int idx)
            => v ?? throw new ValidationException($"{kind} at index {idx} missing required field '{field}'");

        private static PropertyTile CreateProperty(TileDto t, string name)
        {
            if (!ColorGroupExtensions.TryParse(t.Color, out var g))
                throw new ValidationException($"Property[{t.Index}] invalid color '{t.Color}'");

            var color = g.ToPropertyColor();
            var price = Require(t.Price, "price", "Property", t.Index);
            var baseRent = Require(t.BaseRent, "baseRent", "Property", t.Index);

            return new PropertyTile(t.Index, name, color, price, baseRent);
        }

        private static RailroadTile CreateRailroad(TileDto t, string name)
        {
            var price = Require(t.Price, "price", "Railroad", t.Index);
            return new RailroadTile(t.Index, name, price);
        }

        private static UtilityTile CreateUtility(TileDto t, string name)
        {
            var price = Require(t.Price, "price", "Utility", t.Index);
            return new UtilityTile(t.Index, name, price);
        }
        public static Board LoadFromJson()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Configs", "board.json");
            var chanceDeck = new Deck<Card>(new List<Card>());
            var communityDeck = new Deck<Card>(new List<Card>());
            return LoadFromJson(path, chanceDeck, communityDeck);
        }
    }
}
