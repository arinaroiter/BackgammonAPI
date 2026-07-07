using BackgammonAPI.Application.Interfaces;
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
    public class ComputerPlayer : IAIPlayer
    {
        private readonly MoveGenerator _moveGenerator;

        public ComputerPlayer(MoveGenerator moveGenerator)
        {
            _moveGenerator = moveGenerator;
        }

        public Task<Move> GetValidMoveAsync(OponentColor oponentColor, List<TrianglePoint> points, GameState instance)
        {
            List<Move> legalMoves = _moveGenerator.GetLegalMoves(oponentColor, points, instance);

            Move? chosen = legalMoves.Count > 0
               ? legalMoves.OrderByDescending(m => ScoreMove(m)).First()
               : null;

            return Task.FromResult(chosen);
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