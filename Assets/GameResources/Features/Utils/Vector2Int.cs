namespace GameResources.Features.Utils
{
    public struct Vector2Int
    {
        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2Int zero = new Vector2Int(0, 0);

        public int X;
        public int Y;
    }
}