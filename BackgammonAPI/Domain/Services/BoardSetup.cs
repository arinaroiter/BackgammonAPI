using BackgammonAPI.Domain.Aggregates;
using BackgammonAPI.Domain.Entities;
using static BackgammonAPI.Domain.ValueObjects.Constants;

namespace BackgammonAPI.Domain.Services
{
    public static class BoardSetup
    {
        public static void Initialize(GameState state)
        {
            // Step 1 — create 24 triangles, exactly as BuildTrg() does
            state.AllPoints = Enumerable.Range(0, 24)
                .Select(i => new TrianglePoint
                {
                    trianglePointIndex = i,
                    triangleIsUpward = i <= 11   // 0-11 upward, 12-23 downward
                })
                .ToList();

            state.AllCheckers = new List<CheckerPoint>();

            // Step 2 — place checkers, exactly as PlaceCheckers() does
            // Black
            PlaceCheckers(state, CheckerColor.Black, 0, 5);
            PlaceCheckers(state, CheckerColor.Black, 11, 2);
            PlaceCheckers(state, CheckerColor.Black, 16, 3);
            PlaceCheckers(state, CheckerColor.Black, 18, 5);

            // White
            PlaceCheckers(state, CheckerColor.White, 23, 2);
            PlaceCheckers(state, CheckerColor.White, 12, 5);
            PlaceCheckers(state, CheckerColor.White, 6, 5);
            PlaceCheckers(state, CheckerColor.White, 4, 3);
        }

        private static void PlaceCheckers(GameState state, CheckerColor color,
            int pointIndex, int count)
        {
            var triangle = state.AllPoints[pointIndex];

            for (int i = 0; i < count; i++)
            {
                var checker = new CheckerPoint
                {
                    checkerPointIndex = state.AllCheckers.Count, // unique index auto-increment
                    color = color,
                    tagName = color.ToString(),
                    parentTriangle = triangle
                };

                triangle.checkersOnPoint.Add(checker);
                state.AllCheckers.Add(checker);
            }
        }
    }
}