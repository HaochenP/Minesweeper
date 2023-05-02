using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using Minesweeper;
using System.Windows.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<List<Cell>> map = new ObservableCollection<List<Cell>>();
        int width = 10;
        int height = 10;
        string difficulty = "Easy";
        int mines = 50;
        private DispatcherTimer shuffleTimer;
        private int elapsedTime;
        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            var viewModel = new MainViewModel { Map = map };
            DataContext = viewModel;
        }



        public void InitializeGame()
        {
            Tuple<int, int, int> gameSettings = gameSetting(difficulty);
            map = new ObservableCollection<List<Cell>>();
            width = gameSettings.Item1;
            height = gameSettings.Item2;
            mines = gameSettings.Item3;
            elapsedTime = 5;
            int unRevealedSquares = width * height;

            for (int i = 0; i < width; i++)
            {
                List<Cell> row = new List<Cell>();
                for (int j = 0; j < height; j++)
                {
                    row.Add(new Cell(i, j, difficulty));
                }
                map.Add(row);
            }

            map = PlaceMines(mines, map);
            //map = randomShuffle(map);

            var viewModel = new MainViewModel { Map = map };
            DataContext = viewModel;

            if (shuffleTimer != null)
            {
                shuffleTimer.Stop();
            }
            else
            {
                shuffleTimer = new DispatcherTimer();
                shuffleTimer.Tick += ShuffleTimerTick;
                

            }
            shuffleTimer.Interval = TimeSpan.FromSeconds(1);
            shuffleTimer.Start();

        }



        private void ShuffleTimerTick(object sender, EventArgs e)
        {
            elapsedTime--;
            TimerLabel.Content = $"Time left until shuffle : {elapsedTime}s";
            if (elapsedTime == 0)
            {
                map = randomShuffle(map);
                var viewModel = (MainViewModel)DataContext;
                viewModel.Map = map;
                elapsedTime = 5;
                TimerLabel.Content = $"Time left until shuffle : {elapsedTime}s";
            }
            
        }




        void button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Cell cell = (Cell)button.DataContext;
            RevealCell(cell);
        }

        public void OnDifficultyMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string newDifficulty = menuItem.Header.ToString();
                ChangeDifficulty(newDifficulty);
            }
        }

        public void NewGame(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        public void ChangeDifficulty(string newDifficulty)
        {
            // Update the difficulty value
            map = new ObservableCollection<List<Cell>>();
            difficulty = newDifficulty;

            // Reinitialize the game with the new difficulty
            InitializeGame();
        }

        public Tuple<int, int, int> gameSetting(string difficulty)
        {
            if (difficulty == "Easy")
            {
                return Tuple.Create(9, 9, 10);
            }
            else if (difficulty == "Medium")
            {
                return Tuple.Create(16, 16, 40);
            }
            else
            {
                return Tuple.Create(16, 30, 99);
            }
        }

        public void RevealCell(Cell cell)
        {
            if (!cell.IsMine)
            {
                if (!cell.IsRevealed)
                {
                    cell.Reveal();
                    if (CheckIfFinished(map))
                    {
                        MessageBox.Show("You Won");
                        InitializeGame();
                    }
                }
            }
            else
            {
                MessageBox.Show("You Lost");
                InitializeGame();
            }
        }

        public bool CheckIfFinished(ObservableCollection<List<Cell>> map)
        {
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    if(!cell.IsRevealed && !cell.IsMine)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public ObservableCollection<List<Cell>> PlaceMines(int minesLeft, ObservableCollection<List<Cell>> map)
        {
            Random random = new Random();
            int minePlaced = 0;

            while (minePlaced < minesLeft)
            {
                int row = random.Next(map.Count);
                int col = random.Next(map[0].Count);
                if (!map[row][col].IsMine)
                {
                    map[row][col].IsMine = true;
                    minePlaced++;
                    map[row][col].UpdateAdjacentMinesForNeighbors(row, col, map);
                }
            }
            outputMap(map);
            return map;
        }

        public void outputMap(ObservableCollection<List<Cell>> map)
        {
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    Trace.WriteLine(cell.X + " " + cell.Y + " " + cell.IsMine + " number of adjcent mines: " + cell.NeighbouringMines);
                    Console.WriteLine(cell.X + " " + cell.Y + " " + cell.IsMine + " number of adjcent mines: " + cell.NeighbouringMines);
                }
                Console.WriteLine();
            }
        }

        public void revealedMines(ObservableCollection<List<Cell>> map)
        {
            List<Cell> revealedMines = new List<Cell>();
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    if (cell.IsRevealed)
                    {
                        revealedMines.Add(cell);
                    }
                }
            }
        }

        public ObservableCollection<List<Cell>> randomShuffle(ObservableCollection<List<Cell>> map)
        {
            Random random = new Random();
            ObservableCollection<List<Cell>> newMap = DeepCopy(map);

            foreach (List<Cell> row in newMap.ToList())
            {
                foreach (Cell cell in row.ToList())
                {
                    int rand = random.Next(0, 100);
                    if (rand < 50)
                    {
                        int randomRow = random.Next(newMap.Count);
                        int randomCol = random.Next(newMap[0].Count);
                        int currentCellRow = cell.X;
                        int currentCellCol = cell.Y;
                        if (!newMap[randomRow][randomCol].IsRevealed && !newMap[currentCellRow][currentCellCol].IsRevealed)
                        {
                            var result = swapCells(cell, newMap[randomRow][randomCol]);
                            newMap[currentCellRow][currentCellCol] = result.Item1;
                            newMap[randomRow][randomCol] = result.Item2;
                        }
                    }
                }
            }
            outputMap(newMap);
            return newMap;
        }

        public ObservableCollection<List<Cell>> DeepCopy(ObservableCollection<List<Cell>> map)
        {
            ObservableCollection<List<Cell>> newMap = new ObservableCollection<List<Cell>>();
            foreach (List<Cell> row in map)
            {
                List<Cell> newRow = new List<Cell>();
                foreach (Cell cell in row)
                {
                    newRow.Add(new Cell(cell.X, cell.Y, cell.Difficulty)
                    {
                        IsMine = cell.IsMine,
                        IsRevealed = cell.IsRevealed,
                        IsFlagged = cell.IsFlagged
                    });
                }
                newMap.Add(newRow);
            }
            return newMap;
        }


        public Tuple<Cell, Cell> swapCells(Cell cell1, Cell cell2)
        {
            // Swap X and Y coordinates
            int tempX = cell1.X;
            int tempY = cell1.Y;
            cell1.X = cell2.X;
            cell1.Y = cell2.Y;
            cell2.X = tempX;
            cell2.Y = tempY;

            // Swap other cell properties
            bool tempIsMine = cell1.IsMine;
            int tempNeighbouringMines = cell1.NeighbouringMines;
            bool tempIsRevealed = cell1.IsRevealed;
            bool tempIsFlagged = cell1.IsFlagged;

            cell1.IsMine = cell2.IsMine;
            cell1.NeighbouringMines = cell2.NeighbouringMines;
            cell1.IsRevealed = cell2.IsRevealed;
            cell1.IsFlagged = cell2.IsFlagged;

            cell2.IsMine = tempIsMine;
            cell2.NeighbouringMines = tempNeighbouringMines;
            cell2.IsRevealed = tempIsRevealed;
            cell2.IsFlagged = tempIsFlagged;

            return Tuple.Create(cell1, cell2);
        }


    }

    public class Cell
    {
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsMine { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }
        public int NeighbouringMines { get; set; }

        public string Difficulty { get; set; }

        public Cell(int x, int y, string difficulty)
        {
            X = x;
            Y = y;
            IsMine = false;
            IsRevealed = false;
            IsFlagged = false;
            NeighbouringMines = 0;
            Difficulty = difficulty;
        }

        public void Reveal()
        {
            IsRevealed = true;
        }

        public void ToggleFlag()
        {
            IsFlagged = !IsFlagged;
        }

        public void UpdateAdjacentMinesForNeighbors(int row, int col, ObservableCollection<List<Cell>> map)
        {
            int[] neighbors = { -1, 0, 1 };

            foreach (int rowOffset in neighbors)
            {
                foreach (int colOffset in neighbors)
                {
                    if (rowOffset == 0 && colOffset == 0)
                        continue;

                    int newRow = row + rowOffset;
                    int newCol = col + colOffset;

                    if (newRow >= 0 && newRow < map.Count && newCol >= 0 && newCol < map[0].Count)
                    {
                        map[newRow][newCol].NeighbouringMines++;
                    }
                }
            }
        }
    }
}

