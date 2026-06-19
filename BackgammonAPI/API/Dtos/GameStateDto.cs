using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.API.Dtos
{
    public class GameStateDto
    {
        public string GameId { get; set; }
        public string CurrentPlayerColor { get; set; }
        public string Status { get; set; }
        public int Die1 { get; set; }
        public int Die2 { get; set; }

        public bool IsGameStarted { get; set; }
        public bool IsVsAI { get; set; }

        public string MyCurrentOponent {get; set;}
        public string AIColor { get; set; }
        public int TotalDiceRemaining { get; set; }
        public List<TrianglePointDto> Points { get; set; }
        public List<CheckerPointDto> AllCheckers { get; set; }       // ← ADD
        public List<CheckerPointDto> BlackBarCheckers { get; set; }
        public List<CheckerPointDto> WhiteBarCheckers { get; set; }
    }
}
