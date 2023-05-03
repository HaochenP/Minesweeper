using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    public class Cell : CellViewModel
    {
        public int X { get; set; }
        public int Y { get; set; }

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
