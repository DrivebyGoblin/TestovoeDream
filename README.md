[English](README.md) | [Русский](README.ru.md)

# Mini Factory

A small idle factory for Android, built as a Unity Developer test assignment. Three machines run on one screen: the first is available from the start, while the others can be unlocked with in-game currency. The project includes upgrades, a timed Boost, offline income, and a coin pack purchase flow built with Unity IAP.

## Run in Unity

1. Clone the repository and open its root folder in Unity Hub.
2. Use Unity `6000.3.22f1`. Let Unity restore packages and finish compiling.
3. Open `Assets/_Project/Scenes/MachineCardUI.unity`.
4. Press **Play**. Machines generate currency automatically. Use the interface to unlock and upgrade machines, activate Boost, and purchase coins.
5. To inspect analytics, open the Console. Events are printed with the `[Analytics Event]` label.

## Configuration and save data

The local configuration is `Assets/_Project/Configs/LocalGameConfig.asset`. It contains machine parameters, Boost settings, the offline income cap, the IAP toggle, product ID, and reward. Current values include a 2x Boost lasting 30 seconds, a 24-hour offline cap, and a consumable product `coins_pack_small` that grants 1,000 coins.

Save data is stored as JSON in `PlayerPrefs` under the `GameSaveData` key. In the Unity Editor, **Tools > Clear Save Data** removes the app's PlayerPrefs and `save.json` if it exists.

## Android build

1. In Unity Hub, install **Android Build Support**, including SDK, NDK, and OpenJDK.
2. In Unity, open **File > Build Profiles**, select Android, and switch to that profile.
3. Ensure the game scene is enabled and first in the build scene list; it is already selected in this repository.
4. In Player Settings, use **IL2CPP**, **ARM64**, minimum API level **25**, and automatic target API level. The application ID is `com.DefaultCompany.urp_2d`.
5. For a local APK, disable **Build App Bundle**. **Development Build** is optional.
6. Build outside `Assets`, for example in `Builds/Android`. For **Build And Run**, connect a compatible device with USB debugging enabled and approve the computer on the device.

The author reports that an Android build was manually checked on a Realme C55 and no issues were observed. The AI did not independently build or test the app on a physical device.

The purchase flow can be checked in the Unity Editor with **Fake Store**. A real Google Play product has not been created, so a published purchase is not claimed to work. The purchase button is enabled after the product is retrieved; errors are shown in the UI.

## Systems and architecture

- **Composition:** `Bootstrap` creates and wires models, UI, persistence, analytics, and IAP. Main dependencies are composed in `Awake`; the store starts asynchronously in `Start` with exception handling.
- **Game loop:** `GameLoop` stops online production while the app is in the background and skips the first frame after returning to avoid counting the offline interval twice. It passes `Time.deltaTime` to `BoostService` and then `FactoryModel`. There is no general tick manager; only these two systems update each frame.
- **Factory and economy:** `MachineModel` holds machine state and level, `WalletModel` holds the balance, and `FactoryModel` calculates total production. Presenters connect models to views and player actions.
- **Boost:** `BoostService` tracks remaining duration and multiplier. Production returns to its normal rate when Boost ends.
- **Offline progress:** `OfflineProgressService` calculates elapsed time, caps payable duration, and separates the part earned while Boost was active. When restoring, Boost duration decreases by all real elapsed time, even when income is capped.
- **Lifecycle and persistence:** `AppLifecycleScope` and `SaveService` persist machine state, balance, Boost, and purchase history. Saves happen on pause, focus loss, exit, purchase delivery, and offline-income application. `ApplicationSession` combines `OnApplicationPause` and `OnApplicationFocus` into one absence interval. On return, income is applied once to the current models without reloading balance or levels. If the app is closed in the background, the original pause timestamp is preserved.
- **Configuration:** `GameConfig` carries configuration data, while `LocalGameConfig` is a `ScriptableObject` implementing `IConfigProvider`. Models receive ready-to-use values. The source can be replaced in `Bootstrap`; Firebase Remote Config is not connected.
- **In-app purchases:** `UnityIAPService` isolates Unity IAP behind `IIAPService`; `StorePresenter` connects store UI, wallet, and persistence. The reward is saved before purchase confirmation. Purchase IDs in `PurchaseHistory` prevent duplicate grants.
- **Analytics:** `AnalyticsEvents` defines event names and parameters. `AnalyticsFacade` dispatches them to registered `IAnalyticsProvider` implementations; `ConsoleAnalyticsProvider` is connected. Additional providers can be registered in `Bootstrap`; an error in one provider does not stop the others.

### Production and upgrade formulas

For an unlocked machine starting at level 1:

- Production per second = base production × production multiplier^(level − 1)
- Next upgrade cost = base upgrade cost × cost multiplier^(current level − 1)
- Total production = sum of unlocked machine production × Boost multiplier (or 1 without Boost)

For example, base production of 10 and a multiplier of 2 gives 10, 20, then 40 coins per second. Locked machines produce nothing. Unlock price is `BaseUnlockCost`. These values map to `BaseProduction`, `ProductionMultiplier`, `BaseUpgradeCost`, and `CostMultiplier` in configuration.

### Analytics events

The project emits `game_started`, `machine_unlocked`, `machine_upgraded`, `boost_started`, `boost_finished`, `offline_income_applied`, `purchase_succeeded`, and `purchase_failed`. Machine unlock and upgrade events are sent only after successful payment. Restoring saved state does not create new unlock or upgrade events.

## Packages

- Unity IAP `5.4.3` — consumable purchases
- Unity Test Framework `1.6.0` — NUnit Edit Mode tests
- Unity UI `2.0.0` — UI, including TextMeshPro
- Universal Render Pipeline `17.3.0` — rendering
- Input System `1.20` — input

Packages from the 2D template remain in the project. Versions are recorded in `Packages/manifest.json` and `Packages/packages-lock.json`. Firebase and AppMetrica are not connected.

## Tests

Tests are in `Assets/_Project/Tests/Editor/MiniFactoryLogicTests.cs` and `ApplicationSessionTests.cs`. In Unity, open **Window > General > Test Runner**, select **EditMode**, and choose **Run All**.

The nine tests cover: Boost expiring partway through offline time; the offline income cap; a clock moving backwards; insufficient funds producing no state change or notifications; restoring processed IAP history without duplicate rewards; preserving one pause timestamp across repeated callbacks; pause and focus callbacks arriving in reverse order; pause without focus and repeated pause; and avoiding duplicate income on return.

After the lifecycle changes, the code compiled and all nine test methods were run individually with NUnit assertions: **9/9 passed**. This result does not replace a run in Unity Test Runner or an Android build check. The test files are in an `Editor` folder and are excluded from Android player builds.

## Known limitations and next steps

Some scripts could use cleanup, including removing unused `using` directives and small style improvements. Useful next steps are to check more Android devices, add Play Mode tests, and profile an Android build.

See [AI_USAGE.md](AI_USAGE.md) for a description of AI-assisted work on this project.