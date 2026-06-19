using BackgammonAPI.Domain.Aggregates;

namespace BackgammonAPI.Application.Interfaces
{

    public interface IGameRepository
    {
        GameState GetGame(string gameId);
        void SaveGame(GameState state);
        void DeleteGame(string gameId);
    }

}
