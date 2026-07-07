# Backgammon API

A REST API for a fully-featured Backgammon game engine, built with **ASP.NET Core (.NET 8)** and **C#**. The project models the complete rules of Backgammon — dice rolls, move validation, hitting, the bar, and turn management — and supports three kinds of opponent: human, a heuristic computer player, and an **LLM-based opponent powered by the Anthropic Claude API**, all served through an interactive Swagger UI.

This project began with two goals. The first was personal: to build a backgammon game my father and I could play remotely, after not finding an existing one we both liked and found easy to use. The second was to learn how to build a real AI opponent from the ground up — using the game as a way into the broader subject of game AI.

Along the way it became an exercise in designing a clean, well-structured backend: applying **Clean Architecture** and **Domain-Driven Design** principles, separating the game rules from the infrastructure, and packaging the result as a portable Docker container.

**🔗 Live demo:** [Swagger UI](https://backgammon-api.nicetree-3900d9e6.eastus.azurecontainerapps.io/swagger) — the API is deployed on Azure Container Apps; explore and try the endpoints directly in the browser.

---

## Project status

**In progress (~85–90% complete).** The core engine is functional and playable end-to-end within a single game: dice rolls, move validation, hitting, sending checkers to the bar, forced bar re-entry, and turn management all work, against both a human and the AI opponent.

Known gaps still being worked on:

- **End-of-game flow** — detecting a win and starting a fresh game is not yet implemented.
- **Doubles** — rolling a double currently grants two moves; per the rules it should grant four. (The fix is to model the remaining dice as a list rather than a pair — see Roadmap.)
- **Unity client** — the original Unity game has not yet been refactored to talk to this API.

These are tracked in the [Roadmap](#roadmap) below.

---

## Highlights

- **Core game-rule engine** — legal-move validation, direction enforcement, hitting an opponent's blot, sending it to the bar, and forced bar re-entry before any other move.
- **Heuristic AI opponent** — a computer player that evaluates every legal way to play the current turn and selects the highest-scoring one, using a heuristic that rewards hits, building blocks, reinforcing points, and escaping exposed checkers (and entering from the bar first when required). It searches a single turn ahead; evolving this from a hand-tuned heuristic toward search and machine-learned evaluation is a deliberate learning path (see Roadmap).
- **LLM-based AI opponent** — a second AI player that uses the **Anthropic Claude API** to choose moves. The design keeps correctness in the code and reasoning in the model: the engine computes the list of legal moves, and the LLM selects the strategically best one from that list — so the LLM can never produce an illegal move. Both AI players sit behind a shared `IAIPlayer` interface, so the game can switch between heuristic and LLM opponents transparently. Built with async processing, secure API-key management (User Secrets locally, Azure secrets in production), structured logging, and a graceful fallback to a valid move if the API call fails.
- **Clean Architecture / DDD** — four clearly separated layers with dependencies pointing inward, so the core game logic has no knowledge of the web or storage layers.
- **Containerised** — a multi-stage Dockerfile produces a slim runtime image that runs the API identically on any machine.

---

## Architecture

The solution is organised into four layers, following the Clean Architecture dependency rule (outer layers depend on inner layers, never the reverse):

```
┌─────────────────────────────────────────────┐
│  API            Controllers, DTOs, Mappers   │  ← HTTP entry point (Swagger)
├─────────────────────────────────────────────┤
│  Application    GameService, Interfaces      │  ← orchestration of game operations
├─────────────────────────────────────────────┤
│  Infrastructure Repositories (in-memory)     │  ← data persistence
├─────────────────────────────────────────────┤
│  Domain         Game rules, entities, AI     │  ← core logic (no dependencies)
└─────────────────────────────────────────────┘
```

| Layer | Responsibility | Key types |
|-------|----------------|-----------|
| **Domain** | The heart of the game — all rules and state, with no external dependencies | `GameState`, `GameEngine`, `MoveValidator`, `MoveGenerator`, `ComputerPlayer`, `LLMPlayer`, `BoardSetup`, board entities |
| **Application** | Coordinates game operations through a service and defines the interfaces the outer layers implement | `GameService`, `IGameRepository`, `IGameEngine`, `IAIPlayer` |
| **Infrastructure** | Concrete implementations of persistence | `InMemoryGameRepository` (backed by `IMemoryCache`) |
| **API** | HTTP surface — receives requests, maps to/from DTOs, returns JSON | `GameController`, `GameMapper`, DTOs |

The **Domain** layer is deliberately kept free of any reference to ASP.NET, caching, or storage — the game rules could be lifted out and reused in a console app, a desktop client, or the original game engine without modification.

---

## Tech stack

- **.NET 8** / **C#**
- **ASP.NET Core Web API**
- **Swashbuckle / Swagger** for interactive API documentation
- **IMemoryCache** for in-memory game storage
- **Docker** (multi-stage build) for containerisation
- **Azure Container Apps** for cloud hosting, with **Azure Container Registry** for the image
- **GitHub Actions** for CI/CD (automated build and deploy)
- **Anthropic Claude API** for the LLM-based opponent (`async` HttpClient integration)

---

## Getting started

### Try it live (no setup)

The API is deployed and running on Azure — just open the [live Swagger UI](https://backgammon-api.nicetree-3900d9e6.eastus.azurecontainerapps.io/swagger) and try the endpoints in your browser.

### Run with Docker

No .NET SDK required — just Docker.

```bash
# from the project folder (contains the Dockerfile)
docker build -t backgammon-api .
docker run -p 8080:8080 backgammon-api
```

Then open **http://localhost:8080/swagger** in your browser.

### Run locally with the .NET SDK

```bash
cd BackgammonAPI
dotnet run
```

Then open the Swagger URL shown in the console output.

---

## Using the API

All gameplay is driven through the Swagger UI. A typical flow:

**To start a game:**

1. **Create a game** — start a new game with the standard opening position. Pass `isVsAI=true` to play against an AI, and `aiType=Computer` (heuristic, the default) or `aiType=LLM` (Claude) to choose which AI opponent.
2. **Opening roll** — assigns the players' colors and provides the dice for the first turn. The human player always moves first.
3. **Make a move** — using the opening-roll dice, submit the first move(s). No separate roll is needed to begin.

**Then each turn repeats:**

4. **Roll the dice** — the current player rolls for their turn.
5. **Make a move** — submit the checker move(s); the engine validates each one, applies it, handles any hit, and advances the turn when both dice are used.
6. **AI move** — when playing against an AI, trigger it to take its turn. Depending on the `aiType` chosen at game creation, the move is selected either by the heuristic computer player or by the LLM (Claude) opponent (it enters from the bar if needed, then plays its chosen moves).

**At any time:**

7. **Get state** — fetch the current board, dice, and whose turn it is.

The engine enforces the real rules of Backgammon: moves must go in the correct direction, you can't land on a point held by two or more opposing checkers, hitting a lone opposing checker sends it to the bar, and a player with a checker on the bar must re-enter it before making any other move.

---

## Notable design decisions

- **Game logic isolated in the Domain layer.** Validation and state changes live entirely in the domain, with the API and storage layers kept thin. This keeps the rules testable and reusable.
- **Validate-then-mutate.** Moves are fully validated before any board state is changed, so a rejected move never leaves the game in a half-updated state.
- **Interface-based storage.** The Application layer depends on `IGameRepository`, not a concrete store, so the current in-memory implementation can be swapped for a database or distributed cache without touching the game logic.
- **Pluggable AI players behind one interface.** Both the heuristic and the LLM opponents implement a shared `IAIPlayer` interface, and both draw their candidate moves from the same `MoveGenerator`. Selecting an opponent is a matter of routing to the right implementation, and a future stronger AI (e.g. a search- or ML-based player) can be added as just another implementation.
- **Correctness in code, judgment in the model.** The LLM opponent is deliberately *not* trusted to compute or validate moves — the engine produces the exact set of legal moves and the model only picks from it. This eliminates an entire class of errors (illegal or wrong-direction moves) while still letting the LLM contribute strategic reasoning. If the API call fails or returns something unparseable, the player falls back to a valid move so a game is never left broken.

---

## Deployment

The API is hosted on **Azure Container Apps** and deployed automatically through a **GitHub Actions** pipeline. On every push to `main`, the workflow:

1. Builds the Docker image on the GitHub Actions runner.
2. Pushes it to **Azure Container Registry**.
3. Updates the Container App to run the new image.

Each image is tagged with the commit SHA, so every deployment is traceable to the exact commit that produced it. Authentication to Azure uses a service principal whose credentials are stored as encrypted GitHub secrets (never committed to the repository).

---

## Roadmap

This project is actively in progress. Completed and planned work:

**Done**
- [x] RESTful Web API for full gameplay (create game, opening roll, roll, move, AI move, game state)
- [x] Core game engine (moves, validation, hitting, bar re-entry, turn management)
- [x] One-ply heuristic AI opponent
- [x] LLM-based AI opponent (Anthropic Claude API) behind a shared `IAIPlayer` interface, with async processing, secure key management, and graceful fallback
- [x] Clean Architecture layering
- [x] In-memory game storage with `IMemoryCache`, behind a repository interface
- [x] Swagger / OpenAPI documentation
- [x] Dockerised build (multi-stage)
- [x] Live deployment on Azure Container Apps (public Swagger demo)
- [x] CI/CD pipeline (GitHub Actions) — builds the image and deploys to Azure automatically on every push to `main`

**Planned**
- [ ] End-of-game detection (win condition) and start-a-new-game flow
- [ ] Full doubles support — grant four moves on a double (model the dice as a list rather than a pair)
- [ ] Evolve the AI along a learning path (a personal goal of understanding game AI from the ground up). The LLM opponent (done) explores AI through natural-language *reasoning*; a separate, ongoing goal is a classically *strong* engine that plays by *calculation*:
  - [ ] Add lookahead search (minimax / expectiminimax) as a stronger classical baseline
  - [ ] Replace the hand-tuned evaluation with a machine-learned model (e.g. ML.NET or PyTorch) trained on self-play data
  - [ ] Explore self-play reinforcement learning — the approach behind TD-Gammon and GNU Backgammon
- [ ] Richer Swagger metadata and per-endpoint documentation
- [ ] Unit tests covering the `GameEngine` rules
- [ ] Pluggable persistence (e.g. a Redis-backed repository)
- [ ] Promote the four layers into separate projects to enforce the dependency rule at compile time
- [ ] Refactor the original Unity client to play through this API

---

## About

Built by **Arina Roiter**. The project started from a personal wish — a backgammon game to play remotely with my father — and a desire to learn how to build a real AI opponent. It grew into a backend design exercise focused on clean structure, clear separation of concerns, and writing game logic that is correct, readable, and testable.

GitHub: [github.com/arinaroiter/BackgammonAPI](https://github.com/arinaroiter/BackgammonAPI)
