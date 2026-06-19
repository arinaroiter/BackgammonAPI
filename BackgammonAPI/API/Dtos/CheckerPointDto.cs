namespace BackgammonAPI.API.Dtos
{
    public class CheckerPointDto
    {
        public int CheckerPointIndex { get; set; }  // checker's position index
        public string Color { get; set; }            // "Black" or "White"
        public string TagName { get; set; }          // used for eat validation
        public int ParentTriangleIndex { get; set; }
    }
}
