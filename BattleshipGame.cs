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

        private readonly char[,] playerBoard = new char[BoardSize, BoardSize];
        private readonly char[,] computerBoard = new char[BoardSize, BoardSize];
        private readonly char[,] playerViewBoard = new char[BoardSize, BoardSize];
        private readonly char[,] computerViewBoard = new char[BoardSize, BoardSize];

        private readonly List<Ship> playerShips = new();
        private readonly List<Ship> computerShips = new();
        private readonly Random random = new();

        public BattleshipGame()
        {
            InitializeBoards();
            PlaceShips(computerBoard, computerShips);
        }

        private void InitializeBoards()
        {
            foreach (var board in new[] { playerBoard, computerBoard, playerViewBoard, computerViewBoard })
                for (int i = 0; i < BoardSize; i++)
                    for (int j = 0; j < BoardSize; j++)
                        board[i, j] = Empty;
        }

        private void PlaceShips(char[,] board, List<Ship> ships)
        {
            foreach (var size in new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 })
                while (true)
                {
                    int y = random.Next(BoardSize), x = random.Next(BoardSize);
                    bool horizontal = random.Next(2) == 0;
                    if (CanPlaceShip(board, y, x, size, horizontal))
                    {
                        PlaceShip(board, ships, y, x, size, horizontal);
                        break;
                    }
                }
        }

        private bool CanPlaceShip(char[,] board, int y, int x, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int ny = horizontal ? y : y + i;
                int nx = horizontal ? x + i : x;
                if (!IsInBounds(ny, nx) || board[ny, nx] != Empty || IsAdjacentToShip(board, ny, nx))
                    return false;
            }
            return true;
        }

        private static bool IsInBounds(int y, int x) => y >= 0 && y < BoardSize && x >= 0 && x < BoardSize;

        private static bool IsAdjacentToShip(char[,] board, int y, int x)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int ny = y + dy, nx = x + dx;
                    if (IsInBounds(ny, nx) && board[ny, nx] == Ship)
                        return true;
                }
            return false;
        }

        private void PlaceShip(char[,] board, List<Ship> ships, int y, int x, int size, bool horizontal)
        {
            var ship = new Ship(size);
            for (int i = 0; i < size; i++)
            {
                int ny = horizontal ? y : y + i;
                int nx = horizontal ? x + i : x;
                board[ny, nx] = Ship;
                ship.Positions.Add((ny, nx));
            }
            ships.Add(ship);
        }

        public void StartGame()
        {
            PlaceShipsManually();

            while (!IsGameOver())
            {
                DisplayBoards();
                PlayerMove();
                if (IsGameOver()) break;
                ComputerMove();
            }
            Console.WriteLine("Game Over!");
        }

        private void PlaceShipsManually()
        {
            foreach (var size in new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 })
            {
                while (true)
                {
                    Console.WriteLine($"Place a ship of size {size}:");
                    DisplayBoard(playerBoard);
                    Console.Write("Enter orientation (H for horizontal, V for vertical) and position (x y): ");
                    var input = Console.ReadLine()?.Split();

                    if (input == null || input.Length != 3 || !int.TryParse(input[1], out int x) || !int.TryParse(input[2], out int y))
                    {
                        Console.WriteLine("Invalid input. Try again.");
                        continue;
                    }

                    bool horizontal = input[0].Equals("H", StringComparison.OrdinalIgnoreCase);
                    if (CanPlaceShip(playerBoard, y, x, size, horizontal))
                    {
                        PlaceShip(playerBoard, playerShips, y, x, size, horizontal);
                        break;
                    }
                    Console.WriteLine("Invalid position. Try again.");
                }
            }
        }

        private void PlayerMove()
        {
            Console.Write("Enter your move (x y): ");
            var input = Console.ReadLine()?.Split();

            if (input == null || input.Length != 2 || !int.TryParse(input[0], out int x) || !int.TryParse(input[1], out int y) || !IsInBounds(y, x) || computerViewBoard[y, x] != Empty)
            {
                Console.WriteLine("Invalid move. Try again.");
                return;
            }

            ResolveMove(computerBoard, computerViewBoard, computerShips, y, x);
        }

        private void ComputerMove()
        {
            int y, x;
            do
            {
                y = random.Next(BoardSize);
                x = random.Next(BoardSize);
            } while (playerViewBoard[y, x] != Empty);

            ResolveMove(playerBoard, playerViewBoard, playerShips, y, x, isPlayer: false);
        }

        private void ResolveMove(char[,] targetBoard, char[,] viewBoard, List<Ship> ships, int y, int x, bool isPlayer = true)
        {
            if (targetBoard[y, x] == Ship)
            {
                targetBoard[y, x] = Hit;
                viewBoard[y, x] = Hit;
                Console.WriteLine(isPlayer ? "Hit!" : $"Computer hit your ship at ({y}, {x})!");
                CheckIfShipSunk(ships, y, x);
            }
            else
            {
                viewBoard[y, x] = Miss;
                Console.WriteLine(isPlayer ? "Miss!" : $"Computer missed at ({y}, {x}).");
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
                        foreach (var (sy, sx) in ship.Positions)
                            MarkSurrounding(playerViewBoard, sy, sx);
                    }
                    break;
                }
            }
        }

        private static void MarkSurrounding(char[,] board, int y, int x)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int ny = y + dy, nx = x + dx;
                    if (IsInBounds(ny, nx) && board[ny, nx] == Empty)
                        board[ny, nx] = Miss;
                }
        }

        private void DisplayBoards()
        {
            Console.WriteLine("Your Board:");
            DisplayBoard(playerBoard);
            Console.WriteLine("Enemy Board:");
            DisplayBoard(computerViewBoard);
        }

        private static void DisplayBoard(char[,] board)
        {
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                    Console.Write(board[i, j] + " ");
                Console.WriteLine();
            }
        }

        private bool IsGameOver() =>
            playerShips.TrueForAll(s => s.Hits == s.Size) ||
            computerShips.TrueForAll(s => s.Hits == s.Size);
    }
}
