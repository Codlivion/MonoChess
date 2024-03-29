using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MonoChess
{
    internal class Space
    {
        internal Point location;
        internal Point position;
        internal Rectangle bounds;
        internal Piece placedPiece = null;
        internal bool movable = false;
        internal bool clicked = false;

        internal Space(Point p)
        {
            location = p;
            position = new Point(Main.boardPos.X + p.X * Main.sqrSize, Main.boardPos.Y + p.Y * Main.sqrSize);
            bounds = new Rectangle(position.X, position.Y, Main.sqrSize, Main.sqrSize);
        }
    }

    internal class Player
    {
        internal List<Piece> pieces;

        internal Player()
        {
            pieces = new List<Piece>();
        }

        internal List<Space> MoveSpaces()
        {
            List<Space> result = new List<Space>();
            foreach (Piece piece in pieces)
            {
                if (piece != null)
                {
                    foreach (Space space in piece.MovableSpaceBase())
                    {
                        result.Add(space);
                    }
                }
            }
            return result;
        }

        internal List<Space> ValidMoves()
        {
            List<Space> result = new List<Space>();
            foreach (Space space in pieces[0].MovableSpace())
            {
                result.Add(space);
            }
            for (int i = 1; i < pieces.Count; i++)
            {
                if (pieces[i] != null)
                {
                    foreach (Space space in pieces[i].MovableSpace())
                    {
                        result.Add(space);
                    }
                }
            }
            return result;
        }
    }

    internal class Board
    {
        internal Point position;
        internal Rectangle bounds;
        internal Space[] spaces;
        internal string checkInfo = "";

        internal Board()
        {
            position = Main.boardPos;
            bounds = new Rectangle(position.X, position.Y, Main.sqrSize * Main.sqrCount, Main.sqrSize * Main.sqrCount);
            spaces = new Space[Main.sqrCount * Main.sqrCount];

            for (int i = 0; i < spaces.Length; i++)
            {
                spaces[i] = new Space(Utilities.ArrayToGrid(i));
            }
        }

        internal void ResetPieces()
        {
            Main.players[0].pieces = new List<Piece>();
            Main.players[1].pieces = new List<Piece>();
            foreach (Space space in spaces) space.placedPiece = null;
            AddPiece(new King(true, 0), 60);
            AddPiece(new Queen(true, 1), 59);
            AddPiece(new Rook(true, 2), 56);
            AddPiece(new Rook(true, 3), 63);
            AddPiece(new Knight(true, 4), 57);
            AddPiece(new Knight(true, 5), 62);
            AddPiece(new Bishop(true, 6), 58);
            AddPiece(new Bishop(true, 7), 61);
            for (int i = 48; i < 56; i++) AddPiece(new Pawn(true, i - 40), i);
            AddPiece(new King(false, 0), 4);
            AddPiece(new Queen(false, 1), 3);
            AddPiece(new Rook(false, 2), 0);
            AddPiece(new Rook(false, 3), 7);
            AddPiece(new Knight(false, 4), 1);
            AddPiece(new Knight(false, 5), 6);
            AddPiece(new Bishop(false, 6), 2);
            AddPiece(new Bishop(false, 7), 5);
            for (int i = 8; i < 16; i++) AddPiece(new Pawn(false, i), i);
        }

        internal void AddPiece(Piece p, int i)
        {
            Main.players[Utilities.PlayerIndex(p.player)].pieces.Add(p);
            spaces[i].placedPiece = p;
            p.position = Utilities.ArrayToGrid(i);
        }

        internal void Draw()
        {
            int color;
            Color c;
            Vector2 pos;
            Rectangle sourceRect;
            for (int i = 0; i < spaces.Length; i++)
            {
                color = (i / 8) % 2 == 0 ? i % 2 : (i + 1) % 2;
                c = Main.turnWhite ? Color.Cyan : Color.Magenta;
                if (spaces[i].clicked) DrawRect(spaces[i].bounds.Location, c);
                else if (spaces[i].movable) DrawRect(spaces[i].bounds.Location, c);
                else DrawRect(spaces[i].bounds.Location, Utilities.gameColors[color]);

                if (spaces[i].placedPiece != null)
                {
                    pos = new Vector2(spaces[i].position.X, spaces[i].position.Y);
                    sourceRect = spaces[i].placedPiece.spriteRect;
                    Main.spriteBatch.Draw(Main.pieceSprites, pos, sourceRect, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.1f);
                }
            }
            for (int i = 0; i < 8; i++)
            {
                pos = new Vector2(position.X + i * 64 + 24, position.Y - 24);
                DrawText(pos, Utilities.ConvertToLetter(i), Color.White);
                pos = new Vector2(position.X + i * 64 + 24, position.Y + 512 + 12);
                DrawText(pos, Utilities.ConvertToLetter(i), Color.White);
                pos = new Vector2(position.X - 24, position.Y + i * 64 + 24);
                DrawText(pos, (8 - i).ToString(), Color.White);
                pos = new Vector2(position.X + 512 + 12, position.Y + i * 64 + 24);
                DrawText(pos, (8 - i).ToString(), Color.White);
            }
            pos = new Vector2(Main.infoPos.X - (checkInfo.Length * 4), Main.infoPos.Y + 32);
            c = Main.turnWhite ? Color.Black : Color.White;
            DrawText(pos, checkInfo, c);
            if (!Main.gameOver)
            {
                string turnInfo = Main.turnWhite ? "White's Turn" : "Black's Turn";
                pos = new Vector2(Main.infoPos.X - (turnInfo.Length * 4), Main.infoPos.Y);
                c = Main.turnWhite ? Color.White : Color.Black;
                DrawText(pos, turnInfo, c);
            }

            if (Main.gameOver)
            {
                UIButton button = Main.restart;
                Main.spriteBatch.Draw(Main.debugtxt, button.bounds, Color.LightGray);
                pos = new Vector2(button.bounds.X + 40, button.bounds.Y + 24);
                DrawText(pos, button.text, Color.Black);
            }
            else if (Main.onPromotion)
            {
                foreach (UIButton button in Main.promotions)
                {
                    pos = new Vector2(button.bounds.X, button.bounds.Y);
                    Main.spriteBatch.Draw(Main.pieceSprites, pos, button.sourceRect, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.1f);
                }
            }
            else
            {
                UIButton button = Main.resign;
                Main.spriteBatch.Draw(Main.debugtxt, button.bounds, Color.LightGray);
                pos = new Vector2(button.bounds.X + 40, button.bounds.Y + 24);
                DrawText(pos, button.text, Color.Black);
            }
        }

        internal void DrawRect(Point pos, Color color)
        {
            Main.spriteBatch.Draw(Main.debugtxt, new Rectangle(pos.X, pos.Y, 64, 64), color);
        }

        internal void DrawText(Vector2 pos, string txt, Color color)
        {
            Main.spriteBatch.DrawString(Main.font, txt, pos, color);
        }
    }
}