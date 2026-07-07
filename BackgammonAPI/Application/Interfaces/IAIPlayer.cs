using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Application.Interfaces
{
    public interface IAIPlayer
    {
        Task<Move> GetValidMoveAsync(OponentColor oponentColor, List<TrianglePoint> points, GameState instance);
    }
}