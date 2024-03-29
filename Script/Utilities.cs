using Microsoft.Xna.Framework;

namespace MonoChess
{
    internal static class Utilities
    {
        internal static Color[] gameColors = new Color[2] { Color.White, Color.Black };

        internal static bool PointInRect(Point p, Rectangle r)
        {
            return p.X > r.X && p.X < r.X + r.Width && p.Y > r.Y && p.Y < r.Y + r.Height;
        }

        internal static Point ArrayToGrid(int i)
        {
            return new Point(i % Main.sqrCount, i / Main.sqrCount);
        }

        internal static int GridToArray(Point p)
        {
            return p.Y * Main.sqrCount + p.X;
        }

        internal static int GridToArray(int x, int y)
        {
            return y * Main.sqrCount + x;
        }

        internal static bool WithinBoard(int x, int y)
        {
            return x >= 0 && x < Main.sqrCount && y >= 0 && y < Main.sqrCount;
        }

        internal static int PlayerIndex(bool p)
        {
            return p ? 0 : 1;
        }

        internal static bool CheckSpace(Space space) //if its empty
        {
            return space.placedPiece == null;
        }

        internal static bool CheckSpace(Space space, bool playerWhite) //empty or occupied by the enemy
        {
            return space.placedPiece == null || space.placedPiece.player != playerWhite;
        }

        internal static string ConvertToLetter(int i)
        {
            switch (i)
            {
                case 0: return "A";
                case 1: return "B";
                case 2: return "C";
                case 3: return "D";
                case 4: return "E";
                case 5: return "F";
                case 6: return "G";
                case 7: return "H";
                default: return "!";
            }
        }

        internal static string ConvertedPos(Point p)
        {
            return ConvertToLetter(p.X) + (Main.sqrCount - p.Y);
        }
    }
}