using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using System.IO;

namespace Pacman
{
    public static class EntityManager
    {
        // List of entities
        public static Player player = new Player();
        public static Blinky blinky = new Blinky();
        public static Pinky pinky = new Pinky();
        public static Inky inky = new Inky();
        public static Clyde clyde = new Clyde();

        public static List<Entity> GetEntityList()
        {
            return new List<Entity>() { player, blinky, pinky, inky, clyde };
        }

        public static void AssignSprites(ContentManager content)
        {
            player.Sprite = content.Load<Texture2D>("player");
            blinky.Sprite = content.Load<Texture2D>("blinky");
            pinky.Sprite = content.Load<Texture2D>("pinky");
            inky.Sprite = content.Load<Texture2D>("inky");
            clyde.Sprite = content.Load<Texture2D>("clyde");

            player.PosX = 218;
            player.PosY = 418;
        }
    }

    public abstract class Entity
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public Texture2D Sprite { get; set; }

        public abstract void Move();

        public List<Tile> GetCurrentTiles()
        {
            Vector2 e_top_left = new Vector2(PosX, PosY);
            Vector2 e_bot_right = new Vector2(PosX + Pacman.player_size, PosY + Pacman.player_size);

            List<Tile> current_tiles = new List<Tile>();
            foreach (Tile tile in TileManager.GetTileList())
            {

                Vector2 t_top_left = new Vector2(tile.PosX, tile.PosY);
                Vector2 t_bot_right = new Vector2(tile.PosX + Pacman.tile_size, tile.PosY + Pacman.tile_size);

                foreach (Vector2 point in new List<Vector2>() { e_top_left, e_bot_right })
                {
                    if (DoObjectsOverlap(e_top_left, t_top_left, e_bot_right, t_bot_right))
                    {
                        current_tiles.Add(tile);
                    }
                }
            }

            return current_tiles;
        }

        public bool DoObjectsOverlap(Vector2 tl1, Vector2 tl2, Vector2 br1, Vector2 br2)
        {
            return tl1.X < br2.X && tl2.X < br1.X && tl1.Y < br2.Y && tl2.Y < br1.Y;
        }

        public bool IsWithinNonTraversable()
        {
            return GetCurrentTiles().Any(x => !x.Traversable);
        }

        public bool IsWithinWarp()
        {
            return GetCurrentTiles().Any(x => x is Warp);
        }
    }

    public sealed class Player : Entity
    {
        public override void Move()
        {
            const int delta_move = 2;

            if (Logic.IsOnlyOneDirectionPressed())
            {
                if (Logic.IsButtonPressed(Pacman.Button.move_up))
                {
                    PosY -= delta_move;
                    if (IsWithinNonTraversable())
                    {
                        PosY += delta_move;
                    }
                }

                else if (Logic.IsButtonPressed(Pacman.Button.move_down))
                {
                    PosY += delta_move;
                    if (IsWithinNonTraversable())
                    {
                        PosY -= delta_move;
                    }
                }

                else if (Logic.IsButtonPressed(Pacman.Button.move_left))
                {
                   PosX -= delta_move;
                    if (IsWithinNonTraversable())
                    {
                        PosX += delta_move;
                    }
                }

                else if (Logic.IsButtonPressed(Pacman.Button.move_right))
                {
                    PosX += delta_move;
                    if (IsWithinNonTraversable())
                    {
                        PosX -= delta_move;
                    }
                }

                else
                {
                    return;
                }

                if (IsWithinWarp())
                {
                    Warp the_warp = GetCurrentTiles().First(x => x is Warp) as Warp;
                    Warp matching_warp = TileManager.FindMatchingWarp(the_warp);
                    if (Logic.IsButtonPressed(Pacman.Button.move_up))
                    {
                        PosX = matching_warp.PosX;
                        PosY = matching_warp.PosY - Pacman.tile_size;
                    }

                    if (Logic.IsButtonPressed(Pacman.Button.move_down))
                    {
                        PosX = matching_warp.PosX;
                        PosY = matching_warp.PosY + Pacman.tile_size;
                    }
                    if (Logic.IsButtonPressed(Pacman.Button.move_left))
                    {
                        PosX = matching_warp.PosX - Pacman.tile_size;
                        PosY = matching_warp.PosY;
                    }

                    if (Logic.IsButtonPressed(Pacman.Button.move_right))
                    {
                        PosX = matching_warp.PosX + Pacman.tile_size;
                        PosY = matching_warp.PosY;
                    }
                }
            }
        }
    }

    public abstract class Ghost : Entity
    {
        public override void Move()
        {

        }
    }

    public sealed class Blinky : Ghost
    {

    }

    public sealed class Pinky : Ghost
    {

    }

    public sealed class Inky : Ghost
    {

    }

    public sealed class Clyde : Ghost
    {

    }
}
