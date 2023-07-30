﻿using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Input;

namespace Snek
{
    internal class Program
    {
        public struct Position
        {
            public Position(int x, int y)
            { 
                this.x = x; 
                this.y = y;
            }

            public int x; 
            public int y;

            public static bool operator ==(Position item1, Position item2)
            { 
                if (item1.x == item2.x && item1.y == item2.y) { return true; }
                return false;
            }
            public static bool operator !=(Position item1, Position item2)
            {
                return !(item1 == item2);
            }
        }

        static int framecount = 0;
        static char[,] Map;
        static Queue<Position> Snake;
        static bool skip = false;
        static Position Apple;
        static List<Position> allPos;
        static void Main(string[] args)
        {
            Snake = new Queue<Position>();
            Snake.Enqueue(new Position(18, 25));
            Snake.Enqueue(new Position(17, 25));
            Snake.Enqueue(new Position(16, 25));
            Snake.Enqueue(new Position(15, 25));
            Map = new char[50, 50];
            generateAllPos();
            placeApple();
            var reallySmallTimeSpan = new TimeSpan(ticks: 1);
            while (true)
            {
                Map = new char[50, 50];
                move();
                placeObjects();
                //isDead();
                applePickedUp();
                Draw();
                Thread.Sleep(reallySmallTimeSpan);
            }
        }

        static void generateAllPos()
        {
            allPos = new List<Position>();
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int j = 0; j < Map.GetLength(1); j++)
                {
                    allPos.Add(new Position(i, j));
                }
            }
        }

        static void placeApple()
        {
            Random r = new Random();
            var tmp = allPos.Except(Snake).ToArray();
            if (tmp.Length == 0) {
                Draw();
                Apple.x = -1;
                placeObjects();
                Draw();
                Console.WriteLine("Snake wins!");
                Console.ReadKey();
                Environment.Exit(0);
            }
            //Apple = new Position(Snake.ToArray()[Snake.Count - 1].x, (Snake.ToArray()[Snake.Count - 1].y + 1) % 50);
            Apple = tmp[r.Next(0, tmp.Length)];
        }

        static void applePickedUp()
        {
            if (Snake.Contains(Apple))
            {
                skip = true;
                placeApple();
            }
        }

        static void placeObjects()
        { 
            foreach (var position in Snake)
            {
                Map[position.x, position.y] = 'X';
            }
            if (Apple.x != -1)
                Map[Apple.x, Apple.y] = 'O';
        }

        static void move()
        {
            var lSnake = Snake.ToArray();
            if (lSnake[lSnake.Length - 1].y == 49 - lSnake[lSnake.Length - 1].x && lSnake[lSnake.Length - 1].x == lSnake[lSnake.Length - 3].x)
                Snake.Enqueue(new Position((lSnake[lSnake.Length - 1].x + 1) % 50, lSnake[lSnake.Length - 1].y));
            else
                Snake.Enqueue(new Position(lSnake[lSnake.Length - 1].x, (lSnake[lSnake.Length - 1].y + 1) % 50));
            if (!skip)
                Snake.Dequeue();
            skip = false;
        }

        static void gameOver()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ded");
            Console.ReadKey();
        }

        static void isDead()
        {
            for (int i = 0; i < Snake.Count; i++)
            {
                for (int j = 0; j < Snake.Count; j++)
                {
                    if (i != j && Snake.ToArray()[i] == Snake.ToArray()[j])
                    {
                        gameOver();
                    }
                }
            }
        }

        static void Draw()
        {
            string frame = "";
            frame += $"Current Length: {Snake.Count} - Frame: {framecount++}\n";
            frame += "╔";
            for (int i = 0; i < 50; i++)
            {
                frame += "═";
            }
            frame += "╗";
            for (int i = 0; i < Map.GetLength(0); i++) 
            {
                frame += "║";
                for (int j = 0; j < Map.GetLength(1); j++)
                {
                    if (Map[i, j] == '\0')
                    {
                        frame += ' ';
                    }
                    else
                    frame += Map[i, j];
                }
                frame += "║";   
                frame += "\n";
            }
            frame += "╚";
            for (int i = 0; i < 50; i++)
            {
                frame += "═";
            }
            frame += "╝";
            for (int i = 0; i < 52; i++)
            {
                frame += " ";
            }
            if ((framecount * 53) % 5300 == 0)
                Console.Clear();
            Console.SetCursorPosition(0, (framecount * 53) % 5300);
            Console.Write(frame);
        }
    }
}