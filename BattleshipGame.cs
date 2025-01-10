using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battleships
{
    internal class BattleshipGame
    {
        private const int BoardSize = 10;
        private const char Empty = '.';
        private const char Miss = 'O';
        private const char Hit = 'X';
        private const char Ship = 'S';

        private char[,] playerBoard;
        private char[,] computerBoard;
        private char[,] playerViewBoard;
        private char[,] computerViewBoard;

        private List<Ship> playerShips;
        private List<Ship> computerShips;

        private Random random;

        public BattleshipGame()
        {
            playerBoard = new char[BoardSize, BoardSize];
            computerBoard = new char[BoardSize, BoardSize];
            playerViewBoard = new char[BoardSize, BoardSize];
            computerViewBoard = new char[BoardSize, BoardSize];
            playerShips = new List<Ship>();
            computerShips = new List<Ship>();
            random = new Random();

            InitializeBoard(playerBoard);
            InitializeBoard(computerBoard);
            InitializeBoard(playerViewBoard);
            InitializeBoard(computerViewBoard);

            PlaceShips(computerBoard, computerShips);
        }

        private void InitializeBoard(char[,] board)
        {
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    board[i, j] = Empty;
                }
            }
        }

        private void PlaceShips(char[,] board, List<Ship> ships)
        {
            var shipSizes = new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            foreach (var size in shipSizes)
            {
                bool placed = false;
                while (!placed)
                {
                    int y = random.Next(BoardSize);
                    int x = random.Next(BoardSize);
                    bool horizontal = random.Next(2) == 0;

                    if (CanPlaceShip(board, y, x, size, horizontal))
                    {
                        PlaceShip(board, ships, y, x, size, horizontal);
                        placed = true;
                    }
                }
            }
        }

        private bool CanPlaceShip(char[,] board, int y, int x, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int ny = horizontal ? y : y + i;
                int nx = horizontal ? x + i : x;

                if (ny < 0 || ny >= BoardSize || nx < 0 || nx >= BoardSize || board[ny, nx] != Empty)
                    return false;

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int sy = ny + dy;
                        int sx = nx + dx;
                        if (sy >= 0 && sy < BoardSize && sx >= 0 && sx < BoardSize && board[sy, sx] == Ship)
                            return false;
                    }
                }
            }

            return true;
        }

        private void PlaceShip(char[,] board, List<Ship> ships, int y, int x, int size, bool horizontal)
        {
            Ship ship = new Ship(size);
            for (int i = 0; i < size; i++)
            {
                int ny = horizontal ? y : y + i;
                int nx = horizontal ? x + i : x;
                board[ny, nx] = Ship;
                ship.Positions.Add((ny, nx));
            }
            ships.Add(ship);
        }

        public void PlaceShipsManually()
        {
            Console.WriteLine("Place your ships on the board.");
            var shipSizes = new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

            foreach (var size in shipSizes)
            {
                bool placed = false;
                while (!placed)
                {
                    Console.WriteLine($"Place a ship of size {size}:");
                    DisplayBoard(playerBoard);

                    Console.WriteLine("Enter orientation (H for horizontal, V for vertical): ");
                    string orientationInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(orientationInput))
                    {
                        Console.WriteLine("Invalid input. Try again.");
                        continue;
                    }

                    char orientation = char.ToUpper(orientationInput[0]);
                    bool horizontal = orientation == 'H';

                    Console.WriteLine("Enter starting position (x y): ");
                    string[] input = Console.ReadLine()?.Split();
                    if (input == null || input.Length != 2 || !int.TryParse(input[0], out int x) || !int.TryParse(input[1], out int y))
                    {
                        Console.WriteLine("Invalid input. Try again.");
                        continue;
                    }

                    if (CanPlaceShip(playerBoard, y, x, size, horizontal))
                    {
                        PlaceShip(playerBoard, playerShips, y, x, size, horizontal);
                        placed = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid position. Try again.");
                    }
                }
            }
        }

        public void PlayerMove(int y, int x)
        {
            if (computerViewBoard[y, x] != Empty)
            {
                Console.WriteLine("You already shot there!");
                return;
            }

            if (computerBoard[y, x] == Ship)
            {
                computerViewBoard[y, x] = Hit;
                computerBoard[y, x] = Hit;
                Console.WriteLine("Hit!");
                CheckIfShipSunk(computerShips, y, x);
            }
            else
            {
                computerViewBoard[y, x] = Miss;
                Console.WriteLine("Miss!");
            }
        }

        public void ComputerMove()
        {
            int y, x;
            do
            {
                y = random.Next(BoardSize);
                x = random.Next(BoardSize);
            } while (playerViewBoard[y, x] != Empty);

            if (playerBoard[y, x] == Ship)
            {
                playerViewBoard[y, x] = Hit;
                playerBoard[y, x] = Hit;
                Console.WriteLine("Computer hit your ship at ({0}, {1})!", y, x);
                CheckIfShipSunk(playerShips, y, x);
            }
            else
            {
                playerViewBoard[y, x] = Miss;
                Console.WriteLine("Computer missed at ({0}, {1}).", y, x);
            }
        }

        private void CheckIfShipSunk(List<Ship> ships, int y, int x)
        {
            foreach (var ship in ships)
            {
                if (ship.Positions.Contains((y, x)))
                {
                    ship.Hits++;
                    if (ship.Hits == ship.Size)
                    {
                        Console.WriteLine("Ship sunk!");
                        MarkSurrounding(ship.Positions);
                    }
                    break;
                }
            }
        }

        private void MarkSurrounding(List<(int y, int x)> positions)
        {
            foreach (var (y, x) in positions)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int ny = y + dy;
                        int nx = x + dx;
                        if (ny >= 0 && ny < BoardSize && nx >= 0 && nx < BoardSize && playerViewBoard[ny, nx] == Empty)
                        {
                            playerViewBoard[ny, nx] = Miss;
                        }
                    }
                }
            }
        }

        public void DisplayBoard(char[,] board)
        {
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    Console.Write(board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        public bool IsGameOver()
        {
            return playerShips.TrueForAll(ship => ship.Hits == ship.Size) ||
                   computerShips.TrueForAll(ship => ship.Hits == ship.Size);
        }

        public void StartGame()
        {
            PlaceShipsManually();

            while (!IsGameOver())
            {
                Console.WriteLine("Your Board:");
                DisplayBoard(playerBoard);

                Console.WriteLine("Enemy Board:");
                DisplayBoard(computerViewBoard);

                Console.WriteLine("Enter your move (x y): ");
                string[] input = Console.ReadLine()?.Split();
                if (input == null || input.Length != 2 || !int.TryParse(input[0], out int x) || !int.TryParse(input[1], out int y))
                {
                    Console.WriteLine("Invalid input. Try again.");
                    continue;
                }

                PlayerMove(y, x);

                if (IsGameOver()) break;

                ComputerMove();
            }

            Console.WriteLine(IsGameOver() ? "Game Over!" : "Error: Game logic issue");
        }
    }
}
