using BackgammonAPI.API.Dtos;
using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.API.Mappers
{
    public class GameMapper
    {
        // ─── ToDto — GameState → DTO (send to Unity) ──────────────
        public GameStateDto ToDto(GameState state)
        {
            return new GameStateDto
            {
                GameId = state.GameId.ToString(),
                CurrentPlayerColor = state.CurrentPlayerColor.ToString(),
                Status = state.Status.ToString(),
                Die1 = state.Die1,
                Die2 = state.Die2,
                IsGameStarted = state.IsGameStarted,
                IsVsAI = state.IsVsAI,
                AIColor = state.AIColor.ToString(),
                TotalDiceRemaining = state.TotalDiceRemaining,
                MyCurrentOponent = state.MyCurrentOponent.ToString(),
                AllCheckers = state.AllCheckers
                                 .Select(MapCheckerToDTO)
                                 .ToList(),

                Points = state.AllPoints
                                         .Select(MapTriangleToDTO)
                                         .ToList(),
                BlackBarCheckers = state.Bars[CheckerColor.Black]
                                         .GetGameObjects()
                                         .Select(MapCheckerToDTO)
                                         .ToList(),
                WhiteBarCheckers = state.Bars[CheckerColor.White]
                                         .GetGameObjects()
                                         .Select(MapCheckerToDTO)
                                         .ToList()
            };
        }

        // ─── FromDto — MakeMoveRequest → domain objects ────────────
        public (CheckerPoint checker, TrianglePoint triangle) FromDto(
            GameState state, MakeMoveRequest request)
        {

            var checker = FindChecker(state,
                request.selectedChecker.CheckerPointIndex);

            var triangle = state.AllPoints
                .FirstOrDefault(p => p.trianglePointIndex ==
                    request.targetTriangle.Index);

            return (checker, triangle);
        }


        public GameState FromDto(GameStateDto dto)
        {
            var state = new GameState
            {
                GameId = Guid.Parse(dto.GameId),
                CurrentPlayerColor = Enum.Parse<CurrentPlayerColor>(dto.CurrentPlayerColor),
                Status = Enum.Parse<GameStatus>(dto.Status),
                Die1 = dto.Die1,
                Die2 = dto.Die2,
                TotalDiceRemaining = dto.TotalDiceRemaining,
                IsVsAI = dto.IsVsAI,
                AIColor = Enum.Parse<OponentColor>(dto.AIColor),
                MyCurrentOponent = Enum.Parse<OponentTypes>(dto.MyCurrentOponent)
            };

            // Step 1 — restore Points (triangles without checkers first)
            state.AllPoints = dto.Points
                .Select(p => new TrianglePoint
                {
                    trianglePointIndex = p.Index,
                    triangleIsUpward = p.IsUpward
                })
                .ToList();

            // Step 2 — restore ALL checkers with ParentTriangle reference
            state.AllCheckers = dto.AllCheckers
                .Select(c => MapCheckerFromDTO(c, state.AllPoints))
                .ToList();

            // Step 3 — restore each triangle's checker list
            foreach (var triangleDto in dto.Points)
            {
                var triangle = state.AllPoints
                    .First(p => p.trianglePointIndex == triangleDto.Index);

                triangle.checkersOnPoint = triangleDto.Checkers
                    .Select(c => state.AllCheckers
                        .First(ch => ch.checkerPointIndex == c.CheckerPointIndex))
                    .ToList();
            }

            // Step 4 — restore Black bar
            foreach (var checkerDto in dto.BlackBarCheckers)
            {
                var checker = state.AllCheckers
                    .First(c => c.checkerPointIndex == checkerDto.CheckerPointIndex);
                state.Bars[CheckerColor.Black].AddGameObject(checker);
            }

            // Step 5 — restore White bar
            foreach (var checkerDto in dto.WhiteBarCheckers)
            {
                var checker = state.AllCheckers
                    .First(c => c.checkerPointIndex == checkerDto.CheckerPointIndex);
                state.Bars[CheckerColor.White].AddGameObject(checker);
            }

            return state;
        }

        private CheckerPoint MapCheckerFromDTO(CheckerPointDto dto,
            List<TrianglePoint> points)
        {
            return new CheckerPoint
            {
                checkerPointIndex = dto.CheckerPointIndex,
                color = Enum.Parse<CheckerColor>(dto.Color),
                tagName = dto.TagName,
                // Restore ParentTriangle reference — null if on bar (index -1)
                parentTriangle = dto.ParentTriangleIndex >= 0
                    ? points.FirstOrDefault(p =>
                        p.trianglePointIndex == dto.ParentTriangleIndex)
                    : null
            };
        }

        // ─── Private Helpers ───────────────────────────────────────
        private CheckerPoint FindChecker(GameState state, int index)
        {
            // Search board first
            var checker = state.AllCheckers
                .FirstOrDefault(c => c.checkerPointIndex == index);

            // Search bars if not found
            if (checker == null)
                checker = state.Bars[CheckerColor.Black]
                    .GetGameObjects()
                    .Concat(state.Bars[CheckerColor.White].GetGameObjects())
                    .FirstOrDefault(c => c.checkerPointIndex == index);

            return checker;
        }

        private TrianglePointDto MapTriangleToDTO(TrianglePoint t) => new TrianglePointDto
        {
            Index = t.trianglePointIndex,
            IsUpward = t.triangleIsUpward,
            Checkers = t.checkersOnPoint.Select(MapCheckerToDTO).ToList()
        };

        private CheckerPointDto MapCheckerToDTO(CheckerPoint c) => new CheckerPointDto
        {
            CheckerPointIndex = c.checkerPointIndex,
            Color = c.color.ToString(),
            TagName = c.tagName,
            ParentTriangleIndex = c.parentTriangle?.trianglePointIndex ?? -1
        };
    }
}