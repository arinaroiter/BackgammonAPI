using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgammonAPI.Domain.ValueObjects
{
    public static class Constants
    {
        public enum OponentTypes { Human, AI, None };
        public enum OponentColor { White, Black, None };
        public enum CurrentPlayerColor { Black, White, Tie, None };

        public enum CheckerColor { Black, White }
        public enum GameStatus { WaitingForOpeningRoll, WaitingForRoll, WaitingForMove, GameOver, None }
    }
}