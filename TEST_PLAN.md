# TEST_PLAN (7 mốc)

## Mục tiêu chung
- Core Monopoly tuân thủ luật; UI chỉ hiển thị/nhận input, **không chứa luật**.
- Thay đổi Domain **phải có unit test**; thay đổi UI **có GIF demo** trong PR.
- CI: `dotnet build && dotnet test` xanh trước khi merge.

## Phạm vi & môi trường
- **Solution**: `Monopoly.sln`
- **Projects**:
  - `src/Monopoly.Domain` (luật/Entity/Event/Factory)
  - `src/Monopoly.Application` (UseCases/Ports/DTO/Mappers)
  - `src/Monopoly.Infrastructure.Json` (Mốc 5)
  - `src/Monopoly.UI.Unity` (UI chính) hoặc runner `src/Monopoly.ConsoleApp`
- **Test projects**:
  - `tests/Monopoly.Domain.Tests`
  - `tests/Monopoly.Application.Tests` (tạo dần)
  - `tests/Monopoly.Integration.Tests` (khi cần)
- **Lệnh chuẩn**:
  - Build: `dotnet build`
  - Test: `dotnet test --collect:"XPlat Code Coverage"`

---

## Mốc 0 — Kickoff & Contracts
**Mục tiêu**: Chốt API tối thiểu để làm song song.
**Phạm vi**:  
Domain: `Player`, `Board`, `Dice`, `TurnManager`, `Tile (abstract)`, `Card (abstract)`, `GameContext`, `DomainEventBus`  
Events: `PlayerMoved`, `RentPaid`, `LandUnownedProperty`, `WentToJail`  
Application: `IGameRepository`, `IUiEventBus`; UseCases stub `NewGame`, `RollDice`, `EndTurn`  
DTO/VM: `PlayerVM`, `BoardVM`, `TileVM`, `ToastVM`  
**Kiểm thử**:
- [ ] Domain compile; tên/namespace đúng
- [ ] 2–3 unit test khởi tạo + event bus
- [ ] Runner (Console/Unity bootstrap) chạy
**DoD**: CI xanh; README ghi rõ contracts.

---

## Mốc 1 — Walking Skeleton: Roll → Move
**Mục tiêu**: Nhấn Roll → token di chuyển → log event.  
**Phạm vi**:  
Domain: `TurnManager.RollDiceAndAdvance()`, `GoTile`, `PropertyTile (rent=0)`, raise `PlayerMoved`  
Application: `NewGameUseCase`, `RollDiceUseCase`, `EndTurnUseCase`; Mapper `GameContext -> VM`  
UI: Nút `Roll`, listener log sự kiện  
**Tests**:
- [ ] Roll (1..6) → vị trí cập nhật (mod 40)
- [ ] OnLand không crash; `PlayerMoved` phát đi  
- [ ] `RollDiceUseCase` → UI nhận event → animate/log
**DoD**: GIF 3 lần roll; `dotnet test` xanh.

---

## Mốc 2 — Rent Strategy + Mua/Thuê
**Mục tiêu**: Tính rent chuẩn; popup mua/thuê.  
**Phạm vi**:  
Domain: `IRentStrategy` + `Property/Railroad/Utility`  
Application: `BuyPropertyUseCase`  
UI: Unowned → Buy/Pass; Owned → auto trừ rent + breakdown  
**Tests (Domain)**:
- [ ] Property: 0..4 houses + hotel; **x2 monopoly** khi 0 house
- [ ] Railroad: 1..4 tuyến
- [ ] Utility: theo dice (x4/x10)
**Integration**:
- [ ] 4 case: buy / pass / pay rent / owner land
**DoD**: GIF 4 tình huống; bảng rent pass.

---

## Mốc 3 — Jail State + Jail UX
**Mục tiêu**: Vào/ra tù đúng luật; UI khóa thao tác phù hợp.  
**Phạm vi**:  
Domain: `IPlayerState` → `Normal`, `InJail (3 lượt)`, `Bankrupt`; `JailTile`, `GoToJailTile`  
Application: `PayBailUseCase`, `UseCardUseCase`  
UI: Badge “In Jail (n/3)”; nút Pay/UseCard/Roll enable/disable  
**Tests**:
- [ ] Vào tù khi land `GoToJailTile`
- [ ] Ra tù: trả tiền; dùng thẻ; roll đôi
- [ ] Hết 3 lượt không thoát → xử lý theo luật
**DoD**: GIF 3 đường ra tù; tests xanh.

---

## Mốc 4 — Cards Engine + Cards UX
**Mục tiêu**: Rút/giải quyết thẻ; chuỗi async mượt.  
**Phạm vi**:  
Domain: `Deck<TCard>`; `Pay/Receive/Move/GetOutOfJail/RepairHouses`  
Application: `DrawCardUseCase`  
UI: Modal thẻ; `Move` → animate xong mới close; chuỗi **Draw → Resolve → OnLand → (Buy/Rent)**  
**Tests**:
- [ ] Deck rút tuần tự; `MoveCard` trigger OnLand
- [ ] `RepairHouses` tính theo tổng nhà/khách sạn
**DoD**: GIF card flow end-to-end; tests pass.

---

## Mốc 5 — Save/Load + BoardConfig
**Mục tiêu**: Lưu/khôi phục chính xác; đọc bàn từ JSON.  
**Phạm vi**:  
Domain: `TileFactory` đọc `/Config/BoardConfig.json`, validate  
Infrastructure: `JsonGameRepository` (implements `IGameRepository`)  
Application/UI: `SaveGame`, `LoadGame`; nút Save/Load; Settings (tiền khởi điểm, FreeParking toggle)  
**Tests**:
- [ ] Parse file thật; checksum số ô; vị trí Go/Jail đúng  
- [ ] Save giữa chuỗi hành động → Load khôi phục **đúng**: vị trí, tiền, sở hữu, deck, state jail, lượt  
- [ ] File lỗi → báo lỗi rõ
**DoD**: Load khôi phục chuẩn; tests xanh.

---

## Mốc 6 — Đấu giá & Polish
**Mục tiêu**: Auction khi pass không mua; polish UX.  
**Phạm vi**:  
Domain: Auction (raise-bid theo lượt); (tuỳ) Trade  
UI: Modal auction/trade; hiệu ứng di chuyển step-by-step, toast; build WebGL/Player  
**Tests**:
- [ ] Auction nhiều người bid; pass/timeout; winner trừ tiền chính xác
- [ ] Script 30 hành động liên tục không crash
**DoD**: GIF demo auction; README cập nhật.

---

## Quy ước PR & chất lượng
- Nhánh: `feat/<slice>`, `domain/<module>`, `ui/<feature>`
- Tiêu đề PR ví dụ:
  - `domain(rent): implement IRentStrategy + tests`
  - `app(usecase): BuyProperty + mapping`
  - `ui(buy): popup & rent breakdown`
- PR:
  - (Domain) **bắt buộc unit tests**
  - (UI) **bắt buộc GIF**
  - CI: `dotnet build && dotnet test` xanh
- Coverage Domain (phần thay đổi): **≥80%**

---

## Checklist chung mỗi mốc
- [ ] Không logic Domain trong Application/UI
- [ ] Event mapping đầy đủ, không rơi trạng thái
- [ ] UI enable/disable nút đúng state
- [ ] README/ROADMAP cập nhật khi contract đổi
- [ ] GIF demo đính PR

---

## Lệnh nhanh
```bash
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet test tests/Monopoly.Domain.Tests
dotnet run --project src/Monopoly.ConsoleApp
