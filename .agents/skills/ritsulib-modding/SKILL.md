---
name: ritsulib-modding
description: Implement, review, or debug RitsuLib-based Slay the Spire 2 mod features in this project, including content registration, lifecycle hooks, patching, persistence, settings, host-authoritative sync, ModInterop, localization, and version compatibility.
---

# RitsuLib Modding

Use this skill for work that touches RitsuLib APIs or behavior in this repository. Read the repository `AGENTS.md` first; it owns project content, release, safety, and gameplay-design rules. This skill adds RitsuLib-specific implementation guidance.

## Establish the API Surface

Do not code from remembered RitsuLib signatures.

1. Resolve the compile-time package from `.godot/mono/temp/obj/project.assets.json`. The current project compiles against `STS2.RitsuLib.Compat.0.107.1` `0.6.9`.
2. Treat the installed package as authoritative:

```text
C:\Users\bbmmw\.nuget\packages\sts2.ritsulib.compat.0.107.1\0.6.9\
```

3. Use the module XML files for intended API documentation and `ilspycmd -t <fully-qualified-type> <assembly>` when overload behavior or lifecycle semantics matter.
4. Check the manifest minimum version before using a newer surface. The project currently declares RitsuLib `>= 0.5.0`; do not silently rely on a `0.6.9`-only API. Either keep the code compatible with the declared floor, feature-check it, or update the minimum with explicit justification.

The current package is split across `STS2-RitsuLib.Runtime`, `.Shared`, `.Settings`, and `.Ui`. Reflection that inspects only one assembly can miss types that moved between modules.

## Core Decisions

- Prefer `RitsuLibFramework` plus declarative registration attributes before raw Harmony.
- Prefer `ModPatcher`/`IPatchMethod` for game patches. Patch IDs must be stable and unique. `IPatchMethod.IsCritical` defaults to `true`; set it to `false` for optional behavior so a changed target does not disable the mod.
- Use the `ModPatchTarget` overload with parameter types when targeting an overload. Use `ignoreIfMissing: true` only when degraded behavior is safe and logged.
- Pass the active `PlayerChoiceContext` into commands launched by card/power/gameplay effects. Use a new throwing context only for a genuinely UI-driven or local flow.
- Use game or RitsuLib RNG streams. For a mod-private deterministic stream, use `RitsuLibFramework.GetModPlayerRng` / `GetModRunRng`; for the established vanilla reward stream, use `PlayerRng.Rewards`. Do not mix streams unless the design requires it.
- Treat model instances as canonical. Use `ToMutable()` before placing a card, potion, relic, or reward into mutable gameplay state.
- Keep startup registration idempotent and failure-contained. Infrastructure that must block loading can be critical; cosmetic or optional integrations must not.

## Content Registration

Register models in the same assembly passed to `ModTypeDiscoveryHub.RegisterModAssembly`, then call `EnsureGodotScriptsRegistered`.

Common declarative forms used by this project:

```csharp
[RegisterCard(typeof(ColorlessCardPool))]
[RegisterPower]
[RegisterRelic(typeof(WineFoxRelicPool))]
[RegisterPotion(typeof(WineFoxFoodPotionPool))]
[RegisterSharedPotionPool]
[RegisterActEvent(typeof(Hive))]
[RegisterOwnedCardKeyword("nourishment", IconPath = "res://...")]
```

Auto-registration scans concrete classes. Attribute hosts cannot be static or abstract. For owned keywords, use the project’s static-container/private-nested-class pattern because a static class is abstract in IL and is skipped.

When a fluent content pack is a better fit, use `RitsuLibFramework.CreateContentPack(modId)` instead of duplicating manual registry calls. Consult the package XML for the current builder overloads.

## Lifecycle

`SubscribeLifecycle<TEvent>(handler, replayCurrentState: true)` may invoke synchronously with an already-occurred replayable event. The handler must tolerate replay and startup ordering.

When one-shot behavior is needed, use `SubscribeLifecycleOnce` or the overload that receives the subscription and dispose it before doing duplicate work. Do not infer that “subscribed during initialization” guarantees zero prior events.

Use `EssentialInitializationCompletedEvent` only for post-registration validation that truly depends on all essential content being available. Keep model/registry registration in its documented earlier phase.

## Persistence and Multiplayer

Read [references/persistence-and-sync.md](references/persistence-and-sync.md) whenever the change involves saved fields, settings, rewards, random selection, client state, or multiplayer.

The critical rule is: host-authoritative settings and mutable gameplay state must be reconstructed in the same order on every peer and after save load. Static process-only state is not synchronization.

## ModInterop

For optional mods, define a local static bridge rather than adding a hard assembly reference:

```csharp
[ModInterop("TargetModId", "Target.Namespace.ApiType")]
internal static class TargetApiBridge
{
    public static void OptionalCall(...) { }
}
```

RitsuLib rewrites placeholder members during type discovery. If registration needs the rewritten bridge to be ready, do it from a `LocManager.Initialize` postfix as in `Patches/HextechInteropInitializationPatch.cs`. Missing optional mods must leave no-op stubs and must not affect unrelated registration.

## Localization and Settings

Use game terminology for built-in concepts and keep mod-owned keys stable. `smartDescription` is appropriate when an ability’s `{Amount}` should replace a hard-coded value; preserve all formatting tags and placeholders across languages.

Use RitsuLib settings bindings backed by a registered data store, and read the effective value through a runtime layer that can prefer host state. Client-only setters must not publish unsynchronized gameplay changes.

For settings APIs, start from:

- `Settings/AddonModSettingsStore.cs`
- `Settings/AddonModSettingsPage.cs`
- `Settings/AddonRuntimeSettings.cs`
- `Patches/MultiplayerSyncPatches.cs`

## Detailed API Map

Read [references/api-map.md](references/api-map.md) for concrete entry points, patching semantics, persistence registration, settings builders, and diagnostic commands. Read only the sections relevant to the task.

## Validation

- Build with `dotnet build -c Debug`.
- For network or persistence changes, verify host/client values, reconnect cleanup, and save/load reconstruction.
- Inspect the first state-divergence checkpoint rather than the last symptom. Do not add a compatibility workaround to this extension when the evidence points to another mod or framework.
- Report the exact APIs and game flows exercised, plus any verification that could not be performed.
