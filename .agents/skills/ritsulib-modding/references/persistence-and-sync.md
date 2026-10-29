# Persistence and Multiplayer Synchronization

Use this reference for RitsuLib settings, synchronized gameplay configuration, persistent attached state, rewards, random selection, and state-divergence work.

## Invariants

- The host owns gameplay-affecting runtime configuration.
- Every peer must reconstruct the same state from the same saved data and event order.
- Local disk settings are not automatically equivalent to the active multiplayer value.
- Process-only static state is not persisted and is not synchronized.
- Random outcomes use synchronized RNG streams and identical guard order.
- Canonical models must not be mutated. Use mutable copies in gameplay state.

## Host-Authoritative Sidecar Topic

Namespace: `STS2RitsuLib.Networking.Sidecar`.

`RitsuLibSidecarConfigSyncService` exposes:

- `RegisterTopic<TState, TDelta>(topic, initialState, canClientRequest, applyDelta)`
- `TryRequestClientChange<TDelta>(...)`
- `TryGetTopicState<TState>(...)`
- `PublishHostState(netService, topic, changedBy, reason)`
- `TopicChanged`

For a host-only setting, this shape is appropriate:

```csharp
RitsuLibSidecarConfigSyncService.RegisterTopic<SettingsSnapshot, SettingsSnapshot>(
    "unique_stable_topic",
    BuildLocalSnapshot(),
    (_, _) => false,
    (state, _) => state);
```

`TopicChanged` runs synchronously after the cached topic state is committed. Deserialize the snapshot and copy only the values needed by gameplay; do not hold deserialized UI-only objects.

Publishing:

```csharp
if (netService is NetHostGameService)
    RitsuLibSidecarConfigSyncService.PublishHostState(
        netService,
        "unique_stable_topic",
        0,
        "peer_ready");
```

Publish at all points where a client may become able to receive state:

- ENet host start.
- Steam host start.
- Peer becomes broadcast-ready.

Clear remote cache when a client connection initializes and when it disconnects. Without both resets, a later room can consume the previous host's settings.

`TryRequestClientChange` success only means the send was accepted. It does not mean the host approved or persisted the change. If clients are not allowed to change the setting, reject requests in `RegisterTopic`.

`PublishHostState` is fire-and-forget; it does not report broadcast success.

## Project Reference Flow

- `Settings/AddonRuntimeSettings.cs`: effective local/remote value and topic state.
- `Patches/MultiplayerSyncPatches.cs`: publish/reset lifecycle hooks.
- `Settings/AddonModSettingsPage.cs`: editing policy and UI binding.
- `Settings/AddonModSettingsStore.cs`: disk persistence.

Keep these layers separate:

1. Persisted local value.
2. Host-authoritative active snapshot.
3. Effective value consumed by gameplay.
4. UI binding and editability policy.

## Rewards and Random Selection

- Run party/unlock/room/act checks before drawing a random value.
- A failed gate must not consume reward RNG.
- Use `player.PlayerRng.Rewards.NextItem(...)` when participating in the established reward stream.
- Return `ToMutable()` when the selected model enters a reward or player state.
- Persist a predetermined model ID when the reward must reopen with the same result.
- Catch exceptions around reward Postfixes and event transitions; rethrow only when the caller can safely stop the room flow.
- Add an idempotency check before adding a reward from a method that can rerun during save/load.

The project’s `BossDishRewardPatch`, `DishPotionReward`, and `DishLoot` are the reference implementation.

## Persistent Model State

Use `SavedAttachedState<TKey, TValue>` for data attached to a saved model. Register/construct the state early enough for RitsuLib to include its field in `SavedProperties`.

Good candidates:

- Per-card permanent flags.
- Per-relic internal counters that are not already native model properties.
- Mod-specific state that must survive serialization.

Bad candidates:

- UI caches.
- Derived values that can be recomputed deterministically.
- Data already represented by the base game model.
- Unbounded dictionaries keyed by transient objects.

When a value can exist on either a canonical model or a combat/deck copy, explicitly read both locations in the same order on all peers, as `Hextech/WineFoxPersistentCardState.cs` does.

## Divergence Diagnosis

Start with the first failed checkpoint, not the final disconnect.

Look for:

- Difference in choice IDs.
- HP, block, powers, card piles, potions, relics, or gold.
- RNG labels, seeds, and counters.
- `savedProperties.mapHash`.
- Exceptions immediately before the divergence.

Use RitsuLib diagnostic packages:

```text
metadata.json
local-debug-log.records.json
remote-debug-log.records.json
state-divergence-report.txt
```

If a choice ID is reused by the wrong subsystem, fix the lifecycle/order at the source. Do not make this extension special-case another mod's internal card or relic to hide the mismatch unless the user explicitly requests a local workaround.
