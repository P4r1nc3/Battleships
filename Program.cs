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

    internal class BattleshipGame
    {
        private Board playerBoard;
        private Board computerBoard;
        private Random random;
        private Graphic graphic;

        public BattleshipGame()
        {
            playerBoard = new Board();
            computerBoard = new Board();
            random = new Random();
            graphic = new Graphic();
        }

        public void StartGame()
        {
            Console.WriteLine("Welcome to Battleships!");

            // Player places ships
            Console.WriteLine("Place your ships on the board:");
            playerBoard.PlaceShipsManually(graphic);

            // Computer places ships
            computerBoard.PlaceShipsAutomatically(random);

            // Start the game loop
            bool gameOver = false;
            while (!gameOver)
            {
                Console.Clear();

                // Display turn feedback
                if (!string.IsNullOrEmpty(graphic.LastPlayerMove))
                {
                    Console.WriteLine($"You fired at {graphic.LastPlayerMove}: {(graphic.LastPlayerHit ? "Hit" : "Miss")}");
                }

                if (!string.IsNullOrEmpty(graphic.LastComputerMove))
                {
                    Console.WriteLine($"Computer fired at {graphic.LastComputerMove}: {(graphic.LastComputerHit ? "Hit" : "Miss")}");
                }

                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Your board:");
                graphic.DisplayBoard(playerBoard, true);

                Console.WriteLine("Computer's board:");
                graphic.DisplayBoard(computerBoard, false);

                // Player's turn
                Console.WriteLine("Your turn! Enter coordinates to fire (e.g., 34):");
                string? input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || !computerBoard.FireAt(input))
                {
                    Console.WriteLine("Invalid input or already fired there. Try again.");
                    continue;
                }
                graphic.LastPlayerMove = input;
                graphic.LastPlayerHit = computerBoard.WasLastHit();

                // Check if computer lost
                if (computerBoard.AreAllShipsSunk())
                {
                    Console.WriteLine("You won!");
                    gameOver = true;
                    break;
                }

                // Computer's turn
                Console.WriteLine("Computer's turn...");
                string computerMove;
                do
                {
                    int x = random.Next(0, 10);
                    int y = random.Next(0, 10);
                    computerMove = x.ToString() + y.ToString();
                } while (!playerBoard.FireAt(computerMove));

                graphic.LastComputerMove = computerMove;
                graphic.LastComputerHit = playerBoard.WasLastHit();

                Console.WriteLine();

                // Check if player lost
                if (playerBoard.AreAllShipsSunk())
                {
                    Console.WriteLine("You lost!");
                    gameOver = true;
                }
            }
        }
    }

    internal class Board
    {
        private const int Size = 10;
        private readonly Cell[,] grid;
        private readonly List<Ship> ships;
        private bool lastHit;

        public Board()
        {
            grid = new Cell[Size, Size];
            ships = new List<Ship>();
            for (int x = 0; x < Size; x++)
                for (int y = 0; y < Size; y++)
                    grid[x, y] = new Cell();
        }

        public void PlaceShipsManually(Graphic graphic)
        {
            foreach (var shipSize in new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 })
            {
                bool placed = false;
                while (!placed)
                {
                    Console.WriteLine($"Place a ship of size {shipSize} (e.g., H09 or V85):");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input) || !TryPlaceShip(input, shipSize))
                    {
                        Console.WriteLine("Invalid placement. Try again.");
                        continue;
                    }
                    placed = true;
                }
                graphic.DisplayBoard(this, true);
            }
        }

        public void PlaceShipsAutomatically(Random random)
        {
            foreach (var shipSize in new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 })
            {
                bool placed = false;
                while (!placed)
                {
                    string orientation = random.Next(0, 2) == 0 ? "H" : "V";
                    int x = random.Next(0, 10);
                    int y = random.Next(0, 10);
                    placed = TryPlaceShip(orientation + x.ToString() + y.ToString(), shipSize);
                }
            }
        }

        private bool TryPlaceShip(string input, int size)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 3)
                return false;

            char orientation = input[0];
            if (orientation != 'H' && orientation != 'V')
                return false;

            if (!int.TryParse(input.Substring(1, 1), out int x) || !int.TryParse(input.Substring(2, 1), out int y))
                return false;

            int dx = orientation == 'H' ? 1 : 0;
            int dy = orientation == 'V' ? 1 : 0;

            // Check if ship fits and doesn't overlap or touch others
            for (int i = 0; i < size; i++)
            {
                int nx = x + i * dx;
                int ny = y + i * dy;
                if (nx < 0 || ny < 0 || nx >= Size || ny >= Size || !IsCellAvailable(nx, ny))
                    return false;
            }

            // Place ship
            Ship ship = new Ship(size);
            for (int i = 0; i < size; i++)
            {
                int nx = x + i * dx;
                int ny = y + i * dy;
                grid[nx, ny].HasShip = true;
                ship.Cells.Add(grid[nx, ny]);
            }
            ships.Add(ship);
            return true;
        }

        private bool IsCellAvailable(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && ny >= 0 && nx < Size && ny < Size && grid[nx, ny].HasShip)
                        return false;
                }
            return true;
        }

        public bool FireAt(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length != 2)
                return false;

            if (!int.TryParse(input[0].ToString(), out int x) || !int.TryParse(input[1].ToString(), out int y))
                return false;

            if (x < 0 || y < 0 || x >= Size || y >= Size || grid[x, y].IsHit)
                return false;

            lastHit = grid[x, y].HasShip;
            grid[x, y].IsHit = true;
            return true;
        }

        public bool WasLastHit() => lastHit;

        public bool AreAllShipsSunk()
        {
            return ships.All(ship => ship.IsSunk);
        }

        public Cell[,] GetGrid() => grid;

        public int GetSize() => Size;
    }

    internal class Cell
    {
        public bool HasShip { get; set; }
        public bool IsHit { get; set; }
    }

    internal class Ship
    {
        public List<Cell> Cells { get; }

        public Ship(int size)
        {
            Cells = new List<Cell>(size);
        }

        public bool IsSunk => Cells.All(cell => cell.IsHit);
    }

    internal class Graphic
    {
        public string LastPlayerMove { get; set; } = string.Empty;
        public bool LastPlayerHit { get; set; }
        public string LastComputerMove { get; set; } = string.Empty;
        public bool LastComputerHit { get; set; }

        public void DisplayBoard(Board board, bool showShips)
        {
            Console.WriteLine("  0 1 2 3 4 5 6 7 8 9");
            Cell[,] grid = board.GetGrid();
            int size = board.GetSize();

            for (int y = 0; y < size; y++)
            {
                Console.Write(y + " ");
                for (int x = 0; x < size; x++)
                {
                    if (grid[x, y].IsHit)
                        Console.Write(grid[x, y].HasShip ? "X " : "M ");
                    else if (grid[x, y].HasShip && showShips)
                        Console.Write("S ");
                    else
                        Console.Write(". ");
                }
                Console.WriteLine();
            }
        }
    }
}
