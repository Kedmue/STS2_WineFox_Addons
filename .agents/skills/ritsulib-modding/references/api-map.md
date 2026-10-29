# RitsuLib API Map

Use this reference for concrete RitsuLib entry points. Verify exact overloads against the package referenced by the current build before editing code.

## Package and Assemblies

Compile-time package:

```text
STS2.RitsuLib.Compat.0.107.1/0.6.9
```

Package directory:

```text
C:\Users\bbmmw\.nuget\packages\sts2.ritsulib.compat.0.107.1\0.6.9\lib\net9.0\
```

Assemblies:

- `STS2-RitsuLib.dll`: facade and compatibility surface.
- `STS2-RitsuLib.Runtime.dll`: framework, content, lifecycle, patching, persistence, networking.
- `STS2-RitsuLib.Shared.dll`: shared persistence and interop primitives.
- `STS2-RitsuLib.Settings.dll`: settings bindings and UI builders.
- `STS2-RitsuLib.Ui.dll`: reusable UI controls.

Useful inspection commands:

```powershell
ilspycmd -t STS2RitsuLib.RitsuLibFramework <Runtime.dll>
ilspycmd -t STS2RitsuLib.Patching.Core.ModPatcher <Runtime.dll>
ilspycmd -t STS2RitsuLib.Networking.Sidecar.RitsuLibSidecarConfigSyncService <Runtime.dll>
ilspycmd -t STS2RitsuLib.Settings.ModSettingsPageBuilder <Settings.dll>
```

XML docs in the same directory are appropriate for intended contracts; decompiled code is authoritative for actual behavior in the installed version.

## Framework Entry Points

`STS2RitsuLib.RitsuLibFramework`:

- `CreateLogger(modId)`
- `CreatePatcher(ownerModId, patcherName, patcherLabel)`
- `SubscribeLifecycle<TEvent>(handler, replayCurrentState)`
- `SubscribeLifecycleOnce<TEvent>(handler, replayCurrentState)`
- `BeginModDataRegistration(modId, initializeProfileIfReady)`
- `GetDataStore(modId)`
- `GetRunSavedDataStore(modId)`
- `GetContentRegistry(modId)`
- `GetKeywordRegistry(modId)`
- `CreateContentPack(modId)`
- `RegisterModSettings(modId, configure, pageId)`
- `EnsureGodotScriptsRegistered(assembly, logger)`
- `GetModRunRng` / `GetModPlayerRng`

`CreatePatcher` returns `ModPatcher`; it is the preferred wrapper around Harmony.

## Registration Attributes

Namespace: `STS2RitsuLib.Interop.AutoRegistration`.

Core examples:

```csharp
[RegisterCard(typeof(ColorlessCardPool))]
[RegisterPower]
[RegisterRelic(typeof(WineFoxRelicPool))]
[RegisterPotion(typeof(WineFoxFoodPotionPool))]
[RegisterSharedPotionPool]
[RegisterActEvent(typeof(Hive))]
[RegisterOwnedCardKeyword("nourishment", IconPath = "res://...")]
```

More registrations available in the package include shared pools, encounters, characters, ancients, epochs, timeline slots, enchantments, afflictions, achievements, modifiers, node attachments, smart formatters, and top-bar buttons.

Registration constraints:

- Register the containing assembly before discovery finalizes.
- Static/abstract/interface types are not concrete discoverable content types.
- For an owned keyword hosted by a static container, nest a private sealed class carrying the attribute.
- A missing optional pool or model must be reported clearly; do not register it into an unrelated fallback pool.

## Patching

Implement `IPatchMethod`:

```csharp
public sealed class MyPatch : IPatchMethod
{
    public static string PatchId => "stable_unique_id";
    public static bool IsCritical => false;
    public static string Description => "What the patch changes.";

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(SomeType), nameof(SomeType.SomeMethod))];

    public static void Postfix() { }
}
```

Defaults and behavior:

