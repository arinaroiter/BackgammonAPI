using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using BackgammonAPI.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Domain.Services
{
    // Single responsibility: produce the list of legal moves for the AI side.
    // Used by both ComputerPlayer (heuristic) and LLMPlayer (Claude).
    public class MoveGenerator
    {
        private readonly MoveValidator _moveValidator;

        public MoveGenerator(MoveValidator moveValidator)
        {
            _moveValidator = moveValidator;
        }

        public List<Move> GetLegalMoves(OponentColor oponentColor, List<TrianglePoint> points, GameState instance)
        {
            CheckerColor aiColor = Enum.Parse<CheckerColor>(instance.AIColor.ToString());

            // ─── Priority 1 — Bar checker MUST play first ─────────
            var barCheckers = instance.Bars[aiColor].GetGameObjects();
            if (barCheckers.Count > 0)
            {
                List<Move> relevantMovesFromBar = new List<Move>();
                var barChecker = barCheckers[^1];

                var entryPoints = aiColor == CheckerColor.Black
                    ? points
                        .Where(p => p.trianglePointIndex >= 0 && p.trianglePointIndex <= 11)
                        .OrderByDescending(p => p.trianglePointIndex)
                        .ToList()
                    : points
                        .Where(p => p.trianglePointIndex >= 12 && p.trianglePointIndex <= 23)
                        .OrderByDescending(p => p.trianglePointIndex)
                        .ToList();

                foreach (TrianglePoint point in entryPoints)
                {
                    bool isEatMe;
                    if (_moveValidator.IsMoveAllowed(barChecker, point, out isEatMe, instance))
                    {
                        if (_moveValidator.IsMoveInCorrectDirection(
                                barChecker.parentTriangle, point.trianglePointIndex,
                                barChecker, instance, consumeDice: false))
                        {
                            relevantMovesFromBar.Add(new Move
                            {
                                checkerPoint = barChecker,
                                trianglePoint = point
                            });
                        }
                    }
                }

                return relevantMovesFromBar;
            }

            // ─── On-board moves ─────────────────────────────────
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
                        if (_moveValidator.IsMoveInCorrectDirection(
                                topChecker.parentTriangle, somePoint.trianglePointIndex,
                                topChecker, instance, consumeDice: false))
                        {
                            relevantOnBoardMoves.Add(new Move
                            {
                                checkerPoint = topChecker,
                                trianglePoint = somePoint
                            });
                        }
                    }
                }
            }

            return relevantOnBoardMoves;
        }
    }
}