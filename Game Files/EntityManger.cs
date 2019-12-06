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

        public static Vector2 jail_point = new Vector2(13 * Pacman.tile_size, 14 * Pacman.tile_size);

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

        public static List<Ghost> GetGhostList()
        {
            return new List<Ghost>() { blinky, pinky, inky, clyde };
        }

        public static void CreateEntities(ContentManager content)
        {
            player = new Player(new Vector2((int)(13 * Pacman.tile_size), 26 * Pacman.tile_size))
            {
                Sprite = content.Load<Texture2D>("Sprites/player")
            };

            blinky = new Blinky(new Vector2(26 * Pacman.tile_size, 4 * Pacman.tile_size))
            {
                SpriteList = new List<Texture2D>()
                {
                    content.Load<Texture2D>("Sprites/blinky"),
                    content.Load<Texture2D>("Sprites/frightened_ghost"),
                    content.Load<Texture2D>("Sprites/eaten_ghost"),
                    content.Load<Texture2D>("Sprites/blinky_target")
                }
            };

            pinky = new Pinky(new Vector2(1 * Pacman.tile_size, 4 * Pacman.tile_size))
            {
                SpriteList = new List<Texture2D>()
                {
                    content.Load<Texture2D>("Sprites/pinky"),
                    content.Load<Texture2D>("Sprites/frightened_ghost"),
                    content.Load<Texture2D>("Sprites/eaten_ghost"),
                    content.Load<Texture2D>("Sprites/pinky_target")
                }
            };

            inky = new Inky(new Vector2(26 * Pacman.tile_size, 32 * Pacman.tile_size))
            {
                SpriteList = new List<Texture2D>()
                {
                    content.Load<Texture2D>("Sprites/inky"),
                    content.Load<Texture2D>("Sprites/frightened_ghost"),
                    content.Load<Texture2D>("Sprites/eaten_ghost"),
                    content.Load<Texture2D>("Sprites/inky_target")
                }
            };

            clyde = new Clyde(new Vector2(1 * Pacman.tile_size, 32 * Pacman.tile_size))
            {
                SpriteList = new List<Texture2D>()
                {
                    content.Load<Texture2D>("Sprites/clyde"),
                    content.Load<Texture2D>("Sprites/frightened_ghost"),
                    content.Load<Texture2D>("Sprites/eaten_ghost"),
                    content.Load<Texture2D>("Sprites/clyde_target")
                }
            };

            foreach (Entity entity in GetEntityList())
            {
                entity.PosX = (int)entity.HomePoint.X;
                entity.PosY = (int)entity.HomePoint.Y;
                if (entity is Ghost ghost)
                {
                    ghost.Sprite = ghost.SpriteList[0];
                    ghost.TargetSprite = ghost.SpriteList[3];
                }
            }
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
                
            if (ghost_state == GhostState.frightened && state_timer >= 7)
            {
                ghost_state = suspended_state;
                state_timer = suspended_timer;
                EndFrightenedState();
            }

            if (ghost_state == GhostState.scatter && state_timer >= 7)
            {
                TurnaroundAllGhosts();
                ghost_state = GhostState.chase;
                suspended_state = GhostState.chase;
                state_timer = 0;
            }

            if (ghost_state == GhostState.chase && state_timer >= 20)
            {
                TurnaroundAllGhosts();
                ghost_state = GhostState.scatter;
                suspended_state = GhostState.scatter;
                state_timer = 0;
            }
        }

        public static void TurnaroundAllGhosts()
        {
            foreach (Ghost ghost in GetGhostList())
            {
                ghost.FacingDirection = Pacman.OppositeDir[ghost.FacingDirection];
            }
        }

        public static void BeginFrightenedState()
        {
            state_timer = 0;
            ghost_state = GhostState.frightened;
            TurnaroundAllGhosts();

            foreach (Ghost ghost in GetGhostList().Where(x => !x.Eaten))
            {
                ghost.move_speed = Ghost.frightened_speed;
                ghost.Sprite = ghost.SpriteList[1];
                ghost.Rested = false;
            }
        }

        public static void EndFrightenedState()
        {
            foreach (Ghost ghost in GetGhostList().Where(x => !x.Eaten))
            {
                ghost.move_speed = Ghost.normal_speed;
                ghost.Sprite = ghost.SpriteList[0];
                ghost.Rested = false;
            }
        }
    }

    public abstract class Entity : GameObject
    {
        public Pacman.Direction FacingDirection { get; set; }
        public Vector2 HomePoint { get; set; }
        public int move_speed = 2;

        public Entity(Vector2 home_point)
        {
            HomePoint = home_point;
        }

        public abstract void Move();

        public bool PredictCollision(int x_delta, int y_delta)
        {
            return GetCurrentTiles(PosX + x_delta, PosY + y_delta).Any(x => !x.Traversable);
        }

        public bool IsWithinWarp()
        {
            return GetCurrentTiles().Any(x => x is Warp);
        }

        public void WarpEntity()
        {
            Warp the_warp = GetCurrentTiles(PosX, PosY).First(x => x is Warp) as Warp;
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

        public void FixPosition()
        {
            // This fixes any problems that would have occured from changing move-speeds mid-game
            int x_remainder = PosX % move_speed;
            int y_remainder = PosY % move_speed;

            if (PredictCollision(x_remainder, 0))
            {
                PosX -= x_remainder;
            }

            else
            {
                PosX += x_remainder;
            }

            if (PredictCollision(y_remainder, 0))
            {
                PosY -= y_remainder;
            }

            else
            {
                PosY += y_remainder;
            }
        }

        public List<Tile> GetCurrentTiles()
        {
            return Logic.FindOverlapsFromList(this, TileManager.GetTileList().Select(x => x as GameObject)).Select(x => x as Tile).ToList();
        }

        public List<Tile> GetCurrentTiles(int pos_x, int pos_y)
        {
            return Logic.FindOverlapsFromList(pos_x, pos_y, TileManager.GetTileList().Select(x => x as GameObject)).Select(x => x as Tile).ToList();
        }
    }

    public sealed class Player : Entity
    {
        public Pacman.Button? CurrentAction { get; set; }
        public Pacman.Button? QueuedAction { get; set; }

        public Player(Vector2 home_point) : base(home_point) { }

        public override void Move()
        {
            FixPosition();

            // Get keyboard input to determine current and queued actions
            if (Logic.IsButtonPressed(Pacman.Button.move_up))
            {
                if ((CurrentAction == Pacman.Button.move_left || CurrentAction == Pacman.Button.move_right) && PredictCollision(0, -move_speed))
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
                if ((CurrentAction == Pacman.Button.move_left || CurrentAction == Pacman.Button.move_right) && PredictCollision(0, move_speed))
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
                if ((CurrentAction == Pacman.Button.move_up || CurrentAction == Pacman.Button.move_down) && PredictCollision(-move_speed, 0))
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
                if ((CurrentAction == Pacman.Button.move_up || CurrentAction == Pacman.Button.move_down) && PredictCollision(move_speed, 0))
                {
                    QueuedAction = Pacman.Button.move_right;
                }

                else
                {
                    CurrentAction = Pacman.Button.move_right;
                }
            }

            // Execute the current and queued actions
            if (CurrentAction == Pacman.Button.move_up || (QueuedAction == Pacman.Button.move_up && !PredictCollision(0, -move_speed)))
            {
                if (!PredictCollision(0, -move_speed))
                {
                    PosY -= move_speed;
                    FacingDirection = Pacman.Direction.up;

                    if (QueuedAction == Pacman.Button.move_up)
                    {
                        CurrentAction = Pacman.Button.move_up;
                        QueuedAction = null;
                    }
                }
            }

            if (CurrentAction == Pacman.Button.move_down || (QueuedAction == Pacman.Button.move_down && !PredictCollision(0, move_speed)))
            {
                if (!PredictCollision(0, move_speed))
                {
                    PosY += move_speed;
                    FacingDirection = Pacman.Direction.down;

                    if (QueuedAction == Pacman.Button.move_down)
                    {
                        CurrentAction = Pacman.Button.move_down;
                        QueuedAction = null;
                    }
                }
            }

            if (CurrentAction == Pacman.Button.move_left || (QueuedAction == Pacman.Button.move_left && !PredictCollision(-move_speed, 0)))
            {
                if (!PredictCollision(-move_speed, 0))
                {
                    PosX -= move_speed;
                    FacingDirection = Pacman.Direction.left;

                    if (QueuedAction == Pacman.Button.move_left)
                    {
                        CurrentAction = Pacman.Button.move_left;
                        QueuedAction = null;
                    }
                }
            }

            if (CurrentAction == Pacman.Button.move_right || (QueuedAction == Pacman.Button.move_right && !PredictCollision(move_speed, 0)))
            {
                if (!PredictCollision(move_speed, 0))
                {
                    PosX += move_speed;
                    FacingDirection = Pacman.Direction.right;

                    if (QueuedAction == Pacman.Button.move_right)
                    {
                        CurrentAction = Pacman.Button.move_right;
                        QueuedAction = null;
                    }
                }
            }

            EatThings();
            EncounterGhost();
            if (IsWithinWarp())
            {
                WarpEntity();
            }
        }

        public void EatThings()
        {
            List<Item> to_be_eaten = Logic.FindOverlapsFromList(this, TileManager.GetItemList().Select(x => x as GameObject)).Select(x => x as Item).ToList();

            foreach (Item item in to_be_eaten)
            {
                item.UponEating();
            }
        }

        public void EncounterGhost()
        {
            List<Ghost> encountered_ghosts = Logic.FindOverlapsFromList(this, EntityManager.GetGhostList().Select(x => x as GameObject)).Select(x => x as Ghost).ToList();

            foreach (Ghost ghost in encountered_ghosts)
            {
                // Nothing happens if you run into a ghost while its running back to rest
                if (ghost.Eaten && !ghost.Rested)
                {

                }

                // If you encounter a ghost while it's frightened, it will get eaten and run back to rest
                else if (EntityManager.ghost_state == EntityManager.GhostState.frightened && !ghost.Rested)
                {
                    ghost.GetEaten();
                }

                // Otherwise, the ghost will kill you
                else
                {

                }
            }
        }
    }

    public abstract class Ghost : Entity
    {
        public Texture2D TargetSprite { get; set; }
        public Vector2 CurrentTarget { get; set; }
        public List<Texture2D> SpriteList { get; set; }
        public bool Eaten = false;
        public bool Rested = false;
        public const int normal_speed = 2;
        public const int frightened_speed = 1;
        public const int eaten_speed = 4;

        public Ghost(Vector2 home_point) : base(home_point) { }

        public override void Move()
        {
            CurrentTarget = UpdateTarget();
            FixPosition();
            TryToRecover();

            float dist_up = (float)Math.Sqrt(Math.Pow(PosX - CurrentTarget.X, 2) + Math.Pow(PosY - move_speed - CurrentTarget.Y, 2));
            float dist_down = (float)Math.Sqrt(Math.Pow(PosX - CurrentTarget.X, 2) + Math.Pow(PosY + move_speed - CurrentTarget.Y, 2));
            float dist_left = (float)Math.Sqrt(Math.Pow(PosX - move_speed - CurrentTarget.X, 2) + Math.Pow(PosY - CurrentTarget.Y, 2));
            float dist_right = (float)Math.Sqrt(Math.Pow(PosX + move_speed - CurrentTarget.X, 2) + Math.Pow(PosY - CurrentTarget.Y, 2));

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
                    if (PredictCollision(0, -move_speed))
                    {
                        distances.Remove(Pacman.Direction.up);
                        continue;
                    }

                    PosY -= move_speed;
                    FacingDirection = Pacman.Direction.up;
                    break;
                }

                if (distances.Values.Min() == dist_down && distances.Keys.Contains(Pacman.Direction.down))
                {
                    if (PredictCollision(0, move_speed))
                    {
                        distances.Remove(Pacman.Direction.down);
                        continue;
                    }

                    PosY += move_speed;
                    FacingDirection = Pacman.Direction.down;
                    break;
                }

                if (distances.Values.Min() == dist_left && distances.Keys.Contains(Pacman.Direction.left))
                {
                    if (PredictCollision(-move_speed, 0))
                    {
                        distances.Remove(Pacman.Direction.left);
                        continue;
                    }

                    PosX -= move_speed;
                    FacingDirection = Pacman.Direction.left;
                    break;
                }

                if (distances.Values.Min() == dist_right && distances.Keys.Contains(Pacman.Direction.right))
                {
                    if (PredictCollision(move_speed, 0))
                    {
                        distances.Remove(Pacman.Direction.right);
                        continue;
                    }

                    PosX += move_speed;
                    FacingDirection = Pacman.Direction.right;
                    break;
                }
            }

            if (IsWithinWarp())
            {
                WarpEntity();
            }
        }
        
        public Vector2 UpdateTarget()
        {
            // Eaten mode
            if (Eaten)
            {
                return EntityManager.jail_point;
            }

            // Scatter mode
            if (EntityManager.ghost_state == EntityManager.GhostState.scatter)
            {
                return HomePoint;
            }

            // Frightened mode
            else if (EntityManager.ghost_state == EntityManager.GhostState.frightened && !Rested)
            {
                return PickRandomAdjacentPoint();
            }

            // Chase mode
            return GetChaseTarget();
        }

        public abstract Vector2 GetChaseTarget();

        public Vector2 PickRandomAdjacentPoint()
        {
            Random rng = new Random();

            Vector2 point_up = new Vector2(PosX , PosY - Pacman.tile_size);
            Vector2 point_down = new Vector2(PosX, PosY + Pacman.tile_size);
            Vector2 point_left = new Vector2(PosX - Pacman.tile_size, PosY);
            Vector2 point_right = new Vector2(PosX + Pacman.tile_size, PosY);

            Dictionary<Pacman.Direction, Vector2> point_list = new Dictionary<Pacman.Direction, Vector2>()
            {
                { Pacman.Direction.up, point_up },
                { Pacman.Direction.down, point_down },
                { Pacman.Direction.left, point_left },
                { Pacman.Direction.right, point_right }
            };

            point_list.Remove(Pacman.OppositeDir[FacingDirection]);
            return point_list.ToList()[rng.Next(point_list.Count())].Value;
        }

        public void GetEaten()
        {
            Eaten = true;
            Sprite = SpriteList[2];
            move_speed = eaten_speed;
        }

        public void TryToRecover()
        {
            List<Tile> recovery_points = TileManager.GetTileList().Where(x => x.IsRecoveryPoint).ToList();

            if (Logic.FindOverlapsFromList(this, recovery_points.Select(x => x as GameObject)).Count > 0)
            {
                Recover();
            }
        }

        public void Recover()
        {
            Eaten = false;
            Rested = true;
            Sprite = SpriteList[0];
            move_speed = normal_speed;
        }
    }

    public sealed class Blinky : Ghost
    {
        public override Vector2 GetChaseTarget()
        {
            return new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
        }

        public Blinky(Vector2 home_point) : base(home_point) { }
    }

    public sealed class Pinky : Ghost
    {
        public override Vector2 GetChaseTarget()
        {
            return EntityManager.FindPointInFrontOfEntity(EntityManager.player, 4);
        }

        public Pinky(Vector2 home_point) : base(home_point) { }
    }

    public sealed class Inky : Ghost
    {
        public override Vector2 GetChaseTarget()
        {
            Vector2 mid_point = EntityManager.FindPointInFrontOfEntity(EntityManager.player, 2);
            Vector2 blinky_point = new Vector2(EntityManager.blinky.PosX, EntityManager.blinky.PosY);

            float rise = (blinky_point.Y - mid_point.Y);
            float run = (blinky_point.X - mid_point.X);

            return new Vector2(mid_point.X - run, mid_point.Y - rise);
        }

        public Inky(Vector2 home_point) : base(home_point) { }
    }

    public sealed class Clyde : Ghost
    {
        public override Vector2 GetChaseTarget()
        {
            Vector2 player_point = new Vector2(EntityManager.player.PosX, EntityManager.player.PosY);
            float current_distance = (float)Math.Sqrt(Math.Pow(PosY - player_point.Y, 2) + Math.Pow(PosX - player_point.X, 2));

            if (current_distance > Pacman.tile_size*8)
            {
                return player_point;
            }

            return HomePoint;
        }

        public Clyde(Vector2 home_point) : base(home_point) { }
    }
}
