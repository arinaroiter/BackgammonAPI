using BackgammonAPI.Application.Interfaces;
using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Domain.Services
{
    public class GameEngine : IGameEngine
    {

        private readonly MoveGenerator _moveGenerator;
        private readonly MoveValidator _moveValidator;
        private readonly ComputerPlayer _computerPlayer;
        private readonly LLMPlayer _llmPlayer;
        public GameEngine(MoveGenerator moveGenerator, MoveValidator moveValidator, ComputerPlayer computerPlayer,LLMPlayer llmPlayer)
        {
            _moveGenerator = moveGenerator;
            _moveValidator = moveValidator;
            _computerPlayer = computerPlayer;
            _llmPlayer = llmPlayer;

        }

        public async Task<bool> AIMoveCheckerToPoint(GameState instance)
        {
            bool success = true;
            try
            {
                IAIPlayer player = GetAIPlayer(instance.AIType);
                Move move = await player.GetValidMoveAsync(instance.AIColor, instance.AllPoints, instance);

                if (move != null)
                {
                    new CheckerPoint().SetChecker(move.checkerPoint);
                    new TrianglePoint().SetTriangle(move.trianglePoint);

                    MoveCheckerToPoint(move.checkerPoint, move.trianglePoint, instance);
                }
                //selectedChecker = null; // Deselect after the move
            }
            catch (Exception e) { success = false; }

            return success;
        }

        private IAIPlayer GetAIPlayer(AI_PlayerType aiType)
        {
            return aiType == AI_PlayerType.LLM
                ? _llmPlayer
                : _computerPlayer;
        }
        public bool MoveCheckerToPoint(CheckerPoint selectedChecker, TrianglePoint selectedTriangle, GameState instance)
        {
            bool success = true;
            try
            {
                bool isEatMe;

                if (_moveValidator.IsMoveAllowed(selectedChecker, selectedTriangle, out isEatMe, instance))
                {
                    if (!_moveValidator.IsMoveInCorrectDirection(selectedChecker.parentTriangle, selectedTriangle.trianglePointIndex, selectedChecker, instance))
                    {

                        return false; // Or show error to player
                    }

                    if (isEatMe)
                    {
                        EatTheChecker(selectedTriangle, instance);

                    }

                    if (instance.Bars[selectedChecker.color] != null && instance.Bars[selectedChecker.color].GetGameObjects().Count > 0)//&& selectedChecker.barPoint.GetGameObjects()[selectedChecker.barPoint.GetGameObjects().Count - 1] == selectedChecker)
                    {
                        if (instance.Bars[selectedChecker.color].GetGameObjects().Contains(selectedChecker))
                        {
                            instance.Bars[selectedChecker.color].RemoveGameObject(selectedChecker);
                            MoveCheckerToNewPosition(selectedChecker, selectedTriangle, instance);
                        }



                    }

                    else if (selectedChecker.parentTriangle != null && selectedChecker.parentTriangle.checkersOnPoint.Count > 0)//&& selectedChecker.parentTriangle.checkersOnPoint[selectedChecker.parentTriangle.checkersOnPoint.Count - 1] == selectedChecker)
                    {

                        selectedChecker.parentTriangle.checkersOnPoint.Remove(selectedChecker);

                        MoveCheckerToNewPosition(selectedChecker, selectedTriangle, instance);


                    }

                }

            }
            catch (Exception e) { success = false; }



            return success;

        }

        private void EatTheChecker(TrianglePoint selectedTriangle, GameState instance)
        {
 
            var targetList = selectedTriangle.checkersOnPoint;

            if (targetList.Count == 1)
            {
                var topChecker = targetList[0];
                selectedTriangle.checkersOnPoint.Remove(topChecker);
                MoveEatenCheckerToBar(topChecker, instance);
            }

        }

        private void MoveEatenCheckerToBar(CheckerPoint checker, GameState instance)
        {

            instance.Bars[checker.color].AddGameObject(checker);
            checker.parentTriangle = null;

        }

        private void MoveCheckerToNewPosition(CheckerPoint checker, TrianglePoint selectedTriangle, GameState instance)
        {
            var point = selectedTriangle;
            checker.parentTriangle = point;


            TrianglePoint parentTriangle = checker.parentTriangle;
            if (parentTriangle.checkersOnPoint.Contains(checker))
                parentTriangle.checkersOnPoint.Remove(checker);


            if (!point.checkersOnPoint.Contains(checker))
                point.checkersOnPoint.Add(checker);


            if (instance.TotalDiceRemaining == 0)
            {
                instance.Status = GameStatus.WaitingForRoll;
                instance.Die1 = 0;
                instance.Die2 = 0;

                if (instance.CurrentPlayerColor == CurrentPlayerColor.White)
                {
                    instance.CurrentPlayerColor = CurrentPlayerColor.Black;
                }
                else if (instance.CurrentPlayerColor == CurrentPlayerColor.Black)
                {
                    instance.CurrentPlayerColor = CurrentPlayerColor.White;

                }
            }



        }


    }

}