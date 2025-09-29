🏗 Giải thích Kiến trúc Dự Án (bản có “Câu hỏi & Cách giải quyết”)
1) Solution & Root

Monopoly.sln

- Câu hỏi: Build/test đồng bộ tất cả project, quản lý phụ thuộc (Domain → Application → UI) thế nào?

- Giải quyết: Dùng 1 solution trung tâm để CI/CD gọi dotnet build/test ở .sln. Mọi project con tham chiếu qua .csproj, tránh chạy rời rạc.

README.md

- Câu hỏi: Người mới vào repo bắt đầu từ đâu? Chuẩn lệnh/nhánh/CI ở đâu?

- Giải quyết: Đặt setup, lệnh chuẩn, quy ước Git/CI, roadmap ngay README để on-boarding nhanh, tránh mỗi người một kiểu.

TEST_PLAN.md

- Câu hỏi: Mốc (0→6) hoàn thành cái gì, test gì, Done là gì để không tranh cãi?

- Giải quyết: Neo checklist mốc + DoD. Mọi PR bám test/DoD đã công bố.

2) src/ (Code production)
📂 Monopoly.Domain — Business rules & state cốt lõi

Nguyên tắc xuyên suốt: Sự kiện Domain chỉ là tạm thời (ephemeral). Domain Publish → Application Drain ngay trong Use Case → không persist event vào Repository/Snapshot.

Core/

Player.cs

- Câu hỏi: Ai kiểm soát thay đổi tiền/vị trí để không rò rỉ luật sang UI/Application?

- Giải quyết: Đóng gói hành vi hợp lệ (move, pay/receive, đổi IPlayerState) trong Player. Thuộc tính nhạy cảm không set trực tiếp từ ngoài.

Board.cs

- Câu hỏi: Wrap-around (qua GO), tra cứu ô, kích thước board ở đâu?

- Giải quyết: Board giữ kích thước & API tra cứu để TurnManager chỉ điều phối, không ôm data.

IDice/Dice.cs

- Câu hỏi: Test tái lập kết quả xúc xắc (không phụ thuộc RNG thật) thế nào?

- Giải quyết: Tiêm IDice; test dùng fake dice. Domain không cột chặt vào Random.

GameContext.cs

- Câu hỏi: Gom “nguồn sự thật” của ván (players, turn index, deck…) ở đâu để snapshot/restore?

- Giải quyết: GameContext gói toàn bộ state; thiết kế có versioning để mở rộng schema an toàn.

TurnManager.cs

Câu hỏi: Ai điều phối flow roll → move → hậu quả và phát sự kiện Domain theo thứ tự nghiệp vụ?

Giải quyết: TurnManager là điểm điều phối duy nhất. Khi Player di chuyển, Publish PlayerMoved (và các event khác nếu có).

Events/

IDomainEvent

Câu hỏi: Mọi event cần tối thiểu thông tin gì cho phần khác phản ứng?

Giải quyết: Chuẩn hóa OccurredAt (UTC); tùy chọn thêm CorrelationId để trace theo một hành động.

PlayerMoved (+ tương lai: MoneyChanged, EnteredJail, PassedGo...)

Câu hỏi: Làm sao UI/Logger/Rule-engine biết “điều đã xảy ra” mà không dính nội bộ domain?

Giải quyết: Event Domain mô tả fact theo thời gian; bên ngoài chỉ subscribe/consume.

IDomainEventBus / InMemoryDomainEventBus

Câu hỏi (cốt lõi): “Làm sao để khi Player di chuyển, dữ liệu sự kiện biến mất, không lưu?”

Giải quyết: Dùng bus tạm thời trong bộ nhớ:

Publish(IDomainEvent @event) để domain bắn sự kiện.

DequeueAll() để lấy hết & xóa sạch sau khi Use Case kết thúc (Application chịu trách nhiệm gọi).

Tùy chọn Capacity/TTL để phát hiện “quên drain”.

Kết quả: Event ephemeral—tồn tại trong phạm vi 1 Use Case, không serialize vào snapshot, không lưu DB.

Abstractions/

Tile.cs (abstract)

Câu hỏi: Tránh if/else khổng lồ khi “đáp xuống ô” thế nào?

Giải quyết: Đa hình theo loại ô; TurnManager gọi chung, chi tiết hành vi nằm trong lớp con của Tile.

Card.cs (abstract)

Câu hỏi: Áp dụng thẻ Chance/CommunityChest sao cho không rối luồng?

Giải quyết: Lớp con Card định nghĩa hành vi; có thể Publish các DomainEvent để Application xử lý.

Factory/

TileFactory.cs

Câu hỏi: Thay map/board bằng data (JSON) mà không sửa Domain code thế nào?

Giải quyết: TileFactory đọc JSON → sinh Tile đúng loại; Validate cấu hình tại đây/Infra.

Strategy/

IRentStrategy.cs (+ implementations)

