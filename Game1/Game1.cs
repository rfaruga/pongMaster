using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game1 {
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game {
        // Score
        private int player1Score = 0;
        private int player2Score = 0;

        // Paddles color
        private bool player1Hit = false;
        private bool player2Hit = false;
        float paddleBrightingCurrentTime;
        float paddleBrightingMaxTime = 300;
        private Color paddle1Color;
        private Color paddle2Color;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D paddle1;
        Texture2D paddle2;
        Texture2D pongBall;
        Texture2D pongBackground;
        Texture2D ballAnim;
        Texture2D explosion64;

        SpriteFont score;

        Point screenSize = new Point(800, 600);
        Point paddleSize = new Point(10, 100);
        Point pongSize = new Point(64, 64);


        // BALL ANIMATION
        int ballOffset = 10;
        private int ballAnimCount = 71;
        List<Point> ballAnimPosition;
        private int currentAnimPosition;

        private float elapsedTimeBallAnimation = 0;
        private float ballAnimTime = 16.6f;
        //

        // EXPLOSION
        List<Point> explosionAnimPosition;
        private int currentExplosionAnimPosition;
        //

        Rectangle paddle1Rectangle;
        Rectangle paddle2Rectangle;
        Rectangle pongRectangle;

        bool startGame = false;
        bool lostGame = false;

        // 1 = RIGHT || -1 = LEFT
        int pongDirect = 1;

        // KIND OF MOVE
        Point up = new Point(2, -3);
        Point upCenter = new Point(3, -2);
        Point center = new Point(4, 0);
        Point downCenter = new Point(3, 2);
        Point down = new Point(2, 3);

        Point currentKindOfMove;

        float pongSpeed = 1;
        float speedIcrease = 1.1f;

        float elapsedTime = 0;
        float moveTime = 10;

        int paddleMoveInPx = 5;
        int pongMoveInPx = 1;

        public Game1() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
        }

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            paddle1 = this.Content.Load<Texture2D>("paddle1");
            paddle2 = this.Content.Load<Texture2D>("paddle2");
            pongBall = this.Content.Load<Texture2D>("pongBall");
            pongBackground = this.Content.Load<Texture2D>("pongBackground");
            ballAnim = this.Content.Load<Texture2D>("ball-anim");
            explosion64 = this.Content.Load<Texture2D>("explosion64");

            score = this.Content.Load<SpriteFont>("score");

            ballAnimPosition = new List<Point>();
            explosionAnimPosition = new List<Point>();

            // Ball animation
            for (int i = 0, count = 0; i < 5; i++) {
                for (int j = 0; j < 16; j++, ++count) {
                    if (count > ballAnimCount) {
                        break;
                    } else {
                        ballAnimPosition.Add(new Point(j * 64, i * 64));
                    }
                }
            }

            // Explosion animation
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    explosionAnimPosition.Add(new Point(j * 64, i * 64));
                }
            }


            // Set default animation element
            currentAnimPosition = 0;

            // Set default explosion element
            currentExplosionAnimPosition = 0;

            paddle1Rectangle = new Rectangle(0, screenSize.Y / 2 - paddleSize.Y / 2, paddleSize.X, paddleSize.Y);
            paddle2Rectangle = new Rectangle(screenSize.X - 10, screenSize.Y / 2 - paddleSize.Y / 2, paddleSize.X, paddleSize.Y);

            //pongRectangle = new Rectangle(screenSize.X / 2 - pongSize.X / 2, screenSize.Y / 2 - pongSize.Y / 2, pongSize.X, pongSize.Y);
            pongRectangle = new Rectangle(10, screenSize.Y / 2 - pongSize.Y / 2, pongSize.X, pongSize.Y);

            ResetToDefaultPositions();

            currentKindOfMove = up;
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            elapsedTime += gameTime.ElapsedGameTime.Milliseconds;

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Space)) {
                startGame = true;
            } else if (keyboardState.IsKeyDown(Keys.Escape)) {
                Exit();
            }

            if (startGame && elapsedTime > moveTime) {
                elapsedTime -= moveTime;
                // PLAYER 1
                if (keyboardState.IsKeyDown(Keys.Q)) {
                    // UP
                    if (paddle1Rectangle.Y > 0) {
                        paddle1Rectangle.Y -= paddleMoveInPx;
                    }
                } else if (keyboardState.IsKeyDown(Keys.A)) {
                    // DOWN
                    if (paddle1Rectangle.Y < screenSize.Y - paddleSize.Y) {
                        paddle1Rectangle.Y += paddleMoveInPx;
                    }
                }
                // PLAYER 2
                if (keyboardState.IsKeyDown(Keys.P)) {
                    // UP
                    if (paddle2Rectangle.Y > 0) {
                        paddle2Rectangle.Y -= paddleMoveInPx;
                    }
                } else if (keyboardState.IsKeyDown(Keys.L)) {
                    // DOWN
                    if (paddle2Rectangle.Y < screenSize.Y - paddleSize.Y) {
                        paddle2Rectangle.Y += paddleMoveInPx;
                    }
                }

                // UPDATE PONG POSITION

                // current position
                var X = pongRectangle.X;
                var Y = pongRectangle.Y;

                pongRectangle.X = X + (int)(pongSpeed * pongMoveInPx * pongDirect * currentKindOfMove.X);
                if (pongDirect == 1) {
                    pongRectangle.Y = Y + (int)(pongSpeed * pongMoveInPx * pongDirect * currentKindOfMove.Y);
                } else {
                    pongRectangle.Y = Y + (int)(pongSpeed * pongMoveInPx * currentKindOfMove.Y);
                }

                if (pongRectangle.Y + ballOffset <= 0) {
                    if (currentKindOfMove == up) currentKindOfMove = down;
                    else if (currentKindOfMove == upCenter) currentKindOfMove = downCenter;
                } else if (pongRectangle.Y >= screenSize.Y - pongSize.Y + ballOffset) {
                    if (currentKindOfMove == down) currentKindOfMove = up;
                    else if (currentKindOfMove == downCenter) currentKindOfMove = upCenter;
                }

                checkCollision();
            }

            elapsedTimeBallAnimation += gameTime.ElapsedGameTime.Milliseconds;

            if (elapsedTimeBallAnimation > ballAnimTime) {
                elapsedTimeBallAnimation -= ballAnimTime;
                if (currentAnimPosition < ballAnimCount - 1) {
                    currentAnimPosition++;
                } else {
                    currentAnimPosition = 0;
                }
            }

            if (lostGame) {
                LostGame();
            }

            if (player1Hit) {
                paddle1Color = Color.Orange;
                paddleBrightingCurrentTime += gameTime.ElapsedGameTime.Milliseconds;
                if (paddleBrightingCurrentTime > paddleBrightingMaxTime) {
                    paddleBrightingCurrentTime = 0;
                    paddle1Color = Color.White;
                    player1Hit = false;
                }
            }

            if (player2Hit) {
                paddle2Color = Color.Orange;
                paddleBrightingCurrentTime += gameTime.ElapsedGameTime.Milliseconds;
                if (paddleBrightingCurrentTime > paddleBrightingMaxTime) {
                    paddleBrightingCurrentTime = 0;
                    paddle2Color = Color.White;
                    player2Hit = false;
                }
            }

            base.Update(gameTime);
        }

        private void ResetToDefaultPositions() {
            paddle1Rectangle = new Rectangle(0, screenSize.Y / 2 - paddleSize.Y / 2, paddleSize.X, paddleSize.Y);
            paddle2Rectangle = new Rectangle(screenSize.X - 10, screenSize.Y / 2 - paddleSize.Y / 2, paddleSize.X, paddleSize.Y);
            pongRectangle = new Rectangle(screenSize.X / 2 - pongSize.X / 2, screenSize.Y / 2 - pongSize.Y / 2, pongSize.X, pongSize.Y);
            pongSpeed = 1;
            pongDirect *= -1;
            lostGame = false;
            paddle1Color = Color.White;
            paddle2Color = Color.White;
        }

        private void checkCollision() {
            // PLAYER 1
            if (pongRectangle.X + ballOffset < paddleSize.X) {
                //if (paddle1Rectangle.Contains(pongRectangle.X, pongRectangle.Y + 20))
                //{
                // SECTOR 1
                var paddleRectSector1 = new Rectangle(paddle1Rectangle.X, paddle1Rectangle.Y - 10, paddle1Rectangle.Width, 20);
                // SECTOR 2
                var paddleRectSector2 = new Rectangle(paddle1Rectangle.X, paddle1Rectangle.Y + 10, paddle1Rectangle.Width, 30);
                // SECTOR 3
                var paddleRectSector3 = new Rectangle(paddle1Rectangle.X, paddle1Rectangle.Y + 40, paddle1Rectangle.Width, 20);
                // SECTOR 4
                var paddleRectSector4 = new Rectangle(paddle1Rectangle.X, paddle1Rectangle.Y + 60, paddle1Rectangle.Width, 30);
                // SECTOR 5
                var paddleRectSector5 = new Rectangle(paddle1Rectangle.X, paddle1Rectangle.Y + 90, paddle1Rectangle.Width, 20);

                if (paddleRectSector1.Contains(5, pongRectangle.Y + 20)) {
                    player1Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = up;
                    pongSpeed *= speedIcrease;
                } else if (paddleRectSector2.Contains(5, pongRectangle.Y + 20)) {
                    player1Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = upCenter;
                    pongSpeed *= speedIcrease;
                } else if (paddleRectSector3.Contains(5, pongRectangle.Y + 20)) {
                    player1Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = center;
                    pongSpeed *= speedIcrease;
                } else if (paddleRectSector4.Contains(5, pongRectangle.Y + 20)) {
                    player1Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = downCenter;
                    pongSpeed *= speedIcrease;
                } else if (paddleRectSector5.Contains(5, pongRectangle.Y + 20)) {
                    player1Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = down;
                    pongSpeed *= speedIcrease;
                }
            }

            // TODO make method with code below
            // Check collision with screen X
            if (pongRectangle.X + ballOffset <= 0) {
                player2Score++;
                startGame = false;
                lostGame = true;
            }

            if (pongRectangle.X + pongSize.X - ballOffset >= screenSize.X) {
                player1Score++;
                startGame = false;
                lostGame = true;
            }

            // PLAYER 2
            else if (pongRectangle.X - ballOffset > screenSize.X - paddleSize.X - pongSize.X) {
                //if (paddle2Rectangle.Contains(pongRectangle.X + 40, pongRectangle.Y + 20))
                //{
                // SECTOR 1
                var paddleRectSector1 = new Rectangle(paddle2Rectangle.X, paddle2Rectangle.Y - 10, paddle2Rectangle.Width, 20);
                // SECTOR 2
                var paddleRectSector2 = new Rectangle(paddle2Rectangle.X, paddle2Rectangle.Y + 10, paddle2Rectangle.Width, 30);
                // SECTOR 3
                var paddleRectSector3 = new Rectangle(paddle2Rectangle.X, paddle2Rectangle.Y + 40, paddle2Rectangle.Width, 20);
                // SECTOR 4
                var paddleRectSector4 = new Rectangle(paddle2Rectangle.X, paddle2Rectangle.Y + 60, paddle2Rectangle.Width, 30);
                // SECTOR 5
                var paddleRectSector5 = new Rectangle(paddle2Rectangle.X, paddle2Rectangle.Y + 90, paddle2Rectangle.Width, 20);

                if (paddleRectSector1.Contains(screenSize.X - 5, pongRectangle.Y + 20)) {
                    player2Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = up;
                    pongSpeed *= speedIcrease;
                }
                // else if (paddleRectSector2.Contains(pongRectangle.X + 40, pongRectangle.Y + 20))
                else if (paddleRectSector2.Contains(screenSize.X - 5, pongRectangle.Y + 20)) {
                    player2Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = upCenter;
                    pongSpeed *= speedIcrease;
                } else if (paddleRectSector3.Contains(screenSize.X - 5, pongRectangle.Y + 20)) {
                    player2Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = center;
                    pongSpeed *= speedIcrease;
                } else if (paddleRectSector4.Contains(screenSize.X - 5, pongRectangle.Y + 20)) {
                    player2Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = downCenter;
                    pongSpeed *= speedIcrease;
                } else if (paddleRectSector5.Contains(screenSize.X - 5, pongRectangle.Y + 20)) {
                    player2Hit = true;
                    pongDirect *= -1;
                    currentKindOfMove = down;
                    pongSpeed *= speedIcrease;
                } else {

                }
            }

            if (pongSpeed > 10) pongSpeed = 10;
        }

        public void LostGame() {
            //ResetToDefaultPositions();
            // Explosion
            currentExplosionAnimPosition++;
            if (currentExplosionAnimPosition > 24) {
                currentExplosionAnimPosition = 0;
                ResetToDefaultPositions();
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            //Window.Title = String.Format("SCORE: Player 1: {0}  /  Player 2: {1}", player1Score, player2Score);
            Window.Title = "PONG MASTER";

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(pongBackground, new Rectangle(0, 0, 800, 600), Color.White);

            spriteBatch.Draw(paddle1, paddle1Rectangle, paddle1Color);

            spriteBatch.Draw(paddle2, paddle2Rectangle, paddle2Color);

            // Ball animation

            if (lostGame == false) {
                var source = new Rectangle(ballAnimPosition[currentAnimPosition].X, ballAnimPosition[currentAnimPosition].Y, 64, 64);
                spriteBatch.Draw(ballAnim, pongRectangle, source, Color.White);
            } else {
                var source = new Rectangle(explosionAnimPosition[currentExplosionAnimPosition].X, explosionAnimPosition[currentExplosionAnimPosition].Y, 64, 64);
                spriteBatch.Draw(explosion64, pongRectangle, source, Color.White);
            }

            // Without ball animation
            //spriteBatch.Draw(pongBall, pongRectangle, Color.White);

            // Score

            var scoreString = String.Format("{0}     {1}", player1Score.ToString("D2"), player2Score.ToString("D2"));

            var scoreSize = score.MeasureString(scoreString);
            var scorePosition = new Vector2(screenSize.X / 2 - scoreSize.X / 2, 20);

            spriteBatch.DrawString(score, scoreString, scorePosition, Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
