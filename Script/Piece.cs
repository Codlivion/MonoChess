using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MonoChess
{
    internal class Piece
    {
        internal bool player;
        internal string name;
        internal int pieceIndex;
        internal Point position;
        internal Rectangle spriteRect;
        internal Point[] direction;
        internal int moveRange;
        internal bool firstMove;

        internal Piece() { }

        internal virtual List<Space> MovableSpaceBase()
        {
            int i = Utilities.GridToArray(position.X, position.Y);
            List<Space> result = new List<Space>();
            Space space;
            Point pos = Utilities.ArrayToGrid(i);
            Point next;
            foreach (Point dir in direction)
            {
                next = pos;
                for (int c = 0; c < moveRange; c++)
                {
                    next += dir;
                    if (Utilities.WithinBoard(next.X, next.Y))
                    {
                        space = Main.gameBoard.spaces[Utilities.GridToArray(next.X, next.Y)];
                        if (Utilities.CheckSpace(space, player))
                        {
                            result.Add(space);
                            if (!Utilities.CheckSpace(space)) break;
                        }
                        else break;
                    }
                    else break;
                }
            }
            return result;
        }

        internal virtual List<Space> MovableSpace()
        {
            int i = Utilities.GridToArray(position.X, position.Y);
            List<Space> result = new List<Space>();
            Space space;
            Point pos = Utilities.ArrayToGrid(i);
            Point next;
            Piece removed;
            List<Space> enemySight;
            foreach (Point dir in direction)
            {
                next = pos;
                for (int c = 0; c < moveRange; c++)
                {
                    next += dir;
                    if (Utilities.WithinBoard(next.X, next.Y))
                    {
                        space = Main.gameBoard.spaces[Utilities.GridToArray(next.X, next.Y)];
                        if (Utilities.CheckSpace(space, player))
                        {
                            removed = MoveCheck(Utilities.GridToArray(pos.X, pos.Y), Utilities.GridToArray(next.X, next.Y));
                            enemySight = Main.players[Utilities.PlayerIndex(!player)].MoveSpaces();
                            int kingPos = Utilities.GridToArray(Main.players[Utilities.PlayerIndex(player)].pieces[0].position);
                            if (!enemySight.Contains(Main.gameBoard.spaces[kingPos])) result.Add(space);
                            MoveReverse(removed, Utilities.GridToArray(next.X, next.Y), Utilities.GridToArray(pos.X, pos.Y));
                            if (!Utilities.CheckSpace(space)) break;
                        }
                        else break;
                    }
                    else break;
                }
            }
            return result;
        }

        internal virtual void Move(int from, int to)
        {
            if (firstMove) firstMove = false;
            Main.gameBoard.spaces[from].placedPiece = null;
            if (Main.gameBoard.spaces[to].placedPiece != null && Main.gameBoard.spaces[to].placedPiece.player == !player)
            {
                int i = Main.gameBoard.spaces[to].placedPiece.pieceIndex;
                Main.players[Utilities.PlayerIndex(!player)].pieces[i] = null;
            }
            Main.gameBoard.spaces[to].placedPiece = this;
            position = Utilities.ArrayToGrid(to);
        }

        internal Piece MoveCheck(int from, int to)
        {
            Piece removed = null;
            Main.gameBoard.spaces[from].placedPiece = null;
            if (Main.gameBoard.spaces[to].placedPiece != null) removed = Main.gameBoard.spaces[to].placedPiece;
            if (removed != null)
            {
                int i = removed.pieceIndex;
                Main.players[Utilities.PlayerIndex(!player)].pieces[i] = null;
            }
            Main.gameBoard.spaces[to].placedPiece = this;
            position = Utilities.ArrayToGrid(to);
            return removed;
        }

        internal void MoveReverse(Piece removed, int from, int to)
        {
            Main.gameBoard.spaces[from].placedPiece = removed;
            if (removed != null)
            {
                Main.players[Utilities.PlayerIndex(!player)].pieces[removed.pieceIndex] = removed;
            }
            Main.gameBoard.spaces[to].placedPiece = this;
            position = Utilities.ArrayToGrid(to);
        }
    }

    internal class King : Piece
    {
        internal King(bool p, int i)
        {
            player = p;
            name = "King";
            pieceIndex = i;
            spriteRect = p ? new Rectangle(0, 0, 64, 64) : new Rectangle(0, 64, 64, 64);
            direction = new Point[8]
            {
                new Point(-1, -1),  new Point(0, -1), new Point(1, -1), new Point(-1, 0),
                new Point(1, 0), new Point(-1, 1), new Point(0, 1), new Point(1, 1)
            };
            moveRange = 1;
            firstMove = true;
        }

        internal List<(Space, int)> CastleCheck()
        {
            List<(Space, int)> result = new List<(Space, int)>();
            if (firstMove)
            {
                int i = Utilities.GridToArray(position.X, position.Y);
                List<Space> enemySight;
                Piece rook;
                enemySight = Main.players[Utilities.PlayerIndex(!player)].MoveSpaces();
                if (enemySight.Contains(Main.gameBoard.spaces[Utilities.GridToArray(position)])) return result;
                rook = Main.players[Utilities.PlayerIndex(player)].pieces[2];
                if (rook != null && rook.firstMove)
                {
                    bool clear = true;
                    for (int c = 1; c <= 3; c++)
                    {
                        if (Main.gameBoard.spaces[i - c].placedPiece != null || enemySight.Contains(Main.gameBoard.spaces[i - c]))
                        {
                            clear = false;
                            break;
                        }
                    }
                    if (clear) result.Add((Main.gameBoard.spaces[i - 2], 2));
                }
                rook = Main.players[Utilities.PlayerIndex(player)].pieces[3];
                if (rook != null && rook.firstMove)
                {
                    bool clear = true;
                    for (int c = 1; c <= 2; c++)
                    {
                        if (Main.gameBoard.spaces[i + c].placedPiece != null || enemySight.Contains(Main.gameBoard.spaces[i + c]))
                        {
                            clear = false;
                            break;
                        }
                    }
                    if (clear) result.Add((Main.gameBoard.spaces[i + 2], 3));
                }
            }
            return result;
        }

        internal void Castle(int from, int to, int rookIndex)
        {
            Piece rook = Main.players[Utilities.PlayerIndex(player)].pieces[rookIndex];
            if (rook == null) return;
            int m = rookIndex == 2 ? 3 : rookIndex == 3 ? -2 : 0;
            int rookPos = Utilities.GridToArray(rook.position.X, rook.position.Y);
            rook.Move(rookPos, rookPos + m);
            Move(from, to);
        }
    }

    internal class Queen : Piece
    {
        internal Queen(bool p, int i)
        {
            player = p;
            name = "Queen";
            pieceIndex = i;
            spriteRect = p ? new Rectangle(64, 0, 64, 64) : new Rectangle(64, 64, 64, 64);
            direction = new Point[8]
            {
                new Point(-1, -1),  new Point(0, -1), new Point(1, -1), new Point(-1, 0),
                new Point(1, 0), new Point(-1, 1), new Point(0, 1), new Point(1, 1)
            };
            moveRange = 8;
            firstMove = true;
        }
    }

    internal class Rook : Piece
    {
        internal Rook(bool p, int i)
        {
            player = p;
            name = "Rook";
            pieceIndex = i;
            spriteRect = p ? new Rectangle(256, 0, 64, 64) : new Rectangle(256, 64, 64, 64);
            direction = new Point[4] { new Point(0, -1),  new Point(0, 1), new Point(-1, 0), new Point(1, 0) };
            moveRange = 8;
            firstMove = true;
        }
    }

    internal class Knight : Piece
    {
        internal Knight(bool p, int i)
        {
            player = p;
            name = "Knight";
            pieceIndex = i;
            spriteRect = p ? new Rectangle(192, 0, 64, 64) : new Rectangle(192, 64, 64, 64);
            direction = new Point[8]
            {
                new Point(-1, -2),  new Point(1, -2), new Point(-1, 2), new Point(1, 2),
                new Point(-2, -1), new Point(-2, 1), new Point(2, -1), new Point(2, 1)
            };
            moveRange = 1;
            firstMove = true;
        }
    }

    internal class Bishop : Piece
    {
        internal Bishop(bool p, int i)
        {
            player = p;
            name = "Bishop";
            pieceIndex = i;
            spriteRect = p ? new Rectangle(128, 0, 64, 64) : new Rectangle(128, 64, 64, 64);
            direction = new Point[4] { new Point(-1, -1), new Point(1, -1), new Point(-1, 1), new Point(1, 1) };
            moveRange = 8;
            firstMove = true;
        }
    }

    internal class Pawn : Piece
    {
        internal Pawn(bool p, int i)
        {
            player = p;
            name = "Pawn";
            pieceIndex = i;
            spriteRect = p ? new Rectangle(320, 0, 64, 64) : new Rectangle(320, 64, 64, 64);
            direction = new Point[1] { new Point(0, p ? -1 : 1) };
            moveRange = 1;
            firstMove = true;
        }

        internal override List<Space> MovableSpaceBase()
        {
            int i = Utilities.GridToArray(position.X, position.Y);
            List<Space> result = new List<Space>();
            Space space;
            Point pos = Utilities.ArrayToGrid(i);
            Point[] diags = new Point[2] { pos + new Point(-1, direction[0].Y), pos + new Point(1, direction[0].Y) };
            foreach (Point diag in diags)
            {
                if (Utilities.WithinBoard(diag.X, diag.Y))
                {
                    space = Main.gameBoard.spaces[Utilities.GridToArray(diag.X, diag.Y)];
                    if (Utilities.CheckSpace(space, player) && !Utilities.CheckSpace(space)) result.Add(space);
                }
            }
            return result;
        }

        internal override List<Space> MovableSpace()
        {
            int i = Utilities.GridToArray(position.X, position.Y);
            List<Space> result = new List<Space>();
            Space space;
            Point pos = Utilities.ArrayToGrid(i);
            Point next;
            Piece removed;
            List<Space> enemySight;
            int count = firstMove ? moveRange + 1 : moveRange;
            foreach (Point dir in direction)
            {
                next = pos;
                for (int c = 0; c < count; c++)
                {
                    next += dir;
                    if (Utilities.WithinBoard(next.X, next.Y))
                    {
                        space = Main.gameBoard.spaces[Utilities.GridToArray(next.X, next.Y)];
                        if (Utilities.CheckSpace(space, player))
                        {
                            if (Utilities.CheckSpace(space))
                            {
                                removed = MoveCheck(Utilities.GridToArray(pos.X, pos.Y), Utilities.GridToArray(next.X, next.Y));
                                enemySight = Main.players[Utilities.PlayerIndex(!player)].MoveSpaces();
                                int kingPos = Utilities.GridToArray(Main.players[Utilities.PlayerIndex(player)].pieces[0].position);
                                if (!enemySight.Contains(Main.gameBoard.spaces[kingPos])) result.Add(space);
                                MoveReverse(removed, Utilities.GridToArray(next.X, next.Y), Utilities.GridToArray(pos.X, pos.Y));
                            }
                            else break;
                        }
                        else break;
                    }
                    else break;
                }
            }
            Point[] diags = new Point[2] { pos + new Point(-1, direction[0].Y), pos + new Point(1, direction[0].Y) };
            foreach (Point diag in diags)
            {
                if (Utilities.WithinBoard(diag.X, diag.Y))
                {
                    space = Main.gameBoard.spaces[Utilities.GridToArray(diag.X, diag.Y)];
                    if (Utilities.CheckSpace(space, player) && !Utilities.CheckSpace(space))
                    {
                        removed = MoveCheck(Utilities.GridToArray(pos.X, pos.Y), Utilities.GridToArray(diag.X, diag.Y));
                        enemySight = Main.players[Utilities.PlayerIndex(!player)].MoveSpaces();
                        int kingPos = Utilities.GridToArray(Main.players[Utilities.PlayerIndex(player)].pieces[0].position);
                        if (!enemySight.Contains(Main.gameBoard.spaces[kingPos])) result.Add(space);
                        MoveReverse(removed, Utilities.GridToArray(diag.X, diag.Y), Utilities.GridToArray(pos.X, pos.Y));
                    }
                }
            }
            return result;
        }

        internal override void Move(int from, int to)
        {
            if (firstMove) firstMove = false;
            Main.gameBoard.spaces[from].placedPiece = null;
            if (Main.gameBoard.spaces[to].placedPiece != null && Main.gameBoard.spaces[to].placedPiece.player == !player)
            {
                int i = Main.gameBoard.spaces[to].placedPiece.pieceIndex;
                Main.players[Utilities.PlayerIndex(!player)].pieces[i] = null;
            }
            Main.gameBoard.spaces[to].placedPiece = this;
            position = Utilities.ArrayToGrid(to);
            if (position.Y == 0 || position.Y == 7) Main.onPromotion = true;
        }
    }
}