Câu hỏi: Tính tiền thuê (nhà/khách sạn/railroad/utility) linh hoạt ra sao?

Giải quyết: Chiến lược rời rạc, thay “house rules” không đụng core.

State/

IPlayerState, NormalState, InJailState, BankruptState

- Câu hỏi: Tránh if/else theo trạng thái khắp nơi thế nào?

- Giải quyết: State pattern; mỗi state quyết định hành vi hợp lệ, có thể Publish event khi chuyển trạng thái.

Domain tổng kết: Luật nằm trọn Domain; event là tạm thời nhờ DequeueAll() → xử lý xong biến mất, không persist.

📂 Monopoly.Application — UseCases + Ports (Orchestration)
Ports/

IGameRepository

Câu hỏi: Lưu/khôi phục state mà không kéo event vào snapshot?

Giải quyết: Chỉ lưu GameContext (state thuần); event queue không nằm trong snapshot. Có thể đổi sang DB/Kafka mà không đổi Domain.

IUiEventBus

Câu hỏi: Đẩy thông điệp sang UI sao cho UI không phụ thuộc Domain model?

Giải quyết: Tách DomainEvent (fact nghiệp vụ) và UiEvent (trình bày). Mapping đặt ở Application.

DTO & Mappers/

PlayerVM, BoardVM, TileVM, ToastVM / DomainToVm.cs

Câu hỏi: Tránh UI “động vào” entity Domain thế nào?

Giải quyết: ViewModel dành riêng cho UI; toàn bộ mapping đặt trong một nơi (DomainToVm).

UseCases/

NewGameUseCase

Câu hỏi: Chuẩn hóa khởi tạo ván & snapshot ban đầu ở đâu?

Giải quyết: Orchestrate tạo GameContext, lưu qua IGameRepository. Không có event tồn dư.

RollDiceUseCase

Câu hỏi (ephemeral events): Drain event lúc nào để event biến mất?

Giải quyết: Sau khi gọi Domain (TurnManager), Application bắt buộc gọi DomainEventBus.DequeueAll() → map sang IUiEventBus → kết thúc Use Case, bus trống.

EndTurnUseCase

Câu hỏi: Kết thúc lượt có phát sinh event (vd: TurnEnded), drain ra sao?

Giải quyết: Nếu có event, drain ngay trong Use Case; đảm bảo không còn event treo khi return.

Application tổng kết: Ứng dụng là điểm kết vòng đời sự kiện Domain trong một hành động. Nơi chịu trách nhiệm drain để event không bị lưu.

📂 Monopoly.Infrastructure.Json — Adapter cho IGameRepository

BoardConfig.cs, JsonSettings.cs, JsonGameRepository.cs

Câu hỏi: Persist/restore state mà không serialize event queue?

Giải quyết: Lưu chỉ GameContext. Vấn đề schema: dùng versioning để không phá file cũ.

Lưu ý: Tuyệt đối không tham chiếu IDomainEventBus trong Repo/Snapshot.

📂 Monopoly.UI.Unity — Hiển thị & Input

AppBootstrap.cs

Câu hỏi: Khởi động app, gọi NewGame, đăng ký bus UI ở đâu cho rõ ràng?

Giải quyết: Bootstrap tại một chỗ; UI không tạo state domain thủ công.

GameController.cs

Câu hỏi: Xử lý input (Roll/EndTurn) mà không đụng luật?

Giải quyết: Chỉ gọi UseCases; nhận UiEvent/ViewModel để render. Không gọi thẳng Domain entity.

UiEventBus.cs

Câu hỏi: Nhận UiEvent (toast, animation) theo thứ tự và không mất sự kiện?

Giải quyết: Bus UI FIFO; nếu async, dùng hàng đợi tạm ở UI. Không kéo DomainEvent vào UI.

3) tests/ (Unit + Integration)

Monopoly.Domain.Tests/

Câu hỏi: Khóa hành vi core (di chuyển, thứ tự event, state chuyển đổi) thế nào?

Giải quyết: Fake IDice, kiểm tra TurnManager publish đúng event/thứ tự; kiểm tra bus FIFO và sau khi drain thì trống.

Monopoly.Application.Tests/

Câu hỏi: UseCase có drain event đúng lúc? Thứ tự Domain → Repo → UI đúng chưa?

Giải quyết: Test orchestration: Domain publish → Application drain → map UI; snapshot không chứa event.

Monopoly.Integration.Tests/

Câu hỏi: Kịch bản e2e ổn định sau refactor?

Giải quyết: Kịch bản roll → land → rent → ui toast; đảm bảo không rơi event, không persist event.

4) CI/CD

.github/workflows/ci.yml

Câu hỏi: Ngăn merge khi build/test đỏ? Debug nhanh khi fail?

Giải quyết: Pipeline bắt buộc build + test; upload log khi fail. Tùy chọn: matrix Windows/Linux để bắt CRLF/đường dẫn.