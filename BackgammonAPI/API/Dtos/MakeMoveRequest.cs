namespace BackgammonAPI.API.Dtos
{
    public class MakeMoveRequest
    {

        public CheckerPointDto selectedChecker { get; set; }  // triangle index checker is moving FROM
        public TrianglePointDto targetTriangle { get; set; }    // triangle index checker is moving TO
        public string? Color { get; set; } // which checker color is moving - CheckerColor
    }
}
