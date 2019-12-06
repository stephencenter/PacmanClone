using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Xna.Framework;
using System.ComponentModel;

namespace Pacman
{
    public static class EntityManager
    {
        // List of entities
        public static Player player;
        public static Blinky blinky;
        public static Pinky pinky;
        public static Inky inky;
        public static Clyde clyde;

        public static GhostState ghost_state;
        public static float state_timer;
        private static float previous_timer;

        private static GhostState suspended_state;
        private static float suspended_timer;

        // List of possible AI states for the ghosts
        public enum GhostState
        {
            [Description("scatter")] scatter,
            [Description("chase")] chase,
            [Description("frightened")] frightened
            // Eaten is also technically an AI state, but it is handled per-ghost and not globally
        }

        public static List<Entity> GetEntityList()
        {
            return new List<Entity>() { player, blinky, pinky, inky, clyde };
        }

        public static void CreateEntities(ContentManager content)
        {
            player = new Player(content.Load<Texture2D>("Sprites/player"), new Vector2((int)(13.6 * Pacman.tile_size), 26 * Pacman.tile_size));

            blinky = new Blinky(content.Load<Texture2D>("Sprites/blinky"), new Vector2(26 * Pacman.tile_size, 4 * Pacman.tile_size), content.Load<Texture2D>("Sprites/blinky_target"));

            pinky = new Pinky(content.Load<Texture2D>("Sprites/pinky"), new Vector2(1 * Pacman.tile_size, 4 * Pacman.tile_size), content.Load<Texture2D>("Sprites/pinky_target"));

            inky = new Inky(content.Load<Texture2D>("Sprites/inky"), new Vector2(26 * Pacman.tile_size, 32 * Pacman.tile_size), content.Load<Texture2D>("Sprites/inky_target"));

            clyde = new Clyde(content.Load<Texture2D>("Sprites/clyde"), new Vector2(1 * Pacman.tile_size, 32 * Pacman.tile_size), content.Load<Texture2D>("Sprites/clyde_target"));

            foreach (Entity entity in GetEntityList())
            {
                entity.PosX = (int)entity.HomePoint.X;
                entity.PosY = (int)entity.HomePoint.Y;
            }
        }

        public static List<Tile> GetCurrentTiles(float pos_x, float pos_y)
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

        public static Vector2 FindPointInFrontOfEntity(Entity entity, int num_tiles)
        {
            int delta_x = 0;
            int delta_y = 0;

            if (entity.FacingDirection == Pacman.Direction.up)
            {
                delta_y = -num_tiles * Pacman.tile_size;
            }

            else if (entity.FacingDirection == Pacman.Direction.down)
            {
                delta_y = num_tiles * Pacman.tile_size;
            }

            else if (entity.FacingDirection == Pacman.Direction.left)
            {
                delta_x = -num_tiles * Pacman.tile_size;
            }

            else
            {
                delta_x = num_tiles * Pacman.tile_size;
            }

            return new Vector2(entity.PosX + delta_x, entity.PosY + delta_y);
        }

        public static void ManageGhostStates(GameTime game_time)
        {
            float current_time = (float)(game_time.TotalGameTime.TotalSeconds);
            state_timer += current_time - previous_timer;
            previous_timer = current_time;

            if (ghost_state != GhostState.frightened)
            {
                suspended_timer = state_timer;
            }
                
            if (ghost_state == GhostState.frightened && state_timer >= 8)
            {
                ghost_state = suspended_state;
                state_timer = suspended_timer;
            }

            if (ghost_state == GhostState.scatter && state_timer >= 7)
            {
                foreach (Ghost ghost in GetEntityList().Where(x => x is Ghost))
                {
                    ghost.FacingDirection = Pacman.OppositeDir[ghost.FacingDirection];
                }

                ghost_state = GhostState.chase;
                suspended_state = GhostState.chase;
                state_timer = 0;
            }

            if (ghost_state == GhostState.chase && state_timer >= 20)
            {
                foreach (Ghost ghost in GetEntityList().Where(x => x is Ghost))
                {
                    ghost.FacingDirection = Pacman.OppositeDir[ghost.FacingDirection];
                }

                ghost_state = GhostState.scatter;
                suspended_state = GhostState.scatter;
                state_timer = 0;
            }
        }
    }

