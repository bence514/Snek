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
        public static bool aStar(Queue<Program.Position> walls, Program.Position target, out Queue<Program.Position> moveset, bool loose)
        {
            //snake specific
            Tile start = new Tile(walls.Last().x, walls.Last().y, 0);
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
                    if (loose /*|| allReachable(walls, tmp, borders)*/)
                    {
                        foreach (var tile in tmp)
                        {
                            moveset.Enqueue(tile);
                        }

                        return true;
                    }
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

            Tile[] trulyPossibleTiles = new Tile[4];
            int c = 0;
            foreach (var tile in possibleTiles.Where(item => item.x < border && item.y < border && item.x > -1 && item.y > -1))
            {
                if (walls.Contains(new Program.Position(tile.x, tile.y)) && walls.ToList().IndexOf(walls.First(x => x.x == tile.x && x.y == tile.y)) + 1 < tile.cost)
                {
                    trulyPossibleTiles[c] = tile;
                    c++;
                }
                else if (!walls.Contains(new Program.Position(tile.x, tile.y)))
                {
                    trulyPossibleTiles[c] = tile;
                    c++;
                }
            }

            //increase penalty for being near the snake
            //foreach (var tile in trulyPossibleTiles.Where(x => x != null && walls.Any(y => ((y.x + 1 == x.x || y.x - 1 == x.x) && y.y == x.y) || ((y.y + 1 == x.y || y.y - 1 == x.y) && y.x == x.x) || (y.y + 1 == x.y && y.x + 1 == x.x) || (y.y + 1 == x.y && y.x - 1 == x.x) || (y.y - 1 == x.y && y.x + 1 == x.x) || (y.y - 1 == x.y && y.x - 1 == x.x))))
                //tile.distance += 15;

            return trulyPossibleTiles.Where(x => x != null).ToArray();
        }
        /*
        static bool allReachable(Queue<Program.Position> snake, List<Program.Position> moves, int border)
        {
            int[,] virtualMap = new int[border, border];
            Queue<Program.Position> virtualSnake = new Queue<Program.Position>();
            List<Program.Position> virtualMoves = new List<Program.Position>();
            foreach (var item in snake) 
            {
                virtualSnake.Enqueue(snake.Last());
            }
            foreach (var item in moves)
            {
                virtualMoves.Add(item);
            }

            for (int i = 0; i < virtualMap.GetLength(0); i++)
            {
                for (int j = 0; j < virtualMap.GetLength(1); j++)
                {
                    virtualMap[i, j] = 0;
                }
            }

            for (var i = 0; i < virtualMoves.Count; i++)
            {
                virtualSnake.Enqueue(virtualMoves[virtualMoves.Count - i - 1]);
                virtualSnake.Dequeue();
            }

            foreach (var item in virtualSnake)
            {
                virtualMap[item.x, item.y] = 9;
            }

            foreach (var item in virtualMoves)
            {
                virtualMap[item.x, item.y] = 9;
            }

            virtualMap[virtualMoves.Last().x, virtualMoves.Last().y] = 1;

            while (matrixContains(virtualMap, 1)) 
            {
                for (int i = 0; i < virtualMap.GetLength(0); i++)
                {
                    for (int j = 0; j < virtualMap.GetLength(1); j++)
                    {
                        if (virtualMap[i, j] == 1)
                        {
                            if (i - 1 > -1 && virtualMap[i - 1, j] == 0)
                            {
                                virtualMap[i - 1, j] = 1;
                            }
                            if (i + 1 < 50 && virtualMap[i + 1, j] == 0)
                            {
                                virtualMap[i + 1, j] = 1;
                            }
                            if (j - 1 > -1 && virtualMap[i, j - 1] == 0)
                            {
                                virtualMap[i, j - 1] = 1;
                            }
                            if (j + 1 < 50 && virtualMap[i, j + 1] == 0)
                            {
                                virtualMap[i, j + 1] = 1;
                            }
                            virtualMap[i, j] = 2;
                        }
                    }
                }
            }

            if (!matrixContains(virtualMap, 0))
            {
                return true;
            }

            return false;
        }
        */
        static bool matrixContains(int[,] matrix, int prereq)
        { 
            for(int i = 0;i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    if (matrix[i, j] == prereq)
                        return true;
            return false;
        }
    }
}
