using System.Runtime.Intrinsics.X86;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Input;

namespace Snek
{
    internal class Program
    {
        struct Position
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
                if (item1 == item2)
                {
                    return false;
                }
                return true;
            }
        }

        static int framecount = 0;
        static char[,] Map;
        static Queue<Position> Snake;
        static bool skip = false;
        static Position Apple;
        static List<Position> allPos;
        static bool stopKeyCheck = false;
        static ConsoleKey currentKey = ConsoleKey.W;
        static Thread keyCheck;
        static void Main(string[] args)
        {
            Snake = new Queue<Position>();
            Snake.Enqueue(new Position(18, 25));
            Snake.Enqueue(new Position(17, 25));
            Snake.Enqueue(new Position(16, 25));
            Snake.Enqueue(new Position(15, 25));
            keyCheck = new Thread(new ThreadStart(getCurrentKey));
            keyCheck.Start();
            Map = new char[50, 50];
            generateAllPos();
            placeApple();
            while (true)
            {
                Map = new char[50, 50];
                move();
                placeObjects();
                isDead();
                if (keyCheck.ThreadState == ThreadState.Stopped || keyCheck.ThreadState == ThreadState.Unstarted)
                {
                    keyCheck = new Thread(new ThreadStart(getCurrentKey));
                    keyCheck.Start();
                }
                applePickedUp();
                Draw();
                Thread.Sleep(100);
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
            Apple = tmp[r.Next(0, tmp.Length)];

            /*while (true) 
            {   
                Position aPos = new Position(r.Next(0, Map.GetLength(0)), r.Next(0, Map.GetLength(1)));
                if (!Snake.Contains(aPos))
                {
                    Apple = aPos;
                    break;
                }
            }*/
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
            Map[Apple.x, Apple.y] = 'O';
        }

        static void move()
        {
            switch (currentKey)
            {
                //felfele
                case ConsoleKey.W:
                    Position head = Snake.ToArray()[Snake.Count - 1];
                    if (head.x - 1 < 0)
                        Snake.Enqueue(new Position(49, head.y));
                    else
                        Snake.Enqueue(new Position(head.x - 1, head.y));
                    break;
                //bal
                case ConsoleKey.A:
                    head = Snake.ToArray()[Snake.Count - 1];
                    if (head.y - 1 < 0)
                        Snake.Enqueue(new Position(head.x, 49));
                    else
                        Snake.Enqueue(new Position(head.x, head.y - 1));
                    break;
                //lefele
                case ConsoleKey.S:
                    head = Snake.ToArray()[Snake.Count - 1];
                    if (head.x + 1 > 49)
                        Snake.Enqueue(new Position(0, head.y));
                    else
                        Snake.Enqueue(new Position(head.x + 1, head.y));
                    break;
                //jobb
                case ConsoleKey.D:
                    head = Snake.ToArray()[Snake.Count - 1];
                    if (head.y + 1 > 49)
                        Snake.Enqueue(new Position(head.x, 0));
                    else
                        Snake.Enqueue(new Position(head.x, head.y + 1));
                    break;
            }
            if (!skip)
                Snake.Dequeue();
            skip = false;
        }

        static void getCurrentKey()
        {
            Console.Beep();
            while (!stopKeyCheck)
            {
                switch (Console.ReadKey(true).Key)
                {
                    //felfele
                    case ConsoleKey.W:
                        currentKey = ConsoleKey.W;
                        break;
                    //bal
                    case ConsoleKey.A:
                        currentKey = ConsoleKey.A;
                        break;
                    //lefele
                    case ConsoleKey.S:
                        currentKey = ConsoleKey.S;
                        break;
                    //jobb
                    case ConsoleKey.D:
                        currentKey = ConsoleKey.D;
                        break;
                }
                Thread.Sleep(1);
            }
            return;
        }

        static void gameOver()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Draw();
            stopKeyCheck = true;
            if (!(Console.ReadKey(true).Key == ConsoleKey.R))
            {
                Environment.Exit(0);
            }
            Console.ForegroundColor= ConsoleColor.Green;
            Snake = new Queue<Position>();
            Snake.Enqueue(new Position(18, 25));
            Snake.Enqueue(new Position(17, 25));
            Snake.Enqueue(new Position(16, 25));
            Snake.Enqueue(new Position(15, 25));
            currentKey = ConsoleKey.W;
            placeApple();
            framecount = 0;
            stopKeyCheck = false;
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
            frame += $"Current Length: {Snake.Count} - Frame: {framecount++} - {keyCheck.ThreadState}\n";
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