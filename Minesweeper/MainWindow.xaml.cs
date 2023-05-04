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
        // Initialize
        int width = 10;
        int height = 10;
        string difficulty = "Easy";
        int mines = 50;
        // ShuffleTimer
        private DispatcherTimer shuffleTimer;
        private int elapsedTime;
        private bool shuffleMode = false;
        // Score timer
        private DispatcherTimer timer;
        private int timeSpent;
        private bool firstClick = true;
        private int bestTime = 999;
        private bool isPlaying = false;
        private bool amongusMode = false;

        //Amongus
        private MovingObject amongus;
        public DispatcherTimer amongusTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            var viewModel = new MainViewModel { Map = map };
            DataContext = viewModel;
            bestTime = LoadBestTimeFromFile();
            //SpawningMovingObject();
        }
        private MediaPlayer mediaPlayer = new MediaPlayer();


        public void InitializeGame()
        {
            timeSpent = 0;
            if (timer != null)
            {
                timer.Stop();
            }
            firstClick = true;
            if (shuffleMode)
            {
                ShuffleModeOn();
            }
            Tuple<int, int, int> gameSettings = gameSetting(difficulty);
            map = new ObservableCollection<List<Cell>>();

            width = gameSettings.Item1;
            height = gameSettings.Item2;
            mines = gameSettings.Item3;

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
            //map = RandomShuffle(map);
            //UpdateNeighbourMines(map);
            var viewModel = new MainViewModel { Map = map };
            DataContext = viewModel;
            // Timer functionality



        }

        public void RecordTimeSpent()
        {
            timeSpent = 0;
            if (timer != null)
            {
                timer.Stop();
            }
            else
            {
                timer = new DispatcherTimer();
                timer.Tick += TimerTick;


            }
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
        }


        public void TimerTick(object sender, EventArgs e)
        {
            timeSpent++;
            TimeSpentLabel.Content = $"{timeSpent}";

        }



        public void ShuffleModeOn()
        {
            MessageBox.Show("Shuffle mode is on, any remaining unclicked cells will be randomly shuffled every 10 seconds");
            elapsedTime = 10;
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

        public void ShuffleModeOff()
        {
            elapsedTime = 10;
            if (shuffleTimer != null)
            {
                shuffleTimer.Stop();
            }
        }

        public void ToggleShuffle(object sender, EventArgs e)
        {
            shuffleMode = !shuffleMode;
            if (shuffleMode)
            {
                ShuffleMode.Header = "Shuffle Mode: On";
                ShuffleModeOn();
            }
            else
            {
                ShuffleMode.Header = "Shuffle Mode: Off";
                ShuffleModeOff();
            }
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
                elapsedTime = 10;
                TimerLabel.Content = $"Time left until shuffle : {elapsedTime}s";
            }

        }

        public void DisplayVideo(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                Application.Current.MainWindow.Height -= 500;
                Application.Current.MainWindow.Width -= 500;
                mePlayer.Source = null;
                isPlaying = false;
            }
            else
            {
                mePlayer.Source = new Uri(@"C:\Users\haochenjiang\source\repos\Minesweeper\Minesweeper\Resources\subway.mp4");
                Application.Current.MainWindow.Height += 500;
                Application.Current.MainWindow.Width += 500;
                isPlaying = true;
            }

        }


        void Flag(object sender, MouseButtonEventArgs e)
        {
            GetRickRolled();
            Button button = (Button)sender;
            Cell cell = (Cell)button.Tag;
            cell.IsFlagged = !cell.IsFlagged;
        }

        public void SaveBestTimeToFile()
        {
            string path = @"D:\cs\Minesweeper\Minesweeper\Resources\bestTime.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(bestTime);
            }
        }

        void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (firstClick)
            {
                RecordTimeSpent();
                firstClick = false;
            }
            GetRickRolled();
            Button button = (Button)sender;
            Cell cell = (Cell)button.Tag;
            ClickResult(cell);


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
            MessageBox.Show(CellGrid.Width.ToString());
            MessageBox.Show(CellGrid.Height.ToString());

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
            int width = 0;
            int height = 0;
            int mineCount = 0;

            if (difficulty == "Easy")
            {
                Application.Current.MainWindow.Height = 450;
                Application.Current.MainWindow.Width = 350;
                width = 9;
                height = 9;
                mineCount = 10;

            }
            else if (difficulty == "Medium")
            {
                Application.Current.MainWindow.Height = 650;
                Application.Current.MainWindow.Width = 600;
                width = 16;
                height = 16;
                mineCount = 40;

            }
            else
            {
                width = 30;
                height = 16;
                mineCount = 99;

                Application.Current.MainWindow.Height = 650;
                Application.Current.MainWindow.Width = 1000;

            }

            if (isPlaying)
            {
                Application.Current.MainWindow.Height += 500;
                Application.Current.MainWindow.Width += 500;
            }
            return Tuple.Create(width, height, mineCount);
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

        public void ClickResult(Cell cell)
        {
            Trace.WriteLine("i am here");
            if (!cell.IsFlagged)
            {
                //Trace.WriteLine("among us is clicking " + cell.X + cell.Y);
                if (!cell.IsMine)
                {
                    if (!cell.IsRevealed)
                    {
                        RevealAdjcentCells(cell.X, cell.Y);
                        if (CheckIfFinished(map))
                        {
                            // Change face to win face
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri("/Minesweeper;component/Resources/win.png", UriKind.Relative);
                            bitmap.EndInit();
                            Face.Source = bitmap;
                            Face.Width = 300;
                            Face.Height = 300;
                            // Stop timer

                            ShuffleModeOff();
                            timer.Stop();


                            // Set IsGaming to false
                            var viewModel = DataContext as MainViewModel;
                            viewModel.IsGaming = false;

                            // Show message box
                            MessageBox.Show("You Won");
                            MessageBox.Show("You unlocked a secret sound track");

                            // Add new menu item
                            MenuItem newMenuItem2 = new MenuItem();
                            newMenuItem2.Header = "Secret sound track";
                            newMenuItem2.Click += new RoutedEventHandler(OnSoundTrackClick);
                            SoundTracks.Items.Add(newMenuItem2);

                            // Check for best time
                            if (timeSpent < bestTime)
                            {
                                bestTime = timeSpent;
                                SaveBestTimeToFile();
                                BestTimeLabel.Content = $"Best Time: {bestTime}";
                                MessageBox.Show("Pog, You got a new best time");
                            }


                            //InitializeGame();
                        }
                    }
                }
                else
                {
                    cell.IsRevealed = true;
                    RevealAllMines(map);

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("/Minesweeper;component/Resources/lose.png", UriKind.Relative);
                    bitmap.EndInit();
                    Face.Source = bitmap;
                    ShuffleModeOff();

                    Face.Width = 300;
                    Face.Height = 300;

                    if (timer != null)
                    {
                        timer.Stop();
                    }

                    var viewModel = DataContext as MainViewModel;
                    viewModel.IsGaming = false;
                    CustomMessageBox customMessageBox = new CustomMessageBox();
                    customMessageBox.Owner = this;
                    customMessageBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    customMessageBox.ShowDialog();


                    //InitializeGame();
                }
            }
        }

        public void GetRickRolled()
        {
            if (RandomChoiceGenerator())
            {
                PlaySoundTrack("Secret sound track");
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

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri("/Minesweeper;component/Resources/normal.png", UriKind.Relative);
            bitmap.EndInit();
            Face.Source = bitmap;
            Face.Width = 30;
            Face.Height = 30;
            InitializeGame();
        }

        public void PlaySoundTrack(string track)
        {
            if (!string.IsNullOrEmpty(track))
            {
                if (track == "Sound track 1")
                {

                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\lobby-classic-game.mp3"));
                    mediaPlayer.Play();
                }
                else if (track == "Sound track 2")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\soundtrack1.mp3"));
                    mediaPlayer.Play();
                }
                else if (track == "Sound track 3")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\miiplaza.mp3"));
                    mediaPlayer.Play();
                }
                else if (track == "Sound track 4")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\jojo_ending_edited.mp3"));
                    mediaPlayer.Play();
                }
                else if (track == "Sound track 5")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\canzoni preferite.mp3"));
                    mediaPlayer.Play();
                }

                else if (track == "Secret sound track")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\secret-track.MP3"));
                    mediaPlayer.Play();
                }
                else if (track =="among us sound")
                {
                    mediaPlayer.Open(new Uri("C:\\Users\\haochenjiang\\source\\repos\\Minesweeper\\Minesweeper\\Resources\\among-us-role-reveal.mp3"));
                    mediaPlayer.Play();
                }
                else
                {
                    mediaPlayer.Stop();
                }

            }
        }


        public int LoadBestTimeFromFile()
        {
            string path = "D:\\cs\\Minesweeper\\Minesweeper\\Resources\\bestTime.txt";
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                return Int32.Parse(lines[0]);
            }
            else
            {
                return 999;
            }
        }


        public bool CheckIfFinished(ObservableCollection<List<Cell>> map)
        {
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    if (!cell.IsRevealed && !cell.IsMine)
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
            //outputMap(map);
            return map;
        }

        public void outputMap(ObservableCollection<List<Cell>> map)
        {
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    Trace.WriteLine(cell.X + " " + cell.Y + " " + cell.IsMine + " number of adjcent mines: " + cell.NeighbouringMines);
                    Trace.WriteLine(cell.IsRevealed);
                    Console.WriteLine(cell.X + " " + cell.Y + " " + cell.IsMine + " number of adjcent mines: " + cell.NeighbouringMines);
                }
                Console.WriteLine();
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
            //outputMap(newMap);
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
            /*
            int tempX = cell1.X;
            int tempY = cell1.Y;
            cell1.X = cell2.X;
            cell1.Y = cell2.Y;
            cell2.X = tempX;
            cell2.Y = tempY; */

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
        public void RevealAdjcentCells(int row, int column)
        {
            if (row < 0 || row >= width || column < 0 || column >= height)
            {
                return;
            }
            else
            {
                Cell cell = map[row][column];

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

   
        public void RevealAllMines(ObservableCollection<List<Cell>> map)
        {
            foreach (List<Cell> row in map)
            {
                foreach (Cell cell in row)
                {
                    if (cell.IsMine)
                    {
                        cell.IsRevealed = true;
                    }
                }
            }
        }


        //Amongus Logic
        public void SpawningMovingObject()
        {
            amongus = new MovingObject();
            amongus.IsHitTestVisible = true;

            AmongusCanvas.Children.Add(amongus);
            amongusTimer = new DispatcherTimer();
            amongusTimer.Interval = TimeSpan.FromSeconds(1);
            amongusTimer.Tick += AmongusTimerTick;
            amongusTimer.Start();
        }

        public void AmongusTimerTick(object sender, EventArgs e)
        {
            if (amongus.IsVisible) 
            { 
            UpdateAmongusPosition();
            AmongusRandomClick();
            }
            else
            {
                Random random = new Random();
                if(random.Next(0,10) == 1)
                {
                    SpawningMovingObject();
                }
            }
        }

        public void UpdateAmongusPosition()
        {
            Random random = new Random();
            if (amongus != null)
            {
                int newX = random.Next(0, ((int)GameArea.ActualWidth - (int)amongus.Width));
                int newY = random.Next(0, ((int)GameArea.ActualHeight - (int)amongus.Height));

                Canvas.SetLeft(amongus, newX);
                Canvas.SetTop(amongus, newY);

            }
        }

        public void ToggleAmongus(object sender, RoutedEventArgs e)
        {
            amongusMode = !amongusMode;
            if (!amongusMode)
            {
                
                
                amongusTimer.Stop();
                AmongusCanvas.Children.Clear();
                MessageBox.Show("The imposter has been ejected");

                return;
            }
            MessageBox.Show("A imposter has infiltrated your game, it will click on random cells until you click it to make it go away. It has a chance to respawn");
            PlaySoundTrack("among us sound");
            SpawningMovingObject();
        }

        public void AmongusRandomClick()
        {
            Random random = new Random();
            if (random.Next(0, 20) == 0)
            {
                int row = random.Next(0, map.Count);
                int column = random.Next(0, map[0].Count);
                var cell = map[row][column];
                if (!map[row][column].IsRevealed)
                {
                    ClickResult(cell);
                }
            }
        }
    }
}

