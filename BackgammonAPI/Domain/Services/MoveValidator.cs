using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Domain.Services
{
    public class MoveValidator
    {
        public bool IsMoveInCorrectDirection(TrianglePoint parrentTriangle, int toIndex, CheckerPoint checker, GameState instance, bool consumeDice = true)
        {
            int fromIndex;
            CheckerColor playerColor = checker.color;
            var bar = instance.Bars[checker.color];

            if (bar.GetGameObjects() == null || bar.GetGameObjects().Count == 0)
                fromIndex = parrentTriangle.trianglePointIndex;
            else
            {
                if (playerColor == CheckerColor.White)
                    fromIndex = 24;
                else
                    fromIndex = 12;
            }

            int direction = GetMoveDirection(fromIndex, toIndex, playerColor);
            int moveDistance = toIndex - fromIndex;

            int result = direction * moveDistance;

            if (result > 0)
            {
                var distance = 0;

                if (playerColor == CheckerColor.White)
                    distance = GetMoveDistanceWhite(fromIndex, toIndex);
                else
                    distance = GetMoveDistanceBlack(fromIndex, toIndex);
                //TODO: handle double die - enable to go 4 times the same number
                if (instance.Die1.Equals(distance))
                {
                    if (consumeDice)  // ← GameEngine consumes, AI just checks
                    {
                        instance.TotalDiceRemaining -= distance;
                        instance.Die1 = 0;
                    }
                    return true;
                }
                else if (instance.Die2.Equals(distance))
                {
                   
                    if (consumeDice)  // ← GameEngine consumes, AI just checks
                    {
                        instance.TotalDiceRemaining -= distance;
                        instance.Die2 = 0;
                    }
                    return true;
                }

                return false;
            }

            return false;
        }

        public int GetMoveDistanceBlack(int fromIndex, int toIndex)
        {
            if (fromIndex >= 1 && fromIndex <= 12 && toIndex < fromIndex && toIndex >= 0)
            {
                // Moving left on upper row
                return fromIndex - toIndex;
            }
            if (fromIndex == 0 && toIndex >= 12 && toIndex <= 23)
            {
                // Jump from upper 0 to lower 12
                return 1 + (toIndex - 12);
            }
            if (fromIndex >= 12 && fromIndex < 23 && toIndex > fromIndex && toIndex <= 23)
            {
                // Moving right on lower row
                return toIndex - fromIndex;
            }
            return -1; // Invalid move
        }

        public int GetMoveDirection(int fromIndex, int toIndex, CheckerColor color)
        {
            if (color == CheckerColor.Black)
            {
                return fromIndex <= 12 && toIndex <= 11 ? -1 : 1; //12 because to include the checker from gray bar


            }
            else // White
                return fromIndex <= 11 && toIndex <= 11 ? 1 : -1;
        }

        public int GetMoveDistanceWhite(int fromIndex, int toIndex)
        {
            if (fromIndex <= 24 && fromIndex >= 13 && toIndex < fromIndex && toIndex >= 12)
            {
                // Moving left on lower row
                return fromIndex - toIndex;
            }
            if (fromIndex == 12 && toIndex >= 0 && toIndex <= 11)
            {
                // Jump from lower 12 to upper 0
                return 1 + toIndex;
            }
            if (fromIndex >= 0 && fromIndex < 11 && toIndex > fromIndex && toIndex <= 11)
            {
                // Moving right on upper row
                return toIndex - fromIndex;
            }
            return -1; // Invalid move
        }

        public bool IsMoveAllowed(CheckerPoint checker, TrianglePoint targetTriangle, out bool isEatMe, GameState instance)
        {
            isEatMe = false;
            var targetList = targetTriangle.checkersOnPoint;

            var bar = instance.Bars[checker.color];

            if (bar.GetGameObjects().Count > 0)
            {
                var lastCheckerOnBar = bar.GetGameObjects()[bar.GetGameObjects().Count - 1];
                if (checker.Equals(lastCheckerOnBar))
                {
                  
                    if (checker.color.Equals(lastCheckerOnBar.color))
                    {
                        if (targetList.Count > 0)
                        {
                            if (targetList[0] != null && targetList.Count == 1 && !targetList[0].tagName.Equals(checker.tagName))
                            {
                                isEatMe = true;
                                return true;
                            }

                            if (targetList[0] != null && targetList.Count > 1 && targetList[targetList.Count - 1].tagName.Equals(checker.tagName))
                            {
                                return true;
                            }
                        }
                    }
                }
                else 
                {

                    if (checker.color.ToString().Equals(lastCheckerOnBar.color.ToString()) && instance.CurrentPlayerColor.ToString().Equals(lastCheckerOnBar.color.ToString()))
                    {
                        return false;
                    }
                }
            }
          


            if (targetList.Count == 0)
                return true; // Empty point

            var topChecker = targetList[0];

           
            if (topChecker != null && topChecker.tagName.Equals(checker.tagName))
                return true;

            if (topChecker != null && targetList.Count == 1 && !topChecker.tagName.Equals(checker.tagName))
            {
                isEatMe = true;
                return true;
            }

            return false;

        }

    }
}
