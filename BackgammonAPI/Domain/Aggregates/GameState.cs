using BackgammonAPI.Domain.Entities;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Domain.Aggregates
{
    public class GameState
    {

        public int Die1;
        public int Die2;
        public bool isGameStarted = false;
        public bool IsVsAI = false;

        public int TotalDiceRemaining;
        public List<TrianglePoint> AllPoints = new List<TrianglePoint>();
        public List<CheckerPoint> AllCheckers = new List<CheckerPoint>();

        public CurrentPlayerColor CurrentPlayerColor = CurrentPlayerColor.None;
        public OponentTypes MyCurrentOponent = OponentTypes.None;
        public OponentColor OponentColor = OponentColor.None;
        public OponentColor AIColor = OponentColor.None;
        public Guid GameId { get; set; } = Guid.NewGuid();


        public Dictionary<CheckerColor, BarPointBase> Bars { get; set; }
            = new()
            {
                { CheckerColor.Black, new BarPointBlack()},
                { CheckerColor.White, new BarPointWhite() }
            };
        public GameStatus Status { get; internal set; }

        public GameState() { }

        public (int die1, int die2) RollDice()
        {
            var rng = new Random();
            Die1 = rng.Next(1, 7);
            Die2 = rng.Next(1, 7);
            TotalDiceRemaining = Die1 + Die2;
            Status = GameStatus.WaitingForMove;
            return (Die1, Die2);
        }


        // End turn — switch player
        //public void EndTurn()
        //{
        //    currentPlayerColor = currentPlayerColor == CurrentPlayerColor.White
        //        ? CurrentPlayerColor.Black
        //        : CurrentPlayerColor.White;
        //    die1Cube = 0;
        //    die2Cube = 0;
        //    totalDiesAmount = 0;
        //    Status = GameStatus.WaitingForRoll;
        //}
    }
}
