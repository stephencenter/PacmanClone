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

            player.PosX = (int)(13.3*Pacman.tile_size);
            player.PosY = 26*Pacman.tile_size;

            blinky.PosX = 26*Pacman.tile_size;
            blinky.PosY = 4*Pacman.tile_size;

            pinky.PosX = 1*Pacman.tile_size;
            pinky.PosY = 4*Pacman.tile_size;

            inky.PosX = 26*Pacman.tile_size;
            inky.PosY = 32*Pacman.tile_size;

            clyde.PosX = 1*Pacman.tile_size;
            clyde.PosY = 32*Pacman.tile_size;
        }

        public static List<Tile> GetCurrentTiles(int pos_x, int pos_y)
        {
            Vector2 e_top_left = new Vector2(pos_x, pos_y);
            Vector2 e_bot_right = new Vector2(pos_x + Pacman.entity_size, pos_y + Pacman.entity_size);

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

        public static bool DoObjectsOverlap(Vector2 tl1, Vector2 tl2, Vector2 br1, Vector2 br2)
        {
            return tl1.X < br2.X && tl2.X < br1.X && tl1.Y < br2.Y && tl2.Y < br1.Y;
        }
    }

    public abstract class Entity
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public Texture2D Sprite { get; set; }
        public Pacman.Button? CurrentAction { get; set; }
        public Pacman.Button? QueuedAction { get; set; }

        public abstract void Move();

        public bool PredictCollision(int x_delta, int y_delta)
        {
            return EntityManager.GetCurrentTiles(PosX + x_delta, PosY + y_delta).Any(x => !x.Traversable);
        }

        public bool IsWithinWarp()
        {
            return EntityManager.GetCurrentTiles(PosX, PosY).Any(x => x is Warp);
        }

        public void WarpEntity()
        {
            Warp the_warp = EntityManager.GetCurrentTiles(PosX, PosY).First(x => x is Warp) as Warp;
            Warp matching_warp = TileManager.FindMatchingWarp(the_warp);
            if (CurrentAction == Pacman.Button.move_up)
            {
                PosX = matching_warp.PosX;
                PosY = matching_warp.PosY - Pacman.tile_size;
            }

            if (CurrentAction == Pacman.Button.move_down)
            {
                PosX = matching_warp.PosX;
                PosY = matching_warp.PosY + Pacman.tile_size;
            }

            if (CurrentAction == Pacman.Button.move_left)
            {
                PosX = matching_warp.PosX - Pacman.tile_size;
                PosY = matching_warp.PosY;
            }

            if (CurrentAction == Pacman.Button.move_right)
            {
                PosX = matching_warp.PosX + Pacman.tile_size;
                PosY = matching_warp.PosY;
            }
        }
    }

    public sealed class Player : Entity
    {
        public override void Move()
        {
            const int delta_move = 3;

            // Get keyboard input to determine current and queued actions
            if (Logic.IsButtonPressed(Pacman.Button.move_up))
            {
                if ((CurrentAction == Pacman.Button.move_left || CurrentAction == Pacman.Button.move_right) && PredictCollision(0, -delta_move))
                {
                    QueuedAction = Pacman.Button.move_up;
                }

                else
                {
                    CurrentAction = Pacman.Button.move_up;
                }
            }

            else if (Logic.IsButtonPressed(Pacman.Button.move_down))
            {
                if ((CurrentAction == Pacman.Button.move_left || CurrentAction == Pacman.Button.move_right) && PredictCollision(0, delta_move))
                {
                    QueuedAction = Pacman.Button.move_down;
                }

                else
                {
                    CurrentAction = Pacman.Button.move_down;
                }
            }

            else if (Logic.IsButtonPressed(Pacman.Button.move_left))
            {
                if ((CurrentAction == Pacman.Button.move_up || CurrentAction == Pacman.Button.move_down) && PredictCollision(-delta_move, 0))
                {
                    QueuedAction = Pacman.Button.move_left;
                }

                else
                {
                    CurrentAction = Pacman.Button.move_left;
                }
            }

            else if (Logic.IsButtonPressed(Pacman.Button.move_right))
            {
                if ((CurrentAction == Pacman.Button.move_up || CurrentAction == Pacman.Button.move_down) && PredictCollision(delta_move, 0))
                {
                    QueuedAction = Pacman.Button.move_right;
                }

                else
                {
                    CurrentAction = Pacman.Button.move_right;
                }
            }

            // Execute the current and queued actions
            if (CurrentAction == Pacman.Button.move_up || (QueuedAction == Pacman.Button.move_up && !PredictCollision(0, -delta_move)))
            {
                if (!PredictCollision(0, -delta_move))
                {
                    PosY -= delta_move;

                    if (QueuedAction == Pacman.Button.move_up)
                    {
                        CurrentAction = Pacman.Button.move_up;
                        QueuedAction = null;
                    }
                }

                else
                {
                    CurrentAction = null;
                }
            }

            else if (CurrentAction == Pacman.Button.move_down || (QueuedAction == Pacman.Button.move_down && !PredictCollision(0, delta_move)))
            {
                if (!PredictCollision(0, delta_move))
                {
                    PosY += delta_move;

                    if (QueuedAction == Pacman.Button.move_down)
                    {
                        CurrentAction = Pacman.Button.move_down;
                        QueuedAction = null;
                    }
                }

                else
                {
                    CurrentAction = null;
                }
            }

            else if (CurrentAction == Pacman.Button.move_left || (QueuedAction == Pacman.Button.move_left && !PredictCollision(-delta_move, 0)))
            {
                if (!PredictCollision(-delta_move, 0))
                {
                    PosX -= delta_move;

                    if (QueuedAction == Pacman.Button.move_left)
                    {
                        CurrentAction = Pacman.Button.move_left;
                        QueuedAction = null;
                    }
                }

                else
                {
                    CurrentAction = null;
                }
            }

            else if (CurrentAction == Pacman.Button.move_right || (QueuedAction == Pacman.Button.move_right && !PredictCollision(delta_move, 0)))
            {
                if (!PredictCollision(delta_move, 0))
                {
                    PosX += delta_move;

                    if (QueuedAction == Pacman.Button.move_right)
                    {
                        CurrentAction = Pacman.Button.move_right;
                        QueuedAction = null;
                    }

                }

                else
                {
                    CurrentAction = null;
                }
            }

            if (IsWithinWarp())
            {
                WarpEntity();
            }
        }
    }

    public abstract class Ghost : Entity
    {
        Pacman.Direction FacingDirection = Pacman.Direction.down;

        public override void Move()
        {
            //if (!(this is Blinky))
            //{
            //    return;
            //}

            Vector2 target = GetTarget();

            const int delta_move = 2;

            float dist_up = (float)Math.Sqrt(Math.Pow(PosX - target.X, 2) + Math.Pow(PosY - delta_move - target.Y, 2));
            float dist_down = (float)Math.Sqrt(Math.Pow(PosX - target.X, 2) + Math.Pow(PosY + delta_move - target.Y, 2));
            float dist_left = (float)Math.Sqrt(Math.Pow(PosX - delta_move - target.X, 2) + Math.Pow(PosY - target.Y, 2));
            float dist_right = (float)Math.Sqrt(Math.Pow(PosX + delta_move - target.X, 2) + Math.Pow(PosY - target.Y, 2));

            Dictionary<Pacman.Direction, float> distances = new Dictionary<Pacman.Direction, float>() 
            {
                { Pacman.Direction.up, dist_up },
                { Pacman.Direction.down, dist_down },
                { Pacman.Direction.left, dist_left },
                { Pacman.Direction.right, dist_right }
            };

            distances.Remove(Pacman.OppositeDir[FacingDirection]);
            while (true)
            {
                if (distances.Values.Min() == dist_up && distances.Keys.Contains(Pacman.Direction.up))
                {
                    if (PredictCollision(0, -delta_move))
                    {
                        distances.Remove(Pacman.Direction.up);
                        continue;
                    }

                    PosY -= delta_move;
                    FacingDirection = Pacman.Direction.up;
                    CurrentAction = Pacman.Button.move_up;
                    break;
                }

                if (distances.Values.Min() == dist_down && distances.Keys.Contains(Pacman.Direction.down))
                {
                    if (PredictCollision(0, delta_move))
                    {
                        distances.Remove(Pacman.Direction.down);
                        continue;
                    }

                    PosY += delta_move;
                    FacingDirection = Pacman.Direction.down;
                    CurrentAction = Pacman.Button.move_down;
                    break;
                }

                if (distances.Values.Min() == dist_left && distances.Keys.Contains(Pacman.Direction.left))
                {
                    if (PredictCollision(-delta_move, 0))
                    {
                        distances.Remove(Pacman.Direction.left);
                        continue;
                    }

                    PosX -= delta_move;
                    FacingDirection = Pacman.Direction.left;
                    CurrentAction = Pacman.Button.move_left;
                    break;
                }

                if (distances.Values.Min() == dist_right && distances.Keys.Contains(Pacman.Direction.right))
                {
                    if (PredictCollision(delta_move, 0))
                    {
                        distances.Remove(Pacman.Direction.right);
                        continue;
                    }

                    PosX += delta_move;
                    FacingDirection = Pacman.Direction.right;
                    CurrentAction = Pacman.Button.move_right;
                    break;
                }
            }

            if (IsWithinWarp())
            {
                WarpEntity();
            }
        }

        public abstract Vector2 GetTarget();
    }

    public sealed class Blinky : Ghost
    {
        public override Vector2 GetTarget()
        {
            return new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
        }
    }

    public sealed class Pinky : Ghost
    {
        public override Vector2 GetTarget()
        {
            return new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
        }
    }

    public sealed class Inky : Ghost
    {
        public override Vector2 GetTarget()
        {
            return new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
        }
    }

    public sealed class Clyde : Ghost
    {
        public override Vector2 GetTarget()
        {
            return new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
        }
    }
}
