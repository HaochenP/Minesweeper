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

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<List<Cell>> map = new ObservableCollection<List<Cell>>();
        public void InitializeGame()
        {
            
            int width = 10;
            int height = 10;
            int mines = 50;
            string difficulty = "Easy";
            int[,] mineMap = new int[width, height];
            Random rand = new Random();
            int x = 0;
            int y = 0;
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
            map = randomShuffle(map);

        }

        public MainWindow()
        {
            InitializeGame();
            DataContext = map;
        }

        public bool checkIfFinished(ObservableCollection<List<Cell>> map)
        {
            var finished = true;
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    if (!cell.IsRevealed && !cell.IsMine)
                    {
                        finished = false;
                    }
                }
            }
            return finished;
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
            return map;
        }


        public void outputMap(ObservableCollection<List<Cell>> map)
        {
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    Trace.WriteLine(cell.X + " " + cell.Y + " " + cell.IsMine + " number of adjcent mines: " + cell.NeighbouringMines);
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
            ObservableCollection<List<Cell>> newMap = map;

            foreach (List<Cell> row in map.ToList())
            {
                foreach (Cell cell in row.ToList())
                {
                    int rand = random.Next(0, 100);
                    if (rand < 50)
                    {
                        int randoomRow = random.Next(newMap.Count);
                        int randomCol = random.Next(newMap[0].Count);
                        if (!newMap[randoomRow][randomCol].IsRevealed)
                        {
                            var result = swapCells(cell, newMap[randoomRow][randomCol]);
                            newMap[cell.X][cell.Y] = result.Item1;
                            newMap[randoomRow][randomCol] = result.Item2;
                        }
                    }
                }
            }
            outputMap(newMap);
            return newMap;

        }



        public Tuple<Cell,Cell> swapCells(Cell cell1, Cell cell2)
        {
            Cell temp = cell1;
            cell1 = cell2;
            cell2 = temp;
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

