using BackgammonAPI.Application.Interfaces;
using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using static BackgammonAPI.Domain.ValueObjects.Constants;
using Microsoft.Extensions.Configuration;

namespace BackgammonAPI.Domain.Services
{
    public class LLMPlayer : IAIPlayer
    {
        private readonly MoveGenerator _moveGenerator;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<LLMPlayer> _logger;


        public LLMPlayer(MoveGenerator moveGenerator, HttpClient httpClient, IConfiguration configuration, ILogger<LLMPlayer> logger)
        {
            _moveGenerator = moveGenerator;
            _httpClient = httpClient;
            _apiKey = configuration["ANTHROPIC_API_KEY"] ?? "";
            _logger = logger;

        }

        public async Task<Move> GetValidMoveAsync(OponentColor oponentColor, List<TrianglePoint> points, GameState instance)
        {
            List<Move> legalMoves = _moveGenerator.GetLegalMoves(oponentColor, points, instance);
            _logger.LogInformation("LLM: {Count} legal moves available", legalMoves.Count);

            if (legalMoves.Count == 0)
            {
                _logger.LogInformation("LLM: No legal moves.");
                return null;
            }

            // No point calling the LLM if there's only one option
            if (legalMoves.Count == 1)
            {
                _logger.LogInformation("LLM: Only one legal move, skipping Claude.");
                return legalMoves[0];
            }

            string prompt = BuildPrompt(oponentColor, points, legalMoves);
            _logger.LogDebug("LLM prompt:\n{Prompt}", prompt);

            int chosenIndex = await AskClaudeAsync(prompt);
            _logger.LogInformation("LLM: Claude chose index {Index}", chosenIndex);

            if (chosenIndex >= 0 && chosenIndex < legalMoves.Count)
                return legalMoves[chosenIndex];

            //await Task.CompletedTask;

            _logger.LogWarning("LLM: Invalid/failed choice, falling back to first legal move.");
            return legalMoves[0];
        }

        private string BuildPrompt(OponentColor oponentColor, List<TrianglePoint> points, List<Move> legalMoves)
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are playing backgammon. Choose the strategically best move.");
            sb.AppendLine();
            sb.AppendLine("MOVEMENT RULES:");
            sb.AppendLine("White path: 23->12 (descending), then wraps 12->0->11 (ascending), bears off after 11.");
            sb.AppendLine("Black path: 11->0 (descending), then wraps 0->12->23 (ascending), bears off after 23.");
            sb.AppendLine("A point with 2+ enemy checkers is blocked. Landing on 1 enemy checker hits it.");
            sb.AppendLine();
            sb.AppendLine($"You are playing as: {oponentColor.ToString()}");
            sb.AppendLine();

            // Board state
            sb.AppendLine("CURRENT BOARD:");
            foreach (var p in points)
            {
                if (p.checkersOnPoint.Count > 0)
                {
                    var c = p.checkersOnPoint[0].color;
                    sb.AppendLine($"Point {p.trianglePointIndex}: {p.checkersOnPoint.Count} {c} checkers");
                }
            }
            sb.AppendLine();

            // Numbered legal moves
            sb.AppendLine("LEGAL MOVES (already validated - choose ONLY from these):");
            for (int i = 0; i < legalMoves.Count; i++)
            {
                var move = legalMoves[i];
                string from = move.checkerPoint.parentTriangle != null
                    ? move.checkerPoint.parentTriangle.trianglePointIndex.ToString()
                    : "bar";
                int to = move.trianglePoint.trianglePointIndex;
                sb.AppendLine($"{i + 1}. from {from} to {to}");
            }
            sb.AppendLine();
            sb.AppendLine("IMPORTANT: Respond with ONLY a single number - the number of your chosen move. No explanation, no analysis, no words. Just the digit. Example: 2");

            return sb.ToString();
        }

        private async Task<int> AskClaudeAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    model = "claude-sonnet-4-6",
                    max_tokens = 10,
                    temperature = 0,
                    system = "You are a backgammon move selector. Respond with ONLY a single number - the index of the chosen move. No words, no analysis, no explanation. Just the digit.",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
                request.Headers.Add("x-api-key", _apiKey);
                request.Headers.Add("anthropic-version", "2023-06-01");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return -1;  // API error -> fallback

                // Extract Claude's text answer
                using var doc = JsonDocument.Parse(responseBody);
                string text = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                _logger.LogInformation("LLM: Raw response {Body}", responseBody);

                // Parse the leading number, convert 1-based -> 0-based index
                string digits = new string(text.Trim().TakeWhile(char.IsDigit).ToArray());
                if (int.TryParse(digits, out int choice))
                    return choice - 1;

                return -1;
            }
            catch (Exception)
            {
                return -1;  // any failure -> fallback to first legal move
            }
        }
    }
}