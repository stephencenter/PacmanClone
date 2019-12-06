using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace Pacman
{
    public static class TileManager
    {
        private static readonly List<Tile> tile_list = new List<Tile>();
        private static readonly List<Item> item_list = new List<Item>();
        
        public static List<Tile> GetTileList()
        {
            return tile_list;
        }

        public static List<Item> GetItemList()
        {
            return item_list;
        }

        public static Warp FindMatchingWarp(Warp warp)
        {
            return tile_list.First(x => x is Warp && (x as Warp).Symbol == warp.Symbol && x != warp) as Warp;
        }

        public static void CreateGameMap(ContentManager content)
        {
            int x = -Pacman.tile_size;
            int y = 0;

            List<string> text_map = File.ReadAllText("text_map.txt").Split('\n').ToList();
            while (text_map.Count < Pacman.screen_height)
            {
                text_map.Add("");
            }

            foreach (string line in text_map)
            {
                if (line.StartsWith("//"))
                {
                    continue;
                }

                foreach (char symbol in string.Concat(line, new string(' ', Math.Max(0, Pacman.screen_width + 2 - line.Length))))
                {
                    if (symbol == 'X')
                    {
                        tile_list.Add(new Tile(x, y, "Sprites/nontraversable_blue", false, false, content));
                    }

                    else if (symbol == '_')
                    {
                        tile_list.Add(new Tile(x, y, "Sprites/traversable", true, false, content));
                    }

                    else if (symbol == '.')
                    {
                        tile_list.Add(new Tile(x, y, "Sprites/traversable", true, false, content));
                        item_list.Add(new SmallPellet(x, y, "Sprites/small_pellet", content));
                    }

                    else if (symbol == '*')
                    {
                        tile_list.Add(new Tile(x, y, "Sprites/traversable", true, false, content));
                        item_list.Add(new PowerPellet(x, y, "Sprites/power_pellet", content));
                    }
                    
                    else if (symbol == '%')
                    {
                        tile_list.Add(new Tile(x, y, "Sprites/traversable", true, true, content));
                    }

                    else if (new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F' }.Contains(symbol))
                    {
                        tile_list.Add(new Warp(x, y, symbol, "Sprites/traversable_black", true, false, content));
                    }

                    else
                    {
                        tile_list.Add(new Tile(x, y, "Sprites/nontraversable_black", false, false, content));
                    }

                    x += Pacman.tile_size;
                }

                x = -Pacman.tile_size;
                y += Pacman.tile_size;
            }
        }
    }

    public class Tile : GameObject
    {
        public bool Traversable;
        public bool IsRecoveryPoint;

        public Tile(int pos_x, int pos_y, string sprite, bool traversable, bool recovery, ContentManager content)
        {
            PosX = pos_x;
            PosY = pos_y;
            Sprite = content.Load<Texture2D>(sprite);
            Traversable = traversable;
            IsRecoveryPoint = recovery;
        }
    }

    public class Warp : Tile
    {
        public char Symbol;

        public Warp(int pos_x, int pos_y, char symbol, string sprite, bool traversable, bool recovery, ContentManager content) : base(pos_x, pos_y, sprite, traversable, recovery, content)
        {
            Symbol = symbol;
        }
    }

    public abstract class Item : GameObject
    {
        public Item(int pos_x, int pos_y, string sprite, ContentManager content)
        {
            PosX = pos_x;
            PosY = pos_y;
            Sprite = content.Load<Texture2D>(sprite);
        }

        public abstract void UponEating();
    }

    public class SmallPellet : Item
    {
        public SmallPellet(int pos_x, int pos_y, string sprite, ContentManager content) : base(pos_x, pos_y, sprite, content) { }

        public override void UponEating()
        {
            TileManager.GetItemList().Remove(this);
            Pacman.collected_pellets++;
        }
    }

    public class PowerPellet : Item
    {
        public PowerPellet(int pos_x, int pos_y, string sprite, ContentManager content) : base(pos_x, pos_y, sprite, content) { }

        public override void UponEating()
        {
            TileManager.GetItemList().Remove(this);
            EntityManager.BeginFrightenedState();
        }
    }
}
