# monopoly-oop
# Monopoly OOP (C#/.NET 8)

Monopoly theo kiến trúc **Domain-Driven**:
- **Domain**: luật trò chơi, state thuần
- **Application**: Use Cases/Ports (không chứa luật)
- **Infrastructure**: lưu/khôi phục (JSON ở mốc 5)
- **UI**: Unity (chỉ hiển thị & nhận input)

**CI:** `dotnet build && dotnet test` phải xanh trước khi merge.  
**Test coverage (Domain thay đổi):** ≥ 80%

## Quick Start
```bash
dotnet restore Monopoly.sln
dotnet build   Monopoly.sln
dotnet test    Monopoly.sln --collect:"XPlat Code Coverage"
