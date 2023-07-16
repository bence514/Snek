using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Snek
{
    internal class PathFinding
    {
        class Tile
        { 
            public int x;
            public int y;
            public int cost;
            public int distance;
            public int CostDistance { get { return this.cost + this.distance; } }
            public Tile parent;
            public Tile(int x, int y, int cost)
            {
                this.x = x;
                this.y = y;
                this.cost = cost;
            }

            public Tile(int x, int y, Tile parent, int cost)
            {
                this.x = x;
                this.y = y;
                this.parent = parent;
                this.cost = cost;
            }

            public void setDistance(int targetX, int targetY)
            { 
                this.distance = Math.Abs(this.x - targetX) + Math.Abs(this.y - targetY);
            }
        }
        public static bool aStar(Queue<Program.Position> walls, Program.Position target, out Queue<Program.Position> moveset)
        {
            //snake specific
            Tile start = new Tile(walls.Last().x, walls.Last().y, 100);
            var borders = 50;
            //
            Tile finish = new Tile(target.x, target.y, 0);

            moveset = new Queue<Program.Position>();

            start.setDistance(finish.x, finish.y);

            var activeTiles = new List<Tile>();
            activeTiles.Add(start);
            var visitedTiles = new List<Tile>();

            while (activeTiles.Count > 0) 
            { 
                var currentTile = activeTiles.OrderBy(x => x.CostDistance).First();

                //are we there yet?
                if (currentTile.x == finish.x && currentTile.y == finish.y)
                {
                    var tmp = new List<Program.Position>();
                    while (currentTile.x != start.x || currentTile.y != start.y)
                    {
                        tmp.Add(new Program.Position(currentTile.x, currentTile.y));
                        currentTile = currentTile.parent;
                    }
                    tmp.Reverse();
                    foreach (var tile in tmp)
                    { 
                        moveset.Enqueue(tile);
                    }

                    return true;
                }

                visitedTiles.Add(currentTile);
                activeTiles.Remove(currentTile);

                var possibleTiles = getWalkable(walls, currentTile, finish, borders);

                foreach (var tile in possibleTiles)
                {
                    if (visitedTiles.Exists(vtile => vtile.x == tile.x && vtile.y == tile.y))
                    {
                        continue;
                    }

                    if (activeTiles.Exists(vtile => vtile.x == tile.x && vtile.y == tile.y))
                    {
                        if (tile.CostDistance > currentTile.CostDistance)
                        {
                            activeTiles.Remove(activeTiles.First(vtile => vtile.x == tile.x && vtile.y == tile.y));
                            activeTiles.Add(tile);
                        }
                    }
                    else
                    {
                        activeTiles.Add(tile);
                    }
                }
            }

            return false;
        }

        static Tile[] getWalkable(Queue<Program.Position> walls, Tile currentTile, Tile TargetTile, int border)
        { 
            var possibleTiles = new Tile[4] {
            new Tile(currentTile.x + 1, currentTile.y, currentTile, currentTile.cost + 1),
            new Tile(currentTile.x - 1, currentTile.y, currentTile, currentTile.cost + 1),
            new Tile(currentTile.x, currentTile.y + 1, currentTile, currentTile.cost + 1),
            new Tile(currentTile.x, currentTile.y - 1, currentTile, currentTile.cost + 1)};

            foreach (var tile in possibleTiles) 
            {
                tile.setDistance(TargetTile.x, TargetTile.y);
            }

            return possibleTiles.Where(item => item.x < border && item.y < border && item.x > -1 && item.y > -1 && !walls.Contains(new Program.Position(item.x, item.y))).ToArray();
        }
    }
}
