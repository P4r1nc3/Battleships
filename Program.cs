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
                graphics.PrintBoard(playerBoard, computerBoard, playerTurn);

                if (playerTurn)
                {
                    graphics.PrintMessage("Your turn. Enter coordinates to fire (e.g., 05):");
                    string input = Console.ReadLine();
                    if (!IsValidFireInput(input))
                    {
                        graphics.PrintMessage("Invalid input. Enter two digits (e.g., 05 or 98).");
                        continue;
                    }
                    (int x, int y) = ParseFireCoordinates(input);

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

            graphics.PrintBoard(playerBoard, computerBoard, true);
            graphics.PrintMessage(playerBoard.AllShipsSunk() ? "Computer wins!" : "You win!");
        }

        private bool IsValidFireInput(string input)
        {
            if (input.Length != 2) return false;
            return char.IsDigit(input[0]) && char.IsDigit(input[1]);
        }

        private (int, int) ParseFireCoordinates(string input)
        {
            int x = input[0] - '0';
            int y = input[1] - '0';
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
            graphics.PrintBoard(this, null, true);
            AddShip(graphics, "four-mast", 4);
            for (int i = 0; i < 2; i++)
            {
                graphics.PrintBoard(this, null, true);
                AddShip(graphics, "three-mast", 3);
            }
            for (int i = 0; i < 3; i++)
            {
                graphics.PrintBoard(this, null, true);
                AddShip(graphics, "two-mast", 2);
            }
            for (int i = 0; i < 4; i++)
            {
                graphics.PrintBoard(this, null, true);
                AddShip(graphics, "one-mast", 1);
            }
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
                if (!IsValidPlacementInput(input))
                {
                    graphics.PrintMessage("Invalid input. Use H/V followed by two digits.");
                    continue;
                }
                char direction = input[0];
                int x = input[1] - '0';
                int y = input[2] - '0';

                if (CanPlaceShip(x, y, size, direction == 'H'))
                {
                    PlaceShip(x, y, size, direction == 'H');
                    graphics.PrintBoard(this, null, true);
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

        private bool IsValidPlacementInput(string input)
        {
            if (input.Length != 3) return false;
            if (input[0] != 'H' && input[0] != 'V') return false;
            if (!char.IsDigit(input[1]) || !char.IsDigit(input[2])) return false;
            return true;
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int dx = horizontal ? i : 0;
                int dy = horizontal ? 0 : i;

                if (x + dx >= BoardSize || y + dy >= BoardSize || grid[x + dx, y + dy] != '.')
                    return false;

                if (!IsSurroundingAreaClear(x + dx, y + dy))
                    return false;
            }

            return true;
        }

        private bool IsSurroundingAreaClear(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize && grid[nx, ny] == 'S')
                    {
                        return false;
                    }
                }
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

        public char[,] GetHiddenGrid()
        {
            var hiddenGrid = new char[BoardSize, BoardSize];
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    hiddenGrid[i, j] = (grid[i, j] == 'S') ? '.' : grid[i, j];
                }
            }
            return hiddenGrid;
        }
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

        public void PrintBoard(Board playerBoard, Board computerBoard, bool showAll)
        {
            Console.WriteLine("Player Board:");
            PrintGrid(playerBoard.GetGrid());

            if (computerBoard != null)
            {
                Console.WriteLine("Computer Board:");
                PrintGrid(showAll ? computerBoard.GetGrid() : computerBoard.GetHiddenGrid());
            }
        }

        private void PrintGrid(char[,] grid)
        {
            Console.WriteLine("  0 1 2 3 4 5 6 7 8 9");
            for (int y = 0; y < 10; y++)
            {
                Console.Write(y + " ");
                for (int x = 0; x < 10; x++)
                {
                    Console.Write(grid[x, y] + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
