using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace Pacman
{
    public class Pacman : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public int pos_x = 0;
        public int pos_y = 0;
        public int delta_move = 2;

        private Texture2D p_sprite;

        public enum Button
        {
            move_up,
            move_down,
            move_left,
            move_right
        } 

        public static Dictionary<Button, List<Keys>> control_map = new Dictionary<Button, List<Keys>>()
        {
            { Button.move_up, new List<Keys>() { Keys.W, Keys.Up } },
            { Button.move_down, new List<Keys>() { Keys.S, Keys.Down } },
            { Button.move_left, new List<Keys>() { Keys.A, Keys.Left } },
            { Button.move_right, new List<Keys>() { Keys.D, Keys.Right } }
        };

        public Pacman()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            p_sprite = Content.Load<Texture2D>("player");

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
           
            if (CMethods.IsOnlyOneDirectionPressed())
            {
                if (CMethods.IsButtonPressed(Button.move_up))
                {
                    pos_y -= delta_move;
                }

                if (CMethods.IsButtonPressed(Button.move_down))
                {
                    pos_y += delta_move;
                }

                if (CMethods.IsButtonPressed(Button.move_left))
                {
                    pos_x -= delta_move;
                }

                if (CMethods.IsButtonPressed(Button.move_right))
                {
                    pos_x += delta_move;
                }
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(p_sprite, new Vector2(pos_x, pos_y), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public static class CMethods
    {
        // Check to see if a specific button is pressed
        public static bool IsButtonPressed(Pacman.Button button)
        {
            return Pacman.control_map[button].Any(x => Keyboard.GetState().IsKeyDown(x));
        }

        // Make sure only one direction is active at a time
        public static bool IsOnlyOneDirectionPressed()
        {
            List<Pacman.Button> directions = new List<Pacman.Button>()
            {
                Pacman.Button.move_up,
                Pacman.Button.move_down,
                Pacman.Button.move_left,
                Pacman.Button.move_right
            };

            return directions.Count(x => IsButtonPressed(x)) == 1;
        }
    }
}
