[English](AI_USAGE.md) | [Русский](AI_USAGE.ru.md)

# AI Usage

## Tools and workflow

**OpenAI Codex** was used as an AI coding agent with access to the local Unity project codebase. The agent read and modified C# scripts and a scene, analyzed Git, and ran compilation and checks. The author was involved throughout: assigning tasks, discussing architecture, and reviewing proposed changes.

**ChatGPT** and **Gemini** were also used to discuss implementation, explain code, and compare solution options.
Development was iterative: the author's code was combined with AI-suggested code snippets and then refined collaboratively. For this reason, dividing every file cleanly into “written independently” and “AI-generated” would not accurately describe the actual workflow.

## Tasks assigned to AI

- Reviewing the specification and explaining requirements, project structure, and specific sections of code.
- Assisting with Unity IAP v5: store adapter, StorePresenter, currency delivery, transaction history persistence, UI states, and store errors.
- Extracting GameLoop from Bootstrap and adding the component to the scene; handling asynchronous IAP initialization errors with `await`.
- Reviewing analytics added by the author, correcting interfaces, and connecting events to gameplay actions.
- Checking offline calculations and fixing the remaining Boost duration to use elapsed real time.
- Creating five unit tests and checking that they compile and their assertions work.
- Preparing `README.md` and this document based on the current repository state.
- Writing this USAGE document from the author's notes, making it more structured and readable.
- Rewriting the README from the author's notes, making it more structured and readable.

## Examples of key prompts

Examples from the conversation, shortened without changing their meaning. The examples below are translated from Russian:

1. “Go ahead and implement GameLoop, and fix whatever you didn't like in Bootstrap. This won't break the game, right?”
2. “I added new analytics functionality. Review the scripts and tell me whether they meet the specification; point out any weak spots.”
3. “Go ahead and fix all of this” — after the analytics review.
4. “Make sure Bootstrap creates everything needed. Does this architecture support having multiple analytics providers at once?”
5. “Add at least three unit tests for the logic you consider most important” — a request to implement item 6 of the specification.

## Parts implemented with AI assistance

- IAP integration and its related UI, data models, and processed purchase history.
- GameLoop and its integration with Bootstrap and the game scene.
- Improvements to `AnalyticsFacade` and `AnalyticsEvents`: string machine IDs, useful parameters, provider error isolation, and diagnostics for missing initialization.
- Analytics calls in Bootstrap, MachinePresenter, BoostService, AppLifecycleScope, and StorePresenter.
- Fixing Boost timing on restore and saving applied offline progress.
- `Assets/_Project/Tests/Editor/MiniFactoryLogicTests.cs` — five NUnit tests.

## Author's work and collaboration with AI

The author wrote part of the code, formulated tasks, discussed options with Codex, ChatGPT, and Gemini, compared their suggestions, and decided which changes to include in the project.

The workflow did not consist of accepting the first answer: suggested solutions were reviewed, corrected, and discussed again. All of the tools used produced both useful and incorrect suggestions.

The code was created and refined collaboratively, so exact authorship percentages were not calculated. Specific examples of the author's contribution include:

- Preparing the initial analytics implementation, then reviewing and refining it with AI.
- Separating `IAPProduct`, `IAPPurchase`, `IAPFailure`, and `StorePresenter` into individual files.
- Initiating the extraction of the game loop from Bootstrap and discussing initialization order.

The components listed in the previous section were implemented **with AI assistance**; this does not mean they received no author modifications. The author reports manually checking an Android build on a Realme C55 with no issues observed. The AI did not independently build or test the app on a physical device.

## AI suggestions that were changed or discussed

- The author suggested moving `Update` out of Bootstrap. Instead of a general `ITickable` / `TickManager` system, a small GameLoop for two dependencies was retained.
- At the author's suggestion, a unified initialization flow was discussed instead of separating `Awake` and `Start`. The alternative was considered, but in the current code core composition remains in `Awake` and IAP initialization remains in `Start`. The discussion is not presented as an implemented change.
- The author changed the location of IAP classes and moved them into separate files.
- Additional SDKs were not added just to increase the scope of the work: real analytics providers and Remote Config are not part of the current implementation.

AI was used as an assistant for implementation and review. The author remains responsible for the final decisions and for being able to explain the code.

## Follow-up lifecycle work

At the author's request, Codex added `OnApplicationPause` and `OnApplicationFocus` handling through `ApplicationSession`, stopped online production while the app is in the background, and applied offline progress once on return. The pause timestamp is saved if the app is subsequently closed in the background. Four regression tests were added, bringing the total to nine. The README was updated, and the changes were committed separately and merged through `develop`. The author reports a manual Android check on a Realme C55 with no issues observed; the AI did not independently perform or verify a physical-device test.
