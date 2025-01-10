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
                    int x = random.Next(BoardSize);
                    int y = random.Next(BoardSize);
                    bool horizontal = random.Next(2) == 0;

                    if (CanPlaceShip(board, x, y, size, horizontal))
                    {
                        PlaceShip(board, ships, x, y, size, horizontal);
                        placed = true;
                    }
                }
            }
        }

        private bool CanPlaceShip(char[,] board, int x, int y, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int nx = horizontal ? x : x + i;
                int ny = horizontal ? y + i : y;

                if (nx < 0 || nx >= BoardSize || ny < 0 || ny >= BoardSize || board[nx, ny] != Empty)
                    return false;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int sx = nx + dx;
                        int sy = ny + dy;
                        if (sx >= 0 && sx < BoardSize && sy >= 0 && sy < BoardSize && board[sx, sy] == Ship)
                            return false;
                    }
                }
            }

            return true;
        }

        private void PlaceShip(char[,] board, List<Ship> ships, int x, int y, int size, bool horizontal)
        {
            Ship ship = new Ship(size);
            for (int i = 0; i < size; i++)
            {
                int nx = horizontal ? x : x + i;
                int ny = horizontal ? y + i : y;
                board[nx, ny] = Ship;
                ship.Positions.Add((nx, ny));
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

                    if (CanPlaceShip(playerBoard, x, y, size, horizontal))
                    {
                        PlaceShip(playerBoard, playerShips, x, y, size, horizontal);
                        placed = true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid position. Try again.");
                    }
                }
            }
        }

        public void PlayerMove(int x, int y)
        {
            if (computerViewBoard[x, y] != Empty)
            {
                Console.WriteLine("You already shot there!");
                return;
            }

            if (computerBoard[x, y] == Ship)
            {
                computerViewBoard[x, y] = Hit;
                computerBoard[x, y] = Hit;
                Console.WriteLine("Hit!");
                CheckIfShipSunk(computerShips, x, y);
            }
            else
            {
                computerViewBoard[x, y] = Miss;
                Console.WriteLine("Miss!");
            }
        }

        public void ComputerMove()
        {
            int x, y;
            do
            {
                x = random.Next(BoardSize);
                y = random.Next(BoardSize);
            } while (playerViewBoard[x, y] != Empty);

            if (playerBoard[x, y] == Ship)
            {
                playerViewBoard[x, y] = Hit;
                playerBoard[x, y] = Hit;
                Console.WriteLine("Computer hit your ship at ({0}, {1})!", x, y);
                CheckIfShipSunk(playerShips, x, y);
            }
            else
            {
                playerViewBoard[x, y] = Miss;
                Console.WriteLine("Computer missed at ({0}, {1}).", x, y);
            }
        }

        private void CheckIfShipSunk(List<Ship> ships, int x, int y)
        {
            foreach (var ship in ships)
            {
                if (ship.Positions.Contains((x, y)))
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

        private void MarkSurrounding(List<(int x, int y)> positions)
        {
            foreach (var (x, y) in positions)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize && playerViewBoard[nx, ny] == Empty)
                        {
                            playerViewBoard[nx, ny] = Miss;
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

                PlayerMove(x, y);

                if (IsGameOver()) break;

                ComputerMove();
            }

            Console.WriteLine(IsGameOver() ? "Game Over!" : "Error: Game logic issue");
        }
    }
}
