using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
        private SpriteBatch sprite_batch;
        public const int entity_size = 16;
        public const int tile_size = 16;
        public const int screen_width = 28;
        public const int screen_height = 36;
        public static float scaling_factor = 1.75f;
        public static int collected_pellets = 0;
        public static bool victory = false;
        public static bool defeat = false;
        private SpriteBatch font_batch;

        // List of valid actions, these can have multiple keys assigned to them
        public enum Button
        {
            [Description("move up")] move_up,
            [Description("move down")] move_down,
            [Description("move left")] move_left,
            [Description("move right")] move_right,
            [Description("reset")] reset
        }

        // Dictionary that determines which keys correspond to which actions
        public static Dictionary<Button, List<Keys>> control_map = new Dictionary<Button, List<Keys>>()
        {
            { Button.move_up, new List<Keys>() { Keys.W, Keys.Up } },
            { Button.move_down, new List<Keys>() { Keys.S, Keys.Down } },
            { Button.move_left, new List<Keys>() { Keys.A, Keys.Left } },
            { Button.move_right, new List<Keys>() { Keys.D, Keys.Right } },
            { Button.reset, new List<Keys>() { Keys.R} }
        };

        // List of valid directions the entities can face or move in
        public enum Direction
        {
            [Description("up")] up,
            [Description("down")] down,
            [Description("left")] left,
            [Description("right")] right
        }

        // Use this to get the opposite direction the entity is facing
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
            IsMouseVisible = true;
            graphics.ApplyChanges();
            TileManager.CreateGameMap(Content);
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sprite_batch = new SpriteBatch(GraphicsDevice);
            font_batch = new SpriteBatch(GraphicsDevice);
            EntityManager.CreateEntities(Content);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime game_time)
        {
            float sf1 = (float)(graphics.PreferredBackBufferHeight) / (tile_size * screen_height);
            float sf2 = (float)(graphics.PreferredBackBufferWidth) / (tile_size * screen_width);
            scaling_factor = Math.Min(sf1, sf2);

            EntityManager.ManageGhostStates(game_time);

            foreach (Entity entity in EntityManager.GetEntityList())
            {
                entity.Move();
            }

            Logic.CheckForWinCondition();

            if (victory || defeat)
            {
                if (Logic.IsButtonPressed(Button.reset))
                {
                    Logic.ResetGame(Content, game_time);
                }
            }

            base.Update(game_time);
        }
        
        protected override void Draw(GameTime game_time)
        {
            GraphicsDevice.Clear(Color.Black);
            sprite_batch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(scaling_factor));
            
            // Draw the tiles
            foreach (Tile tile in TileManager.GetTileList())
            {
                sprite_batch.Draw(tile.Sprite, new Vector2(tile.PosX, tile.PosY), Color.White);
            }

            // Draw the items
            foreach (Item item in TileManager.GetItemList())
            {
                sprite_batch.Draw(item.Sprite, new Vector2(item.PosX, item.PosY), Color.White);
            }

            // Draw the player and ghosts
            foreach (Entity entity in EntityManager.GetEntityList())
            {
                sprite_batch.Draw(entity.Sprite, new Vector2(entity.PosX, entity.PosY), Color.White);
                if (entity is Ghost ghost)
                {
                    sprite_batch.Draw(ghost.TargetSprite, new Vector2(ghost.CurrentTarget.X, ghost.CurrentTarget.Y), Color.White);
                }
            }

            Logic.DrawUI(Content, font_batch, sprite_batch, game_time);
            
            sprite_batch.End();
            base.Draw(game_time);
        }
    }

    public class GameObject
    {
        public int PosX;
        public int PosY;
        public Texture2D Sprite;
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

        public static List<GameObject> FindOverlapsFromList(Entity entity, IEnumerable<GameObject> the_list)
        {
            Vector2 a_top_left = new Vector2(entity.PosX, entity.PosY);
            Vector2 a_bot_right = new Vector2(entity.PosX + Pacman.entity_size, entity.PosY + Pacman.entity_size);

            List<GameObject> current_objects = new List<GameObject>();
            foreach (GameObject obj in the_list)
            {
                Vector2 b_top_left = new Vector2(obj.PosX, obj.PosY);
                Vector2 b_bot_right = new Vector2(obj.PosX + Pacman.tile_size, obj.PosY + Pacman.tile_size);

                if (DoObjectsOverlap(a_top_left, b_top_left, a_bot_right, b_bot_right))
                {
                    current_objects.Add(obj);
                }
            }

            return current_objects;
        }

        public static List<GameObject> FindOverlapsFromList(int pos_x, int pos_y, IEnumerable<GameObject> the_list)
        {
            Vector2 a_top_left = new Vector2(pos_x, pos_y);
            Vector2 a_bot_right = new Vector2(pos_x + Pacman.entity_size, pos_y + Pacman.entity_size);

            List<GameObject> current_objects = new List<GameObject>();
            foreach (GameObject obj in the_list)
            {
                Vector2 b_top_left = new Vector2(obj.PosX, obj.PosY);
                Vector2 b_bot_right = new Vector2(obj.PosX + Pacman.tile_size, obj.PosY + Pacman.tile_size);

                if (DoObjectsOverlap(a_top_left, b_top_left, a_bot_right, b_bot_right))
                {
                    current_objects.Add(obj);
                }
            }

            return current_objects;
        }

        public static bool DoObjectsOverlap(Vector2 tl1, Vector2 tl2, Vector2 br1, Vector2 br2)
        {
            return tl1.X < br2.X && tl2.X < br1.X && tl1.Y < br2.Y && tl2.Y < br1.Y;
        }

        public static void CheckForWinCondition()
        {
            if (TileManager.GetItemList().Count(x => x is SmallPellet) == 0)
            {
                Pacman.victory = true;
            }
        }

        public static void DrawUI(ContentManager content, SpriteBatch font_batch, SpriteBatch sprite_batch, GameTime game_time)
        {
            // Draw the HUD
            SpriteFont ui_font = content.Load<SpriteFont>("ui_font");
            SpriteFont winlose = content.Load<SpriteFont>("winlose");
            Texture2D white_pixel = content.Load<Texture2D>("Sprites/white_pixel");

            int tile_size = Pacman.tile_size;

            font_batch.Begin();

            int remaining_pellets = TileManager.GetItemList().Where(x => x is SmallPellet).Count();
            string timer = game_time.TotalGameTime.ToString(@"mm\:ss");
            font_batch.DrawString(ui_font, $"Remaining Pellets: {remaining_pellets}", new Vector2(2, 2), Color.White);
            font_batch.DrawString(ui_font, $"Timer: {timer}", new Vector2(2, 2*tile_size), Color.White);

            if (Pacman.victory)
            {
                sprite_batch.Draw(white_pixel, new Rectangle(0, 14*tile_size, 28*tile_size, 7*tile_size), Color.White);
                sprite_batch.DrawString(winlose, "Victory!", new Vector2(3*tile_size, 14.5f*tile_size), Color.Green);
                sprite_batch.DrawString(ui_font, "Press 'R' to try again!", new Vector2(5*tile_size, 19*tile_size), Color.Black);
                EntityManager.player.move_speed = 0;
            }

            else if (Pacman.defeat)
            {
                sprite_batch.Draw(white_pixel, new Rectangle(0, 14*tile_size, 28*tile_size, 7*tile_size), Color.White);
                sprite_batch.DrawString(winlose, "Defeat!", new Vector2(4*tile_size, 14.5f*tile_size), Color.Red);
                sprite_batch.DrawString(ui_font, "Press 'R' to try again!", new Vector2(5*tile_size, 19*tile_size), Color.Black);
                EntityManager.player.move_speed = 0;
            }

            font_batch.DrawString(ui_font, $"AI State: {EntityManager.ghost_state.EnumToString()}", new Vector2(2, 60*tile_size), Color.White);
            font_batch.DrawString(ui_font, $"State Timer: {EntityManager.state_timer}", new Vector2(2, 2 + 61*tile_size), Color.White);

            font_batch.End();
        }

        public static void ResetGame(ContentManager content, GameTime game_time)
        {
            TileManager.GetTileList().Clear();
            TileManager.GetItemList().Clear();
            EntityManager.CreateEntities(content);
            TileManager.CreateGameMap(content);
            Pacman.collected_pellets = 0;
            Pacman.victory = false;
            Pacman.defeat = false;
            EntityManager.ghost_state = EntityManager.GhostState.scatter;
            EntityManager.state_timer = 0;
            EntityManager.ResetValues();
            game_time.TotalGameTime = new TimeSpan();
        }
    }
}
