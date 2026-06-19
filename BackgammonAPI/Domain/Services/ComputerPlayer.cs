using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using BackgammonAPI.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Domain.Services
{
    public class ComputerPlayer
    {
        private static readonly Random _random = new Random();
        private readonly MoveValidator _moveValidator;

        public ComputerPlayer(MoveValidator moveValidator)
        {
            _moveValidator = moveValidator;
        }

        public Move GetValidMove(OponentColor oponentColor, List<TrianglePoint> points, GameState instance)
        {
            List<Move> relevantMovesFromBar = new List<Move>();
            CheckerColor aiColor = Enum.Parse<CheckerColor>(instance.AIColor.ToString());

            // ─── Priority 1 — Bar checker MUST play first ─────────
            var barCheckers = instance.Bars[aiColor].GetGameObjects();
            if (barCheckers.Count > 0)
            {
                var barChecker = barCheckers[^1];

                // Black enters upper row 11→0, White enters lower row 23→12
                var entryPoints = aiColor == CheckerColor.Black
                    ? points
                        .Where(p => p.trianglePointIndex >= 0
                            && p.trianglePointIndex <= 11)
                        .OrderByDescending(p => p.trianglePointIndex) // 11 first
                        .ToList()
                    : points
                        .Where(p => p.trianglePointIndex >= 12
                            && p.trianglePointIndex <= 23)
                        .OrderByDescending(p => p.trianglePointIndex) // 23 first
                        .ToList();

                foreach (TrianglePoint point in entryPoints)
                {
                    bool isEatMe;
                    if (_moveValidator.IsMoveAllowed(
                            barChecker, point, out isEatMe, instance))
                    {
                        if (_moveValidator.IsMoveInCorrectDirection(
                                barChecker.parentTriangle,
                                point.trianglePointIndex,
                                barChecker,
                                instance,
                                consumeDice: false))
                        {
                            relevantMovesFromBar.Add(new Move
                            {
                                checkerPoint = barChecker,
                                trianglePoint = point
                            });
                        }
                    }
                }

                if (relevantMovesFromBar.Count > 0)
                    return relevantMovesFromBar
                        .OrderByDescending(m => ScoreMove(m))
                        .First();

                return null;
            }

            List<TrianglePoint> relevantOnBoardPoints = new List<TrianglePoint>();
            List<TrianglePoint> notRelevantOnBoardPoints = new List<TrianglePoint>();
            List<Move> relevantOnBoardMoves = new List<Move>();

            foreach (TrianglePoint point in points)
            {
                if (point.checkersOnPoint.Count > 0)
                {
                    var topChecker = point.checkersOnPoint[point.checkersOnPoint.Count - 1];
                    if (topChecker != null)
                    {
                        if (topChecker.color.ToString().Equals(oponentColor.ToString()))
                            relevantOnBoardPoints.Add(point);
                        else notRelevantOnBoardPoints.Add(point);
                    }
                }
                else notRelevantOnBoardPoints.Add(point);

            }

            foreach (TrianglePoint point in relevantOnBoardPoints)
            {
                foreach (TrianglePoint somePoint in notRelevantOnBoardPoints)
                {
                    bool isEatMe;
                    var topChecker = point.checkersOnPoint[point.checkersOnPoint.Count - 1];

                    if (_moveValidator.IsMoveAllowed(topChecker, somePoint, out isEatMe, instance))
                    {

                        if (_moveValidator.IsMoveInCorrectDirection(topChecker.parentTriangle, somePoint.trianglePointIndex, topChecker, instance, consumeDice: false))
                        {

                            Move move = new Move();
                            move.checkerPoint = topChecker;
                            move.trianglePoint = somePoint;

                            relevantOnBoardMoves.Add(move);
                        }
                    }

                }
            }

            if (relevantOnBoardMoves.Count > 0)
                return relevantOnBoardMoves
                    .OrderByDescending(m => ScoreMove(m))
                    .First();

            return null;

        }
    
        // ─── Scoring Heuristic ────────────────────────────────────
        private int ScoreMove(Move move)
        {
            int score = 0;
            var targetCheckers = move.trianglePoint.checkersOnPoint;
            var checkerColor = move.checkerPoint.color;

            if (targetCheckers.Count > 0)
            {
                var checkerOnTarget = targetCheckers[0];
                bool isEnemy = checkerOnTarget.color != checkerColor;

                if (targetCheckers.Count == 1)
                {
                    if (isEnemy)
                        score += 100;  // eat opponent
                    else
                        score += 30;   // create block
                }
                else if (targetCheckers.Count >= 2 && !isEnemy)
                {
                    score += 15;  // reinforce block
                }

                return score;
            }

            // Empty point — escape blot only
            if (move.checkerPoint.parentTriangle != null &&
                move.checkerPoint.parentTriangle.checkersOnPoint.Count == 1)
                score += 20;

            return score;
        }
    }
}