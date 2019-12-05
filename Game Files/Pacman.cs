using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Pacman
{
    public class Pacman : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public const int entity_size = 8;
        public const int tile_size = 8;
        public const int screen_width = 28;
        public const int screen_height = 36;
        public static float scaling_factor = 2f;

        // List of valid actions, these can have multiple keys assigned to them
        public enum Button
        {
            [Description("move up")] move_up,
            [Description("move down")] move_down,
            [Description("move left")] move_left,
            [Description("move right")] move_right
        }

        // Dictionary that determines which keys correspond to which actions
        public static Dictionary<Button, List<Keys>> control_map = new Dictionary<Button, List<Keys>>()
        {
            { Button.move_up, new List<Keys>() { Keys.W, Keys.Up } },
            { Button.move_down, new List<Keys>() { Keys.S, Keys.Down } },
            { Button.move_left, new List<Keys>() { Keys.A, Keys.Left } },
            { Button.move_right, new List<Keys>() { Keys.D, Keys.Right } }
        };

        public enum Direction
        {
            [Description("up")] up,
            [Description("down")] down,
            [Description("left")] left,
            [Description("right")]  right
        }

        public static Dictionary<Direction, Direction> OppositeDir = new Dictionary<Direction, Direction>()
        {
            { Direction.up, Direction.down },
            { Direction.down, Direction.up },
            { Direction.left, Direction.right },
            { Direction.right, Direction.left }
        };

        // Constructor
        public Pacman()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
            graphics.PreferredBackBufferHeight = (int)(tile_size * screen_height * scaling_factor);
            graphics.PreferredBackBufferWidth = (int)(tile_size * screen_width * scaling_factor);
            Window.AllowUserResizing = true;
            graphics.ApplyChanges();
            TileManager.CreateGameMap(Content);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            EntityManager.AssignSprites(Content);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            float sf1 = (float)(graphics.PreferredBackBufferHeight) / (tile_size * screen_height);
            float sf2 = (float)(graphics.PreferredBackBufferWidth) / (tile_size * screen_width);
            scaling_factor = Math.Min(sf1, sf2);

            foreach (Entity entity in EntityManager.GetEntityList())
            {
                entity.Move();
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scaling_factor));
            
            // Draw the tiles
            foreach (Tile tile in TileManager.GetTileList())
            {
                spriteBatch.Draw(tile.Sprite, new Vector2(tile.PosX, tile.PosY), Color.White);
            }

            // Draw the player and ghosts
            foreach (Entity entity in EntityManager.GetEntityList())
            {
                spriteBatch.Draw(entity.Sprite, new Vector2(entity.PosX, entity.PosY), Color.White);
                if (entity is Ghost ghost)
                {
                    spriteBatch.Draw(ghost.TargetSprite, new Vector2(ghost.CurrentTarget.X, ghost.CurrentTarget.Y), Color.White);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }

    public static class Logic
    {
        // Check to see if a specific button is pressed
        public static bool IsButtonPressed(Pacman.Button button)
        {
            return Pacman.control_map[button].Any(x => Keyboard.GetState().IsKeyDown(x));
        }

        public static string EnumToString(this Enum value)
        {
            if (value == null)
            {
                return "none";
            }

            Type type = value.GetType();
            string name = Enum.GetName(value.GetType(), value);

            FieldInfo field = type.GetField(name);
            if (field != null && Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
            {
                return attr.Description;
            }

            return "error";
        }
    }
}
