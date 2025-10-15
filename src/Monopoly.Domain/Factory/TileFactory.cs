// Domain/Factory/TileFactory.cs
using System.Text.Json;
using System.Text.Json.Serialization;
using Monopoly.Domain.Abstractions; // Tile (base)
using Monopoly.Domain.Core;         // Board, Deck<Card>
using Monopoly.Domain.Tiles;        // GoTile, PropertyTile, RailroadTile, UtilityTile, JailTile, GoToJailTile, ChanceTile, QuestionTile, FreeParkingTile, TaxTile

namespace Monopoly.Domain.Factory
{
    public static class TileFactory
    {
        // =======================
        // JSON DTOs
        // =======================
        private sealed class BoardConfigDto
        {
            [JsonPropertyName("size")]  public int Size { get; set; }
            [JsonPropertyName("tiles")] public List<TileDto> Tiles { get; set; } = new();
        }

        private sealed class TileDto
        {
            [JsonPropertyName("index")]    public int Index { get; set; }
            [JsonPropertyName("kind")]     public string Kind { get; set; } = "";
            [JsonPropertyName("name")]     public string? Name { get; set; }

            // Property
            [JsonPropertyName("color")]    public string? Color { get; set; }
            [JsonPropertyName("price")]    public int? Price { get; set; }
            [JsonPropertyName("baseRent")] public int? BaseRent { get; set; }

            // Tax
            [JsonPropertyName("amount")]   public int? Amount { get; set; }

            // GoToJail
            [JsonPropertyName("target")]   public int? Target { get; set; }
        }

        // =======================
        // Exception
        // =======================
        public sealed class ValidationException : Exception
        {
            public ValidationException(string msg) : base(msg) { }
        }

        // =========================================================
        // API: Đọc JSON với deck → tạo đủ 40 ô (Chance & CommunityChest hoạt động)
        // =========================================================
        public static Board LoadFromJson(
            string jsonPath,
            Deck<Card> chanceDeck,
            Deck<Card> communityChestDeck,
            bool requireFullBoard = true)
        {
            var cfg = ParseConfig(jsonPath);
            BasicValidate(cfg);

            var tiles = new Tile[cfg.Size];

            foreach (var t in cfg.Tiles)
            {
                var kind = (t.Kind ?? "").Trim();
                var name = t.Name ?? kind;

                Tile created = kind switch
                {
                    // Core
                    "Go"            => new GoTile(t.Index, name),
                    "Jail"          => new JailTile(t.Index, name),
                    "GoToJail"      => new GoToJailTile(t.Index, Require(t.Target, "target", kind, t.Index), name),
                    "FreeParking"   => new FreeParkingTile(t.Index, name),
                    "Tax"           => new TaxTile(t.Index, name, Require(t.Amount, "amount", kind, t.Index)),

                    // Buyables
                    "Property"      => CreateProperty(t, name),
                    "Railroad"      => CreateRailroad(t, name),
                    "Utility"       => CreateUtility(t, name),

                    // Cards
                    "Chance"        => new ChanceTile(t.Index, chanceDeck, name),
                    "CommunityChest"=> new QuestionTile(t.Index, communityChestDeck, name),
                    // Alias tương thích JSON cũ:
                    "Question"      => new QuestionTile(t.Index, communityChestDeck, name),

                    _ => throw new ValidationException($"Unknown kind '{kind}' at index {t.Index}")
                };

                if (tiles[t.Index] != null)
                    throw new ValidationException($"Duplicate tile at index {t.Index}");

                tiles[t.Index] = created;
            }

            PostValidate(cfg, tiles, requireFullBoard);
            return new Board(tiles.ToList());
        }

        // =======================
        // Helpers
        // =======================
        private static BoardConfigDto ParseConfig(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                throw new FileNotFoundException($"Board config not found: {jsonPath}");

            var json = File.ReadAllText(jsonPath);
            var cfg = JsonSerializer.Deserialize<BoardConfigDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return cfg ?? throw new ValidationException("Invalid JSON: cannot parse BoardConfig");
        }

