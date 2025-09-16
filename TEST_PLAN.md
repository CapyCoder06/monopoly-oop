# TEST_PLAN.md — Monopoly OOP (Walking Skeleton → Vertical Slices)

This document lists the **minimum unit tests** to keep the core (Domain) stable while developing UI in parallel.
Follow the milestones (Mốc) and ensure tests are added **before/with** each feature PR.

---

## Conventions
- **Framework:** xUnit (`tests/Monopoly.Domain.Tests`)
- **Naming:** `ClassName_Should_ExpectedBehavior_When_Context()`
- **Pattern:** Arrange → Act → Assert
- **Structure:**
  ```text
  tests/Monopoly.Domain.Tests/
    Core/
    Tiles/
    Strategy/
    State/
    Cards/
    Factory/
    Integration/
  ```

---

## Mốc 0 — Kickoff & Contracts (Smoke tests)
**Goal:** Project compiles; basic behavior works.
- [ ] `Dice_Should_ReturnValues_1To6_When_Roll()`
- [ ] `Player_Should_Receive_200_When_PassGo()`
- [ ] `Board_Should_Contain_GoTile_At_Index0()`

> Tip: Use tiny board with just GO and one property for fast tests.

---

## Mốc 1 — Walking Skeleton (Roll → Move)
**Goal:** Movement and events.
- [ ] `TurnManager_Should_MovePlayer_ByDiceSum()`
- [ ] `TurnManager_Should_Raise_PlayerMoved_On_Move()`
- [ ] `OnLand_Should_NotThrow_When_LandingOn_GoTile()`
- [ ] `OnLand_Should_NotThrow_When_LandingOn_PropertyTile_WithNoOwner()`
- [ ] (Integration) `RollThenEndTurn_Should_Switch_CurrentPlayer()`

**Edge:**
- [ ] `Move_Should_WrapAround_BoardSize()`
- [ ] `PassGo_Should_Add_200_ExactlyOnce_When_Crossing()`

---

## Mốc 2 — Rent Strategy + Mua/Thuê
**Goal:** Correct rent for Property/Railroad/Utility.
### Property
- [ ] `Rent_Property_NoHouse_Doubles_When_Monopoly()`
- [ ] `Rent_Property_ByHouses_0to4_And_Hotel()`
- [ ] `Rent_ShouldBeZero_When_Mortgaged()`

### Railroad
- [ ] `Rent_Railroad_Increases_By_OwnedCount_1to4()`

### Utility
- [ ] `Rent_Utility_UsesDice_4x_When_OneUtilityOwned()`
- [ ] `Rent_Utility_UsesDice_10x_When_BothOwned()`

### Purchase Flow
- [ ] `Buy_Should_AssignOwner_And_DeductCash_When_Sufficient()`
- [ ] `Buy_Should_Decline_When_InsufficientCash()`
- [ ] `Landing_On_Owned_Property_Should_Transfer_Rent_ToOwner()`

---

## Mốc 3 — Jail State + Jail Rules
**Goal:** State machine and release options.
- [ ] `GoToJail_Should_Set_PlayerState_InJail_And_Position()`
- [ ] `InJail_TurnCounter_Increments_EachTurn()`
- [ ] `PayBail_Should_ExitJail_And_DeductCash()`
- [ ] `UseGetOutOfJailCard_Should_ExitJail_And_ConsumeCard()`
- [ ] `RollDoubles_Should_ExitJail_And_Move()`
- [ ] `After3Turns_Should_AutoPay_And_ExitJail()`

**Edge:**
- [ ] `CannotMove_NormalSteps_While_InJail_Until_Released()`

---

## Mốc 4 — Cards Engine + Deck
**Goal:** Polymorphic cards and deck behavior.
- [ ] `Deck_Should_Draw_FromTop_And_PlaceToBottom()`
- [ ] `PayCard_Should_Deduct_From_Player()`
- [ ] `ReceiveCard_Should_Add_To_Player()`
- [ ] `MoveCard_Should_Move_And_Trigger_OnLand()`
- [ ] `RepairHousesCard_Should_Charge_By_TotalHouses_AndHotels()`
- [ ] `GetOutOfJailCard_Should_Add_To_PlayerInventory()`

**Integration:**
- [ ] `ChanceTile_Should_Draw_And_Execute_Card()`

---

## Mốc 5 — BoardConfig + Factory + Save/Load
**Goal:** Load board from config and persist game.
### Factory
- [ ] `TileFactory_Should_Parse_Config_And_Create_Correct_TileCount()`
- [ ] `TileFactory_Should_Set_Go_And_Jail_Indices_Correctly()`
- [ ] `TileFactory_Should_Group_Colors_And_Rents_As_Configured()`

### Save/Load (Application repo tests allowed if needed)
- [ ] `SaveGame_Should_Write_Serializable_State()`
- [ ] `LoadGame_Should_Rehydrate_Same_State()`
- [ ] `LoadGame_DuringTurn_Should_Restore_TurnOrder_And_Positions()`

---

## Mốc 6 — Auction + Trade (Optional/Polish)
- [ ] `Auction_Should_Award_To_HighestBidder_And_DeductCash()`
- [ ] `Auction_Should_Skip_Player_With_InsufficientFunds()`
- [ ] `Trade_Should_Exchange_Cash_And_Deeds_Between_Players()`

---

## Integration Test Scripts (optional but recommended)
Run mini-scenarios with a tiny board:
- [ ] `Scenario_Roll_Move_Buy_Rent_Pay()`
- [ ] `Scenario_Jail_InOut()`
- [ ] `Scenario_Draw_MoveCard_Then_Rent()`

---

## Test Utilities (helpers)
Create small helpers in test project:
```csharp
public static class TestBoard
{
    public static GameContext Tiny(params Player[]? players) { /* build GO + one property */ }
    public static (GameContext ctx, TurnManager tm) NewWithTurn(params string[] names) { /* ... */ }
}
```
Use deterministic dice (injectable RNG) for predictable tests.

---

## Coverage Target
- **Domain:** ≥ **80%** line coverage per milestone section you touch
- **Critical paths:** Rent, Jail transitions, Card execution ≥ **95%**

---

## CI Gates
- Run in GitHub Actions on PR: `dotnet build`, `dotnet test --configuration Release`
- Fail PR if any test fails. Protect `dev` and `main` branches.

---

## Example Test Names
- `PropertyRentStrategy_Should_DoubleBaseRent_When_Monopoly_NoHouses()`
- `UtilityRentStrategy_Should_Use10xDice_When_BothUtilitiesOwned()`
- `InJailState_Should_Exit_On_RollDoubles()`
