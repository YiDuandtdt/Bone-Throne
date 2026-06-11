# Phase 16.12 - Boss Outcome And Presentation Tuning

## Scope

- Fixed `boss_test` demo defeat outcome timing.
- Added delayed boss-test victory evaluation after Boss death.
- Tuned Barbarian, Fighter, and Boss presentation timing without changing skill data assets or formal level scenes.

## Implementation

- `BattleHUDController`
  - Boss-test defeat button now kills all player units, refreshes foot highlights, waits `1.2s`, then triggers defeat.
  - `boss_test` now auto-ensures `GameOutcomeService` and `BattleOutcomeAutoEvaluator` at runtime if missing.

- `BattleOutcomeAutoEvaluator`
  - Added delayed victory handling.
  - Added `ConfigureBossTest` so the test HUD can reliably track players and enemy/Boss units.
  - Victory still waits for a Boss-like enemy when one exists.

- `SkillSystem`
  - Barbarian skill SFX are delayed by `0.5s`.
  - Fighter first skill SFX are delayed by `0.3s` and played at `2x` volume scale.

- `BossEnemyAIController`
  - Boss movement and attack presentation are now separated by settle/windup delays.
  - Boss attack animation speed is slower, impact is later, sound is lower pitched and delayed slightly.

- `UnitMover`
  - Boss-like units use a slower default movement multiplier.

## Validation

- Ran `dotnet build Assembly-CSharp.csproj`: 0 errors, existing Inspector serialization warnings only.

## Unity Verification

1. Open `boss_test` and enter Play Mode.
2. Click the demo defeat button. All player units should die immediately, then the defeat popup should appear after about `1.2s`.
3. Fight the Boss and kill it. The victory popup should appear after about `1.2s`.
4. Use Barbarian skills and confirm their SFX land about `0.5s` later.
5. Use Fighter skill slot 0 and confirm its SFX lands about `0.3s` later and is louder.
6. Let the Boss take an enemy turn and confirm movement settles before attack, the turn/attack feel slower, and damage lands after a heavier windup.

## Risk And Rollback

- Risk: Boss pacing is tuned by code constants, so further feel tuning still requires script edits unless exposed to Inspector later.
- Rollback: revert the five touched scripts and this DevLog.
