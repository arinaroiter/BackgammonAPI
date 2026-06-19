namespace BackgammonAPI.API.Dtos
{
    public class TrianglePointDto
    {
        public int Index { get; set; }
        public List<CheckerPointDto> Checkers { get; set; } // "Black" or "White" per checker
        public bool IsUpward { get; internal set; }
    }
}
