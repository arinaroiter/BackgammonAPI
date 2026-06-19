using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;

namespace BackgammonAPI.Application.Interfaces
{
    public interface IGameEngine
    {
        bool MoveCheckerToPoint(CheckerPoint selectedChecker, TrianglePoint selectedTriangle, GameState instance);
        bool AIMoveCheckerToPoint(GameState instance);
    }
}