    public abstract class Entity
    {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public Texture2D Sprite { get; set; }
        public Pacman.Direction FacingDirection { get; set; }
        public Vector2 HomePoint { get; set; }

        public Entity(Texture2D sprite, Vector2 home_point)
        {
            Sprite = sprite;
            HomePoint = home_point;
        }

        public abstract void Move();

        public bool PredictCollision(float x_delta, float y_delta)
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

            if (FacingDirection == Pacman.Direction.up)
            {
                PosX = matching_warp.PosX;
                PosY = matching_warp.PosY - Pacman.tile_size;
            }

            if (FacingDirection == Pacman.Direction.down)
            {
                PosX = matching_warp.PosX;
                PosY = matching_warp.PosY + Pacman.tile_size;
            }

            if (FacingDirection == Pacman.Direction.left)
            {
                PosX = matching_warp.PosX - Pacman.tile_size;
                PosY = matching_warp.PosY;
            }

            if (FacingDirection == Pacman.Direction.right)
            {
                PosX = matching_warp.PosX + Pacman.tile_size;
                PosY = matching_warp.PosY;
            }
        }
    }

    public sealed class Player : Entity
    {
        public Pacman.Button? CurrentAction { get; set; }
        public Pacman.Button? QueuedAction { get; set; }

        public Player(Texture2D sprite, Vector2 home_point) : base(sprite, home_point) { }

        public override void Move()
        {
            const int delta_move = 1;

            EatThings();

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
                    FacingDirection = Pacman.Direction.up;

                    if (QueuedAction == Pacman.Button.move_up)
                    {
                        CurrentAction = Pacman.Button.move_up;
                        QueuedAction = null;
                    }
                }
            }

            if (CurrentAction == Pacman.Button.move_down || (QueuedAction == Pacman.Button.move_down && !PredictCollision(0, delta_move)))
            {
                if (!PredictCollision(0, delta_move))
                {
                    PosY += delta_move;
                    FacingDirection = Pacman.Direction.down;

                    if (QueuedAction == Pacman.Button.move_down)
                    {
                        CurrentAction = Pacman.Button.move_down;
                        QueuedAction = null;
                    }
                }
            }

            if (CurrentAction == Pacman.Button.move_left || (QueuedAction == Pacman.Button.move_left && !PredictCollision(-delta_move, 0)))
            {
                if (!PredictCollision(-delta_move, 0))
                {
                    PosX -= delta_move;
                    FacingDirection = Pacman.Direction.left;

                    if (QueuedAction == Pacman.Button.move_left)
                    {
                        CurrentAction = Pacman.Button.move_left;
                        QueuedAction = null;
                    }
                }
            }

            if (CurrentAction == Pacman.Button.move_right || (QueuedAction == Pacman.Button.move_right && !PredictCollision(delta_move, 0)))
            {
                if (!PredictCollision(delta_move, 0))
                {
                    PosX += delta_move;
                    FacingDirection = Pacman.Direction.right;

                    if (QueuedAction == Pacman.Button.move_right)
                    {
                        CurrentAction = Pacman.Button.move_right;
                        QueuedAction = null;
                    }
                }
            }

            if (IsWithinWarp())
            {
                WarpEntity();
            }
        }

        public void EatThings()
        {
            List<Item> to_be_eaten = new List<Item>();

            Vector2 p_topleft = new Vector2(PosX, PosY);
            Vector2 p_botright = new Vector2(PosX + Pacman.tile_size, PosY + Pacman.tile_size);
            foreach (Item item in TileManager.GetItemList())
            {
                Vector2 i_topleft = new Vector2(item.PosX, item.PosY);
                Vector2 i_botright = new Vector2(item.PosX + Pacman.tile_size, item.PosY + Pacman.tile_size);

                if (EntityManager.DoObjectsOverlap(p_topleft, i_topleft, p_botright, i_botright))
                {
                    to_be_eaten.Add(item);
                }
            }

            foreach (Item item in to_be_eaten)
            {
                item.UponEating();
            }
        }
    }

    public abstract class Ghost : Entity
    {
        public Texture2D TargetSprite { get; set; }
        public Vector2 CurrentTarget { get; set; }

        public Ghost(Texture2D sprite, Vector2 home_point, Texture2D t_sprite) : base(sprite, home_point)
        {
            TargetSprite = t_sprite;
        }

        public override void Move()
        {
            CurrentTarget = UpdateTarget();
            const int delta_move = 1;

            float dist_up = (float)Math.Sqrt(Math.Pow(PosX - CurrentTarget.X, 2) + Math.Pow(PosY - delta_move - CurrentTarget.Y, 2));
            float dist_down = (float)Math.Sqrt(Math.Pow(PosX - CurrentTarget.X, 2) + Math.Pow(PosY + delta_move - CurrentTarget.Y, 2));
            float dist_left = (float)Math.Sqrt(Math.Pow(PosX - delta_move - CurrentTarget.X, 2) + Math.Pow(PosY - CurrentTarget.Y, 2));
            float dist_right = (float)Math.Sqrt(Math.Pow(PosX + delta_move - CurrentTarget.X, 2) + Math.Pow(PosY - CurrentTarget.Y, 2));

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
                    break;
                }
            }

            if (IsWithinWarp())
            {
                WarpEntity();
            }
        }

        public abstract Vector2 UpdateTarget();
    }

    public sealed class Blinky : Ghost
    {
        public override Vector2 UpdateTarget()
        {
            // Scatter state
            if (EntityManager.ghost_state == EntityManager.GhostState.scatter)
            {
                return HomePoint;
            }

            // Chase state
            return new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
        }

        public Blinky(Texture2D sprite, Vector2 home_point, Texture2D t_sprite) : base(sprite, home_point, t_sprite) { }
    }

    public sealed class Pinky : Ghost
    {
        public override Vector2 UpdateTarget()
        {
            // Scatter state
            if (EntityManager.ghost_state == EntityManager.GhostState.scatter)
            {
                return HomePoint;
            }

            // Chase state
            return EntityManager.FindPointInFrontOfEntity(EntityManager.player, 4);
        }

        public Pinky(Texture2D sprite, Vector2 home_point, Texture2D t_sprite) : base(sprite, home_point, t_sprite) { }
    }

    public sealed class Inky : Ghost
    {
        public override Vector2 UpdateTarget()
        {
            // Scatter state
            if (EntityManager.ghost_state == EntityManager.GhostState.scatter)
            {
                return HomePoint;
            }

            // Chase state
            Vector2 mid_point = EntityManager.FindPointInFrontOfEntity(EntityManager.player, 2);
            Vector2 blinky_point = new Vector2(EntityManager.blinky.PosX, EntityManager.blinky.PosY);

            float rise = (blinky_point.Y - mid_point.Y);
            float run = (blinky_point.X - mid_point.X);

            return new Vector2(mid_point.X - run, mid_point.Y - rise);
        }

        public Inky(Texture2D sprite, Vector2 home_point, Texture2D t_sprite) : base(sprite, home_point, t_sprite) { }
    }

    public sealed class Clyde : Ghost
    {
        public override Vector2 UpdateTarget()
        {
            // Scatter state
            if (EntityManager.ghost_state == EntityManager.GhostState.scatter)
            {
                return HomePoint;
            }

            // Chase state
            Vector2 player_point = new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
            float current_distance = (float)Math.Sqrt(Math.Pow(PosY - player_point.Y, 2) + Math.Pow(PosX - player_point.X, 2));

            if (current_distance > Pacman.tile_size*8)
            {
                return player_point;
            }

            return HomePoint;
        }

        public Clyde(Texture2D sprite, Vector2 home_point, Texture2D t_sprite) : base(sprite, home_point, t_sprite) { }
    }
}
