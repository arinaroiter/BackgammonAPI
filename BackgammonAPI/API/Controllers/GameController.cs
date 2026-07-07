using BackgammonAPI.API.Dtos;
using BackgammonAPI.API.Mappers;
using BackgammonAPI.Application.Services;
using Microsoft.AspNetCore.Mvc;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly GameService _gameService;
        private readonly GameMapper _mapper;

        public GameController(GameService gameService, GameMapper gameMapper)
        {
            _gameService = gameService;
            _mapper = gameMapper;
        }

        // ─── POST /api/game ───────────────────────────────────────────────
        // Start a new game — returns gameId to client
        [HttpPost]
        public IActionResult CreateGame([FromQuery] bool isVsAI = false,[FromQuery] AI_PlayerType aiType = AI_PlayerType.Computer)
        {
            var state = _gameService.CreateGame(isVsAI, aiType);
            return Ok(new { gameId = state.GameId, message = "Game created!" });
        }

        // ─── GET /api/game/{gameId} ───────────────────────────────────────
        // Get current board state
        [HttpGet("{gameId}")]
        public IActionResult GetGame(string gameId)
        {
            var state = _gameService.GetGame(gameId);
            if (state == null)
                return NotFound(new { message = "Game not found or expired." });

            return Ok(_mapper.ToDto(state));
        }

        // ─── POST /api/game/{gameId}/roll ─────────────────────────────────
        // Roll dice — only allowed when status is WaitingForRoll
        [HttpPost("{gameId}/roll")]
        public IActionResult RollDice(string gameId)
        {
            var state = _gameService.GetGame(gameId);
            if (state == null)
                return NotFound(new { message = "Game not found or expired." });

            if (state.Status != GameStatus.WaitingForRoll)
                return BadRequest(new { message = $"Cannot roll now. Current status: {state.Status}" });

            var (die1, die2) = _gameService.RollDice(gameId);

            var updatedState = _gameService.GetGame(gameId);

            return Ok(new
            {
                die1,
                die2,
                currentPlayer = updatedState.CurrentPlayerColor.ToString(),
                gameState = _mapper.ToDto(updatedState)
            });
        }

        // ─── POST /api/game/{gameId}/move ─────────────────────────────────
        // Make a move
        [HttpPost("{gameId}/move")]
        public IActionResult MakeMove(string gameId, [FromBody] MakeMoveRequest request)
        {
            var state = _gameService.GetGame(gameId);
            if (state == null)
                return NotFound(new { message = "Game not found or expired." });

            if (state.Status != GameStatus.WaitingForMove)
                return BadRequest(new { message = $"Cannot move now. Current status: {state.Status}" });


            var (checker, triangle) = _mapper.FromDto(state, request);

            if (checker == null)
                return BadRequest(new { message = "Checker not found." });

            if (triangle == null)
                return BadRequest(new { message = "Triangle not found." });


            // Validate it's the right player's turn
            if (state.CurrentPlayerColor.ToString() != request.Color)
                return BadRequest(new { message = "Not your turn!" });

            //(CheckerPoint checker, TrianglePoint triangle) value = _mapper.FromDto(state,request);

            bool success = _gameService.ExecuteHumanMove(
                                        gameId,
                                        checker.checkerPointIndex,
                                        triangle.trianglePointIndex);

            if (!success)
                return BadRequest(new { message = "Invalid move." });

            _gameService.SaveGame(state);
            return Ok(_mapper.ToDto(state));
        }

        // ─── POST /api/game/{gameId}/ai-move ──────────────────────────────
        // Trigger AI move — Unity calls this on AI's turn
        [HttpPost("{gameId}/ai-move")]
        public async Task<IActionResult> AiMove(string gameId)
        {
            var state = _gameService.GetGame(gameId);
            if (state == null)
            {
                return NotFound(new
                {
                    message = "Game not found or expired."
                });
            }

            if (!state.IsVsAI)
            {
                return BadRequest(new
                {
                    message = "This is not an AI game.",
                    gameState = _mapper.ToDto(state)
                });

            }
            //think about it
            if (state.Status == GameStatus.WaitingForMove)
                state.Status = GameStatus.WaitingForRoll;

            if (state.Status != GameStatus.WaitingForRoll)
                return BadRequest(new
                {
                    message = "Cannot move now.",
                    gameState = _mapper.ToDto(state)
                });


            // Validate it's actually AI's turn
            if (state.CurrentPlayerColor.ToString() != state.AIColor.ToString())
                return BadRequest(new
                {
                    message = "Not AI's turn.",
                    gameState = _mapper.ToDto(state)
                });

            bool success = await _gameService.ExecuteAIMove(gameId);

            if (!success)
                return BadRequest(new
                {
                    message = "AI could not find a valid move.",
                    gameState = _mapper.ToDto(state)
                });

            var updatedState = _gameService.GetGame(gameId);
            _gameService.SaveGame(updatedState);
            return Ok(_mapper.ToDto(updatedState));
        }

        // ─── DELETE /api/game/{gameId} ────────────────────────────────────
        // Abandon/delete a game
        [HttpDelete("{gameId}")]
        public IActionResult DeleteGame(string gameId)
        {
            var state = _gameService.GetGame(gameId);
            if (state == null)
                return NotFound(new { message = "Game not found or expired." });

            _gameService.DeleteGame(gameId);
            return Ok(new { message = "Game deleted." });
        }

        // ─── POST /api/game/{gameId}/opening-roll ─────────────────
        // Both players roll one die — highest goes first
        [HttpPost("{gameId}/opening-roll")]
        public IActionResult OpeningRoll(string gameId)
        {
            var state = _gameService.GetGame(gameId);
            if (state == null)
                return NotFound(new { message = "Game not found or expired." });

            if (state.Status != GameStatus.WaitingForOpeningRoll)
                return BadRequest(new { message = "Opening roll already done." });

            _gameService.OpeningRoll(gameId);

            return Ok(_mapper.ToDto(_gameService.GetGame(gameId)));
        }

    }
}
