using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace MonoChess
{
    internal class Main : Game
    {
        internal static GraphicsDeviceManager graphics;
        internal static SpriteBatch spriteBatch;

        internal static Board gameBoard;
        internal static Point boardPos = new Point(256, 128);
        internal static Point infoPos = new Point(512, 16);
        internal static int sqrSize = 64;
        internal static int sqrCount = 8;

        internal static Player[] players;
        internal static bool moveMade;
        internal static bool onPromotion;
        internal static bool gameOver;
        internal static bool turnWhite;
        internal static int turnIndex => turnWhite ? 0 : 1;

        MouseState mouseState;
        MouseState prevMouseState;
        int movingIndex = -1;
        int lastMovedIndex;

        internal static Texture2D debugtxt;
        internal static SpriteFont font;
        internal static Texture2D pieceSprites;

        internal static UIButton restart;
        internal static UIButton resign;
        internal static UIButton[] promotions;

        internal Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            gameBoard = new Board();
            players = new Player[2] { new Player(), new Player() };
            debugtxt = new Texture2D(GraphicsDevice, 1, 1);
            debugtxt.SetData(new Color[] { Color.White });
            font = Content.Load<SpriteFont>("Content/Font/ChessFont");
            pieceSprites = Content.Load<Texture2D>("Content/Graphics/ChessPieces");

            restart = new UIButton(new Rectangle(448, 688, 128, 64), new Rectangle(), "Restart");
            resign = new UIButton(new Rectangle(448, 688, 128, 64), new Rectangle(), "Resign");
            promotions = new UIButton[4]
                {
                  new UIButton(new Rectangle(384, 688, 64, 64), new Rectangle(64, 0, 64, 64), ""),
                  new UIButton(new Rectangle(448, 688, 64, 64), new Rectangle(256, 0, 64, 64), ""),
                  new UIButton(new Rectangle(512, 688, 64, 64), new Rectangle(192, 0, 64, 64), ""),
                  new UIButton(new Rectangle(578, 688, 64, 64), new Rectangle(128, 0, 64, 64), "")
                };

            gameBoard.ResetPieces();
            turnWhite = true;
        }

        private void EndTurn()
        {
            movingIndex = -1;
            lastMovedIndex = -1;
            turnWhite = !turnWhite;
            CheckState();
        }

        private void PromotePawn(Piece pawn, Piece promotion)
        {
            Point pos = pawn.position;
            int i = pawn.pieceIndex;
            players[turnIndex].pieces[i] = promotion;
            players[turnIndex].pieces[i].position = pos;
            int spaceIndex = Utilities.GridToArray(pos);
            gameBoard.spaces[spaceIndex].placedPiece = players[turnIndex].pieces[i];
            gameBoard.spaces[spaceIndex].clicked = false;
        }

        private void CheckState()
        {
            List<Space> enemySight = players[Utilities.PlayerIndex(!turnWhite)].MoveSpaces();
            int kingPos = Utilities.GridToArray(players[turnIndex].pieces[0].position);
            bool inCheck = enemySight.Contains(gameBoard.spaces[kingPos]);
            bool canMove = players[turnIndex].ValidMoves().Count > 0;
            if (inCheck)
            {
                string playerColor = turnWhite ? "White" : "Black";
                string enemyColor = Utilities.PlayerIndex(!turnWhite) == 0 ? "White" : "Black";
                if (canMove) { gameBoard.checkInfo = enemyColor + " Checks " + playerColor + " King!"; }
                else
                {
                    gameBoard.checkInfo = enemyColor + " Checkmates " + playerColor;
                    gameOver = true;
                }
            }
            else if (!canMove)
            {
                gameBoard.checkInfo = "Game Is A Draw!";
                gameOver = true;
            }
            else gameBoard.checkInfo = "";
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            mouseState = Mouse.GetState();
            
            bool rectFound = false;
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed)
            {
                foreach (Space space in gameBoard.spaces) space.movable = false;

                if (gameOver)
                {
                    if (Utilities.PointInRect(mouseState.Position, restart.bounds))
                    {
                        gameOver = false;
                        gameBoard.checkInfo = "";
                        gameBoard.ResetPieces();
                        turnWhite = true;
                    }
                }
                else if (onPromotion)
                {
                    for (int i = 0; i < promotions.Length; i++)
                    {
                        if (Utilities.PointInRect(mouseState.Position, promotions[i].bounds))
                        {
                            Piece pawn = players[turnIndex].pieces[lastMovedIndex];
                            switch (i)
                            {
                                case 0:
                                    PromotePawn(pawn, new Queen(turnWhite, pawn.pieceIndex));
                                    break;
                                case 1:
                                    PromotePawn(pawn, new Rook(turnWhite, pawn.pieceIndex));
                                    break;
                                case 2:
                                    PromotePawn(pawn, new Knight(turnWhite, pawn.pieceIndex));
                                    break;
                                case 3:
                                    PromotePawn(pawn, new Bishop(turnWhite, pawn.pieceIndex));
                                    break;
                                default: break;
                            }
                            onPromotion = false;
                            EndTurn();
                            break;
                        }
                    }
                }
                else
                {
                    if (Utilities.PointInRect(mouseState.Position, resign.bounds))
                    {
                        //Confirm? =>
                        gameOver = true;
                        string playerColor = turnWhite ? "White" : "Black";
                        gameBoard.checkInfo = playerColor + " Resigned!";
                    }

                    if (movingIndex != -1)
                    {
                        Piece selected = gameBoard.spaces[movingIndex].placedPiece;
                        Space space;
                        for (int i = 0; i < gameBoard.spaces.Count(); i++)
                        {
                            space = gameBoard.spaces[i];
                            if (selected.name == "King")
                            {
                                King king = selected as King;
                                for (int r = 2; r <= 3; r++)
                                    if (king.CastleCheck().Contains((space, r)) && Utilities.PointInRect(mouseState.Position, space.bounds))
                                    {
                                        king.Castle(Utilities.GridToArray(selected.position.X, selected.position.Y), i, r);
                                        EndTurn();
                                    }
                            }
                            if (selected.MovableSpace().Contains(space) && Utilities.PointInRect(mouseState.Position, space.bounds))
                            {
                                selected.Move(Utilities.GridToArray(selected.position.X, selected.position.Y), i);
                                if (!onPromotion) EndTurn();
                            }
                            else space.clicked = false;
                        }
                    }

                    if (Utilities.PointInRect(mouseState.Position, gameBoard.bounds))
                    {
                        Space space;
                        for (int i = 0; i < gameBoard.spaces.Count(); i++)
                        {
                            space = gameBoard.spaces[i];
                            if (Utilities.PointInRect(mouseState.Position, space.bounds) && space.placedPiece != null && space.placedPiece.player == turnWhite)
                            {
                                movingIndex = i;
                                rectFound = true;
                                space.clicked = true;
                                lastMovedIndex = space.placedPiece.pieceIndex;
                            }
                            else space.clicked = false;
                        }
                    }

                    if (!rectFound) movingIndex = -1;
                }
            }

            prevMouseState = Mouse.GetState();

            if (movingIndex != -1)
            {
                Piece selected = gameBoard.spaces[movingIndex].placedPiece;
                if (selected.name == "King")
                {
                    King king = selected as King;
                    foreach ((Space, int) pair in king.CastleCheck()) pair.Item1.movable = true;
                }
                foreach (Space space in selected.MovableSpace()) space.movable = true;
            }
            else
            {
                foreach (Space space in gameBoard.spaces) space.movable = false;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.BurlyWood);

            spriteBatch.Begin();
            gameBoard.Draw();
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    internal struct UIButton
    {
        internal Rectangle bounds;
        internal Rectangle sourceRect;
        internal string text;
        internal bool active;

        internal UIButton(Rectangle rect, Rectangle source, string t)
        {
            bounds = rect;
            sourceRect = source;
            text = t;
            active = false;
        }
    }
}