        private static void BasicValidate(BoardConfigDto cfg)
        {
            if (cfg.Size <= 0) throw new ValidationException("size must be > 0");

            // index in range + unique
            var seen = new HashSet<int>();
            foreach (var t in cfg.Tiles)
            {
                if (t.Index < 0 || t.Index >= cfg.Size)
                    throw new ValidationException($"index {t.Index} out of range [0..{cfg.Size - 1}]");
                if (!seen.Add(t.Index))
                    throw new ValidationException($"duplicate index {t.Index}");
            }

            // Exactly 1 Go / Jail / GoToJail
            int goCnt   = cfg.Tiles.Count(x => x.Kind.Equals("Go", StringComparison.OrdinalIgnoreCase));
            int jailCnt = cfg.Tiles.Count(x => x.Kind.Equals("Jail", StringComparison.OrdinalIgnoreCase));
            int g2jCnt  = cfg.Tiles.Count(x => x.Kind.Equals("GoToJail", StringComparison.OrdinalIgnoreCase));
            if (goCnt != 1)   throw new ValidationException($"Go tiles = {goCnt}, expected 1");
            if (jailCnt != 1) throw new ValidationException($"Jail tiles = {jailCnt}, expected 1");
            if (g2jCnt != 1)  throw new ValidationException($"GoToJail tiles = {g2jCnt}, expected 1");

            // Property fields
            foreach (var p in cfg.Tiles.Where(x => x.Kind.Equals("Property", StringComparison.OrdinalIgnoreCase)))
            {
                if (string.IsNullOrWhiteSpace(p.Color))
                    throw new ValidationException($"Property at index {p.Index} missing 'color'");

                if (!ColorGroupExtensions.TryParse(p.Color, out _))
                    throw new ValidationException($"Property at index {p.Index} invalid color '{p.Color}'");

                if (p.Price    is null || p.Price    <= 0)
                    throw new ValidationException($"Property at index {p.Index} must have positive 'price'");
                if (p.BaseRent is null || p.BaseRent <  0)
                    throw new ValidationException($"Property at index {p.Index} must have non-negative 'baseRent'");
            }

            // Tax / Railroad / Utility minimal fields
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
            // Đủ 0..size-1
            if (requireFullBoard)
            {
                for (int i = 0; i < cfg.Size; i++)
                    if (tiles[i] is null)
                        throw new ValidationException($"Missing tile for index {i}");
            }

            // GoToJail.target == Jail.index
            var jailIdx = cfg.Tiles.FirstOrDefault(x => x.Kind.Equals("Jail", StringComparison.OrdinalIgnoreCase))?.Index;
            var g2j     = cfg.Tiles.FirstOrDefault(x => x.Kind.Equals("GoToJail", StringComparison.OrdinalIgnoreCase));
            if (jailIdx is int jIdx && g2j?.Target is int tgt && tgt != jIdx)
                throw new ValidationException($"GoToJail.target ({tgt}) must equal Jail.index ({jIdx})");
        }

        private static int Require(int? v, string field, string kind, int idx)
            => v ?? throw new ValidationException($"{kind} at index {idx} missing required field '{field}'");

        private static PropertyTile CreateProperty(TileDto t, string name)
        {
            if (!ColorGroupExtensions.TryParse(t.Color, out var g))
                throw new ValidationException($"Property[{t.Index}] invalid color '{t.Color}'");

            var color    = g.ToPropertyColor();
            var price    = Require(t.Price,    "price",    "Property", t.Index);
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
        public static Board CreateFromConfig()
        {
            // Đường dẫn tới file cấu hình trong thư mục Data
            string configPath = Path.Combine(AppContext.BaseDirectory, "Data", "BoardConfig.json");

            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Board config file not found at: {configPath}");

            // Hai bộ bài rỗng (nếu sau này bạn muốn nạp thêm Card, có thể thay đổi)
            var emptyChanceDeck = new Deck<Card>(new List<Card>());
            var emptyCommunityDeck = new Deck<Card>(new List<Card>());

            // Tạo Board từ file JSON
            return LoadFromJson(configPath, emptyChanceDeck, emptyCommunityDeck, requireFullBoard: true);
        }
    }
}
