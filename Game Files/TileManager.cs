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
        private static readonly List<Tile> TileList = new List<Tile>();

        public static List<Tile> GetTileList()
        {
            return TileList;
        }

        public static Warp FindMatchingWarp(Warp warp)
        {
            return TileList.First(x => x is Warp && (x as Warp).Symbol == warp.Symbol && x != warp) as Warp;
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
                foreach (char symbol in string.Concat(line, new string(' ', Math.Max(0, Pacman.screen_width + 2 - line.Length))))
                {
                    if (symbol == 'X')
                    {
                        TileList.Add(new Tile(x, y, "non_traversable", false, content));
                    }

                    else if (symbol == '.')
                    {
                        TileList.Add(new Tile(x, y, "traversable_grey", true, content));
                    }

                    else if (new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F' }.Contains(symbol))
                    {
                        TileList.Add(new Warp(x, y, symbol, "traversable_black", true, content));
                    }

                    else
                    {
                        TileList.Add(new Tile(x, y, "traversable_black", true, content));
                    }

                    x += Pacman.tile_size;
                }

                x = -Pacman.tile_size;
                y += Pacman.tile_size;
            }
        }
    }

    public class Tile
    {
        public int PosX;
        public int PosY;
        public Texture2D Sprite;
        public bool Traversable;

        public Tile(int pos_x, int pos_y, string sprite, bool traversable, ContentManager content)
        {
            PosX = pos_x;
            PosY = pos_y;
            Sprite = content.Load<Texture2D>(sprite);
            Traversable = traversable;
        }
    }

    public class Warp : Tile
    {
        public char Symbol;

        public Warp(int pos_x, int pos_y, char symbol, string sprite, bool traversable, ContentManager content) : base(pos_x, pos_y, sprite, traversable, content)
        {
            Symbol = symbol;
        }
    }
}