- `IsCritical` defaults to `true`.
- `PatchAll` applies each registered static patch once.
- A failed critical patch triggers rollback of the patcher.
- Failed optional patches are logged without blocking otherwise successful patches.
- Duplicate patch IDs are skipped.
- Registering after a successful `PatchAll` throws.
- Target overload parameters determine which method is patched; omitting them can select an unintended overload.
- `ignoreIfMissing: true` marks the target optional.

Register patches through one owned `ModPatcher` during mod initialization. If a whole patch group is optional, catch initialization failure and retain the rest of the mod.

For reward/event callbacks, contain exceptions so a failed convenience effect cannot interrupt a room transition or make save reload repeatedly fail.

## Saved Attached State

Namespace: `STS2RitsuLib.Utils`.

`SavedAttachedState<TKey, TValue>` attaches persisted state to a reference object and hooks the value into the target model’s `SavedProperties`.

```csharp
private static readonly SavedAttachedState<MyCard, bool> State =
    new("MyPersistentField", defaultValueFactory: static () => false);
```

Construction registers the schema field. Instantiate it before the save-property schema is finalized. Use unique stable field names and serializable `TValue` shapes.

Useful methods:

- `ContainsKey`
- `TryGetValue`
- `GetOrAdd`
- `Set`
- `Update`
- `Remove`

Do not replace this with a plain `Dictionary` or static flag for state that must survive save/load or multiplayer reconstruction.

## Persistence Stores

Project pattern: `Settings/AddonModSettingsStore.cs`.

```csharp
using (RitsuLibFramework.BeginModDataRegistration(modId, false))
{
    RitsuLibFramework.GetDataStore(modId).Register(
        dataKey,
        fileName,
        SaveScope.Global,
        defaultFactory: () => new MySettings(),
        autoCreateIfMissing: true);
}

RitsuLibFramework.GetDataStore(modId).InitializeGlobal();
var settings = RitsuLibFramework.GetDataStore(modId).Get<MySettings>(dataKey);
```

Choose the scope deliberately:

- `SaveScope.Global`: shared across profiles.
- `SaveScope.Profile`: profile-specific.
- Run-scoped state: `GetRunSavedDataStore(modId)`.

Keep data keys and file names stable after release.

## Settings

Project pattern: `Settings/AddonModSettingsPage.cs`.

Register a page:

```csharp
RitsuLibFramework.RegisterModSettings(modId, page => page
    .WithTitle(ModSettingsText.Literal("..."))
    .WithModDisplayName(ModSettingsText.Literal("..."))
    .AddSection("section", section => section
        .WithTitle(ModSettingsText.Literal("..."))
        .AddToggle(
            "entry_id",
            ModSettingsText.Literal("..."),
            binding,
            ModSettingsText.Literal("..."))));
```

Available section rows in the current package include toggles, integer/float sliders, choices, enum choices, strings, multiline strings, colors, key/input bindings, buttons, headers, paragraphs, info cards, images, lists, subpages, and custom controls.

Use `WithReadOnlyOnHostSurfaces` or entry-level visibility/enable predicates for context-sensitive UI. Keep setting descriptions short and user-facing; do not expose implementation terms.

Persistence binding pattern:

```csharp
var binding = new ModSettingsValueBinding<MySettings, bool>(
    modId,
    dataKey,
    SaveScope.Global,
    settings => settings.Enabled,
    (settings, value) =>
    {
        if (!CanEdit())
            return;
        settings.Enabled = value;
        PublishAuthoritativeValue();
    });
```

Read the effective runtime value from a separate layer when multiplayer requires host authority. Do not bind gameplay reads directly to local disk settings.

## Choice of Context

- Use the context supplied by card/power/event callbacks for commands that are part of gameplay.
- Use `ThrowingPlayerChoiceContext` only when no caller context exists and synchronous local resolution is intended.
- If a multi-step action needs user choice, preserve the active `PlayerChoiceContext` through the chain.
