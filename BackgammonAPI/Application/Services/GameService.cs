using BackgammonAPI.Application.Interfaces;
using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using BackgammonAPI.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using static BackgammonAPI.Domain.ValueObjects.Constants;


namespace BackgammonAPI.Application.Services
{
    public class GameService
    {
        private readonly IGameRepository _repository;
        private readonly GameEngine _engine;


        // ✅ IMemoryCache injected via constructor — ASP.NET handles this
        public GameService(IGameRepository repository, GameEngine engine)
        {
            _repository = repository;
            _engine = engine;
        }

        public GameState CreateGame(bool isVsAI, AI_PlayerType aiType)
        {
            var state = new GameState
            {
                IsVsAI = isVsAI,
                AIType = aiType,
                AIColor = OponentColor.None,
                Status = GameStatus.WaitingForOpeningRoll,
                CurrentPlayerColor = CurrentPlayerColor.None

            };
            BoardSetup.Initialize(state);
            SaveGame(state);
            return state;
        }

        public GameState? GetGame(string gameId)
        {
            // returns null if not found or expired

            //return _repository.Get<GameState>(gameId);
            return _repository.GetGame(gameId);
        }

        public void SaveGame(GameState state)
        {
            _repository.SaveGame(state);
            
        }

        public void DeleteGame(string gameId)
        {
            _repository.DeleteGame(gameId);
        }

        public void OpeningRoll(string gameId)
        {
            var state = GetGame(gameId);
            var rng = new Random();

            int blackRoll = rng.Next(1, 7);
            int whiteRoll = rng.Next(1, 7);

            state.MyCurrentOponent = OponentTypes.Human;
            if (state.IsVsAI)
            {
                state.MyCurrentOponent = OponentTypes.AI;
            }

            if (blackRoll > whiteRoll)
            {
                state.CurrentPlayerColor = CurrentPlayerColor.Black;
                state.Die1 = blackRoll;
                state.Die2 = whiteRoll;
                state.OponentColor = OponentColor.White;

                if (state.IsVsAI)
                {
                    state.AIColor = OponentColor.White;
                }
            }
            else if (whiteRoll > blackRoll)
            {
                state.CurrentPlayerColor = CurrentPlayerColor.White;
                state.Die1 = whiteRoll;
                state.Die2 = blackRoll;
                state.OponentColor = OponentColor.Black;

                if (state.IsVsAI)
                {
                    state.AIColor = OponentColor.Black;
                }
            }
            else
            {
                // Tie — roll again, stay in WaitingForOpeningRoll
                state.Status = GameStatus.WaitingForOpeningRoll;
                SaveGame(state);
                return;
            }

            // Winner uses these dice for first move
            state.TotalDiceRemaining = state.Die1 + state.Die2;
            state.IsGameStarted = true;
            state.Status = GameStatus.WaitingForMove; // ← skip WaitingForRoll!
            SaveGame(state);
        }
        // ─── Human Move ───────────────────────────────────────────────
        public bool ExecuteHumanMove(string gameId,
            int selectedCheckerIndex, int targetTriangleIndex)
        {
            var state = GetGame(gameId);
            if (state == null) return false;

            // Find checker — search AllCheckers AND bars
            var checker = FindChecker(state, selectedCheckerIndex);
            var triangle = state.AllPoints
                .FirstOrDefault(p => p.trianglePointIndex == targetTriangleIndex);

            if (checker == null || triangle == null) return false;

            bool success = _engine.MoveCheckerToPoint(checker, triangle, state);

            return success;
        }

        // ─── AI Move ──────────────────────────────────────────────────
        public async Task<bool> ExecuteAIMove(string gameId)
        {
            var state = GetGame(gameId);
            if (state == null) return false;

            bool success = false;
            state.RollDice();

            success = await _engine.AIMoveCheckerToPoint(state);

            if (state.TotalDiceRemaining > 0)
                success = await _engine.AIMoveCheckerToPoint(state);

            return success;
        }

        // ─── Roll Dice ────────────────────────────────────────────────
        public (int die1, int die2) RollDice(string gameId)
        {
            var state = GetGame(gameId);
            var result = state.RollDice();
            SaveGame(state);      // ← explicit save after roll
            return result;
        }

        // ─── Find Checker — always checks both board AND bars ─────────
        public CheckerPoint FindChecker(GameState state, int checkerIndex)
        {
            // Search board checkers first
            var checker = state.AllCheckers
                .FirstOrDefault(c => c.checkerPointIndex == checkerIndex);

            // Search bars if not found on board
            if (checker == null)
                checker = state.Bars[CheckerColor.Black]
                    .GetGameObjects()
                    .Concat(state.Bars[CheckerColor.White].GetGameObjects())
                    .FirstOrDefault(c => c.checkerPointIndex == checkerIndex);

            return checker;
        }

    }
}