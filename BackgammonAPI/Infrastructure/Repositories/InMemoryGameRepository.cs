using BackgammonAPI.Application.Interfaces;
using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Infrastructure.Repositories
{
    public class InMemoryGameRepository : IGameRepository
    {
        private readonly IMemoryCache _cache;

        public InMemoryGameRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public GameState GetGame(string gameId)
        {
            return _cache.Get<GameState>(gameId);
        }

        public void SaveGame(GameState state)
            => _cache.Set(state.GameId.ToString(), state,
                TimeSpan.FromHours(2));

        public void DeleteGame(string gameId)
            => _cache.Remove(gameId);
    }
}