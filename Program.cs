using System;
using System.Collections.Generic;
using System.Linq;

namespace Battleships
{
    internal class Program
    {
        public static void Main()
        {
            new BattleshipGame().StartGame();
        }
    }

    public class BattleshipGame
    {
        private Board playerBoard;
        private Board computerBoard;
        private Graphics graphics;
        private Random random;

        public BattleshipGame()
        {
            playerBoard = new Board("Player");
            computerBoard = new Board("Computer");
            graphics = new Graphics();
            random = new Random();
        }

        public void StartGame()
        {
            graphics.PrintMessage("Welcome to Battleships!");

            graphics.PrintMessage("Setting up player board.");
            playerBoard.SetupBoardFromInput(graphics);

            graphics.PrintMessage("Setting up computer board.");
            computerBoard.SetupBoardRandom(random);

            bool playerTurn = true;

            while (!playerBoard.AllShipsSunk() && !computerBoard.AllShipsSunk())
            {
                graphics.PrintBoard(playerBoard, computerBoard);

                if (playerTurn)
                {
                    graphics.PrintMessage("Your turn. Enter coordinates to fire (e.g., A5):");
                    string input = Console.ReadLine();
                    (int x, int y) = ParseCoordinates(input);

                    if (computerBoard.FireAt(x, y))
                    {
                        graphics.PrintMessage("Hit!");
                    }
                    else
                    {
                        graphics.PrintMessage("Miss.");
                        playerTurn = false;
                    }
                }
                else
                {
                    graphics.PrintMessage("Computer's turn.");
                    (int x, int y) = computerBoard.GetComputerMove(random);

                    if (playerBoard.FireAt(x, y))
                    {
                        graphics.PrintMessage($"Computer hit your ship at {x},{y}!");
                    }
                    else
                    {
                        graphics.PrintMessage($"Computer missed at {x},{y}.");
                        playerTurn = true;
                    }
                }
            }

            graphics.PrintBoard(playerBoard, computerBoard);
            graphics.PrintMessage(playerBoard.AllShipsSunk() ? "Computer wins!" : "You win!");
        }

        private (int, int) ParseCoordinates(string input)
        {
            char letter = input[0];
            int x = letter - 'A';
            int y = int.Parse(input.Substring(1));
            return (x, y);
        }
    }

    public class Board
    {
        private const int BoardSize = 10;
        private readonly string owner;
        private readonly char[,] grid;
        private readonly List<Ship> ships;

        public Board(string owner)
        {
            this.owner = owner;
            grid = new char[BoardSize, BoardSize];
            ships = new List<Ship>();

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    grid[i, j] = '.';
                }
            }
        }

        public void SetupBoardFromInput(Graphics graphics)
        {
            AddShip(graphics, "four-mast", 4);
            for (int i = 0; i < 2; i++) AddShip(graphics, "three-mast", 3);
            for (int i = 0; i < 3; i++) AddShip(graphics, "two-mast", 2);
            for (int i = 0; i < 4; i++) AddShip(graphics, "one-mast", 1);
        }

        public void SetupBoardRandom(Random random)
        {
            AddShipRandom(random, 4);
            for (int i = 0; i < 2; i++) AddShipRandom(random, 3);
            for (int i = 0; i < 3; i++) AddShipRandom(random, 2);
            for (int i = 0; i < 4; i++) AddShipRandom(random, 1);
        }

        private void AddShip(Graphics graphics, string name, int size)
        {
            while (true)
            {
                graphics.PrintMessage($"Place your {name} (size {size}) (e.g., H09):");
                string input = Console.ReadLine();
                char direction = input[0];
                int x = input[1] - '0';
                int y = input[2] - '0';

                if (CanPlaceShip(x, y, size, direction == 'H'))
                {
                    PlaceShip(x, y, size, direction == 'H');
                    break;
                }

                graphics.PrintMessage("Invalid placement. Try again.");
            }
        }

        private void AddShipRandom(Random random, int size)
        {
            while (true)
            {
                int x = random.Next(BoardSize);
                int y = random.Next(BoardSize);
                bool horizontal = random.Next(2) == 0;

                if (CanPlaceShip(x, y, size, horizontal))
                {
                    PlaceShip(x, y, size, horizontal);
                    break;
                }
            }
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int dx = horizontal ? i : 0;
                int dy = horizontal ? 0 : i;

                if (x + dx >= BoardSize || y + dy >= BoardSize || grid[x + dx, y + dy] != '.')
                    return false;
            }

            return true;
        }

        private void PlaceShip(int x, int y, int size, bool horizontal)
        {
            Ship ship = new Ship(size);
            ships.Add(ship);

            for (int i = 0; i < size; i++)
            {
                int dx = horizontal ? i : 0;
                int dy = horizontal ? 0 : i;

                grid[x + dx, y + dy] = 'S';
                ship.AddPosition(x + dx, y + dy);
            }
        }

        public bool FireAt(int x, int y)
        {
            if (grid[x, y] == 'S')
            {
                grid[x, y] = 'X';
                foreach (var ship in ships)
                {
                    ship.Hit(x, y);
                }
                return true;
            }

            if (grid[x, y] == '.')
            {
                grid[x, y] = 'O';
            }

            return false;
        }

        public (int, int) GetComputerMove(Random random)
        {
            while (true)
            {
                int x = random.Next(BoardSize);
                int y = random.Next(BoardSize);

                if (grid[x, y] == '.' || grid[x, y] == 'S')
                {
                    return (x, y);
                }
            }
        }

        public bool AllShipsSunk()
        {
            return ships.All(ship => ship.IsSunk);
        }

        public char[,] GetGrid() => grid;
    }

    public class Ship
    {
        private readonly HashSet<(int, int)> positions;
        private readonly HashSet<(int, int)> hits;

        public Ship(int size)
        {
            positions = new HashSet<(int, int)>();
            hits = new HashSet<(int, int)>();
        }

        public void AddPosition(int x, int y)
        {
            positions.Add((x, y));
        }

        public void Hit(int x, int y)
        {
            if (positions.Contains((x, y)))
            {
                hits.Add((x, y));
            }
        }

        public bool IsSunk => positions.SetEquals(hits);
    }

    public class Graphics
    {
        public void PrintMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void PrintBoard(Board playerBoard, Board computerBoard)
        {
            Console.WriteLine("Player Board:");
            PrintGrid(playerBoard.GetGrid());

            Console.WriteLine("Computer Board:");
            PrintGrid(computerBoard.GetGrid());
        }

        private void PrintGrid(char[,] grid)
        {
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Console.Write(grid[x, y] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
