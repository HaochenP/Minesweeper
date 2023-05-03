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
using System.Globalization;

namespace Minesweeper
{

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
        private MediaPlayer mediaPlayer = new MediaPlayer();


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
            // Timer functionality
            /*
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
            shuffleTimer.Start(); */

        }



        private void ShuffleTimerTick(object sender, EventArgs e)
        {
            elapsedTime--;
            TimerLabel.Content = $"Time left until shuffle : {elapsedTime}s";
            if (elapsedTime == 0)
            {
                map = RandomShuffle(map);
                UpdateNeighbourMines(map);
                var viewModel = (MainViewModel)DataContext;
                viewModel.Map = map;
                elapsedTime = 5;
                TimerLabel.Content = $"Time left until shuffle : {elapsedTime}s";
            }
            
        }

        void Flag(object sender, MouseButtonEventArgs e)
        {
            GetRickRolled();
            Button button = (Button)sender;
            Cell cell = (Cell)button.Tag;
            cell.IsFlagged = !cell.IsFlagged;
        }


        void ButtonClick(object sender, RoutedEventArgs e)
        {
            GetRickRolled();
            Button button = (Button)sender;
            Cell cell = (Cell)button.Tag;
            if (!cell.IsFlagged)
            {
                if (!cell.IsMine)
                {
                    if (!cell.IsRevealed)
                    {
                        RevealAdjcentCells(cell.X, cell.Y);
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
            

        }

        public void RevealAdjcentCells(int row, int column)
        {
            if (row < 0 || row >= width || column < 0 || column >= height)
            {
                return;
            }
            else
            {
                Cell cell = map[row][column];
                Trace.WriteLine("Checking " + row + column);
                Trace.WriteLine(""+cell.IsRevealed + cell.IsMine);
                if (!cell.IsRevealed && !cell.IsMine)
                {
                    cell.IsRevealed = true;
                    if (cell.NeighbouringMines == 0)
                    {
                        RevealAdjcentCells(row - 1, column - 1);
                        RevealAdjcentCells(row - 1, column);
                        RevealAdjcentCells(row - 1, column + 1);
                        RevealAdjcentCells(row, column - 1);
                        RevealAdjcentCells(row, column + 1);
                        RevealAdjcentCells(row + 1, column - 1);
                        RevealAdjcentCells(row + 1, column);
                        RevealAdjcentCells(row + 1, column + 1);
                    }


                    

                }
            }
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
            GetRickRolled();
        }

        public Tuple<int, int, int> gameSetting(string difficulty)
        {
            if (difficulty == "Easy")
            {
                Application.Current.MainWindow.Height = 400;
                Application.Current.MainWindow.Width = 350;
                return Tuple.Create(9, 9, 10);
            }
            else if (difficulty == "Medium")
            {
                Application.Current.MainWindow.Height = 600;
                Application.Current.MainWindow.Width = 550;
                return Tuple.Create(16, 16, 40);
            }
            else
            {
                Application.Current.MainWindow.Height = 600;
                Application.Current.MainWindow.Width = 1000;
                return Tuple.Create(16, 30, 99);
            }
        }

        public bool RandomChoiceGenerator()
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 30);
            if (randomNumber == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void GetRickRolled()
        {
            if (RandomChoiceGenerator())
            {
                PlaySoundTrack("Sound track 3");
            }
        }


        public void OnSoundTrackClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string newSoundTrack = menuItem.Header.ToString();
                PlaySoundTrack(newSoundTrack);
            }
        }

        public void ImageClick(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("image clicked");

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("/Minesweeper;component/Resources/normal.png", UriKind.Relative);
            bitmap.EndInit();
            Face.Source = bitmap;
            InitializeGame();
        }

        public void PlaySoundTrack(string track)
        {
            if (!string.IsNullOrEmpty(track))
            {
                if (track == "Sound track 1")
                {
                    Trace.WriteLine("Started playing");

                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\soundtrack1.mp3"));
                    mediaPlayer.Play();
                }
                else if (track == "Sound track 2")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\lobby-classic-game.mp3"));
                    mediaPlayer.Play();
                }
                else if (track == "Secret sound track")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\secret-track.MP3"));
                    mediaPlayer.Play();
                }
                else {
                    mediaPlayer.Stop();
                }
                
            }
        }

        public void RevealCell(Cell cell)
        {

            if (!cell.IsMine)
            {
                if (!cell.IsRevealed)
                {
                    cell.IsRevealed = true;
                    if (CheckIfFinished(map))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri("/Minesweeper;component/Resources/win.png", UriKind.Relative);
                        bitmap.EndInit();
                        Face.Source = bitmap;
                        MessageBox.Show("You Won");
                        //InitializeGame();
                    }
                }
            }
            else
            {
                MessageBox.Show("You Lost");
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("/Minesweeper;component/Resources/lose.png", UriKind.Relative);
                bitmap.EndInit();
                Face.Source = bitmap;
                //InitializeGame();
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

        public void RevealedMines(ObservableCollection<List<Cell>> map)
        {
            List<Cell> RevealedMines = new List<Cell>();
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    if (cell.IsRevealed)
                    {
                        RevealedMines.Add(cell);
                    }
                }
            }
        }

        public ObservableCollection<List<Cell>> RandomShuffle(ObservableCollection<List<Cell>> map)
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
                            var result = SwapCells(cell, newMap[randomRow][randomCol]);
                            newMap[currentCellRow][currentCellCol] = result.Item1;
                            newMap[randomRow][randomCol] = result.Item2;
                        }
                    }
                }
            }
            outputMap(newMap);
            return newMap;
        }


        public void UpdateNeighbourMines(ObservableCollection<List<Cell>> map)
        {
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    cell.NeighbouringMines = 0;
                }
            }

            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    if (cell.IsMine)
                    {
                        cell.UpdateAdjacentMinesForNeighbors(cell.X, cell.Y, map);
                    }
                }
            }
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


        public Tuple<Cell, Cell> SwapCells(Cell cell1, Cell cell2)
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
            bool tempIsRevealed = cell1.IsRevealed;
            bool tempIsFlagged = cell1.IsFlagged;

            cell1.IsMine = cell2.IsMine;
            cell1.NeighbouringMines = 0;
            cell1.IsRevealed = cell2.IsRevealed;
            cell1.IsFlagged = cell2.IsFlagged;

            cell2.IsMine = tempIsMine;
            cell2.NeighbouringMines = 0;
            cell2.IsRevealed = tempIsRevealed;
            cell2.IsFlagged = tempIsFlagged;

            return Tuple.Create(cell1, cell2);
        }


    }

    


}

