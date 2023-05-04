﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Minesweeper
{
    internal class GameManager
    {
        ObservableCollection<List<Cell>> map = new ObservableCollection<List<Cell>>();
        // Initialize
        private int width = 10;
        private int height = 10;
        private string difficulty = "Easy";
        private int mines = 50;
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
        private string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //Amongus
        private MovingObject amongus;
        private DispatcherTimer amongusTimer;
        private MediaPlayer mediaPlayer = new MediaPlayer();
        MainWindow game = (MainWindow)Application.Current.MainWindow;

        private void InitializeGame()
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            string path = Path.Combine(baseDirectory, "Resources", "normal.png");
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            game.Face.Source = bitmap;
            game.Face.Width = 50;
            game.Face.Height = 50;
            timeSpent = 0;
            timer?.Stop();
            firstClick = true;
            if (shuffleMode)
            {
                ShuffleModeOn();
            }

            if (amongusMode)
            {
                game.AmongusCanvas.Children.Clear();
                SpawningMovingObject();
            }

            Tuple<int, int, int> gameSettings = gameSetting(difficulty);
            map = new ObservableCollection<List<Cell>>();

            width = gameSettings.Item1;
            height = gameSettings.Item2;
            mines = gameSettings.Item3;

            for (int i = 0; i < width; i++)
            {
                List<Cell> row = new List<Cell>();
                for (int j = 0; j < height; j++)
                {
                    row.Add(new Cell(i, j, difficulty));
                }
                map.Add(row);
            }
            PlaceMines();
            //map = PlaceMines(mines, map);
            //map = RandomShuffle(map);
            //UpdateNeighbourMines(map);
            var viewModel = new MainViewModel { Map = map };
            game.DataContext = viewModel;
            // Timer functionality



        }

        private void RecordTimeSpent()
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


        private void TimerTick(object sender, EventArgs e)
        {
            timeSpent++;
            game.TimeSpentLabel.Content = $"{timeSpent}";

        }



        private void ShuffleModeOn()
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

        private void ShuffleModeOff()
        {
            elapsedTime = 10;
            shuffleTimer?.Stop();
        }

        private void ToggleShuffle(object sender, EventArgs e)
        {
            shuffleMode = !shuffleMode;
            if (shuffleMode)
            {
                game.ShuffleMode.Header = "Shuffle Mode: On";
                ShuffleModeOn();
            }
            else
            {
                game.ShuffleMode.Header = "Shuffle Mode: Off";
                ShuffleModeOff();
            }
        }


        private void ShuffleTimerTick(object sender, EventArgs e)
        {
            elapsedTime--;
            game.TimerLabel.Content = $"Time left until shuffle : {elapsedTime}s";
            if (elapsedTime == 0)
            {
                map = RandomShuffle(map);
                UpdateNeighbourMines(map);
                var viewModel = (MainViewModel)game.DataContext;
                viewModel.Map = map;
                elapsedTime = 10;
                game.TimerLabel.Content = $"Time left until shuffle : {elapsedTime}s";
            }

        }

        private void DisplayVideo(object sender, EventArgs e)
        {
            if (isPlaying)
            {
                Application.Current.MainWindow.Height -= 500;
                Application.Current.MainWindow.Width -= 500;
                game.mePlayer.Source = null;
                isPlaying = false;
            }
            else
            {
                game.mePlayer.Source = new Uri(@"D:\cs\Minesweeper\Minesweeper\Resources\subway.mp4");
                Application.Current.MainWindow.Height += 500;
                Application.Current.MainWindow.Width += 500;
                isPlaying = true;
            }

        }


        private void Flag(object sender, MouseButtonEventArgs e)
        {
            GetRickRolled();
            Button button = (Button)sender;
            Cell cell = (Cell)button.Tag;
            cell.IsFlagged = !cell.IsFlagged;
        }

        private void SaveBestTimeToFile()
        {
            string path = @"D:\cs\Minesweeper\Minesweeper\Resources\bestTime.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(bestTime);
            }
        }

        private void CellButtonClick(object sender, RoutedEventArgs e)
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

        private void OnDifficultyMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string newDifficulty = menuItem.Header.ToString();
                ChangeDifficulty(newDifficulty);
            }
        }

        private void NewGame(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        private void ChangeDifficulty(string newDifficulty)
        {
            // Update the difficulty value
            map = new ObservableCollection<List<Cell>>();
            difficulty = newDifficulty;

            // Reinitialize the game with the new difficulty
            InitializeGame();
            GetRickRolled();
        }

        private Tuple<int, int, int> gameSetting(string difficulty)
        {
            int width = 0;
            int height = 0;
            int mineCount = 0;

            if (difficulty == "Easy")
            {
                Application.Current.MainWindow.Height = 450;
                Application.Current.MainWindow.Width = 380;
                width = 9;
                height = 9;
                mineCount = 1;

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
                width = 16;
                height = 30;
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

        private bool RandomChoiceGenerator(int chance)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, chance);
            return randomNumber == 0;
        }

        private void ClickResult(Cell cell)
        {
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
                            string path = Path.Combine(baseDirectory, "Resources", "win.png");
                            bitmap.UriSource = new Uri(path);
                            bitmap.EndInit();
                            game.Face.Source = bitmap;
                            game.Face.Width = 50;
                            game.Face.Height = 50;
                            // Stop timer

                            ShuffleModeOff();
                            timer.Stop();


                            // Set IsGaming to false
                            var viewModel = game.DataContext as MainViewModel;
                            viewModel.IsGaming = false;
                            CustomMessageBox customMessageBox = new CustomMessageBox
                            {
                                Owner = game
                            };
                            bitmap = new BitmapImage();
                            bitmap.BeginInit();

                            path = Path.Combine(baseDirectory, "Resources", "poggers.png");
                            bitmap.UriSource = new Uri(path);
                            bitmap.EndInit();

                            customMessageBox.DisplayImage.Source = bitmap;
                            customMessageBox.OkButton.Content = "You won, very cool";
                            customMessageBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                            customMessageBox.ShowDialog();
                            // Show message box
                            MessageBox.Show("You unlocked a secret sound track, you can now play it in the sound tracks");




                            // Add new menu item
                            MenuItem newMenuItem2 = new MenuItem
                            {
                                Header = "Secret sound track"
                            };
                            newMenuItem2.Click += new RoutedEventHandler(OnSoundTrackClick);
                            game.SoundTracks.Items.Add(newMenuItem2);

                            // Check for best time
                            if (timeSpent < bestTime)
                            {
                                bestTime = timeSpent;
                                SaveBestTimeToFile();
                                game.BestTimeLabel.Content = $"Best Time: {bestTime}";
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
                    string path = Path.Combine(baseDirectory, "Resources", "lose.png");
                    bitmap.UriSource = new Uri(path);
                    bitmap.EndInit();
                    game.Face.Source = bitmap;
                    ShuffleModeOff();

                    game.Face.Width = 50;
                    game.Face.Height = 50;

                    timer?.Stop();

                    var viewModel = game.DataContext as MainViewModel;
                    viewModel.IsGaming = false;
                    CustomMessageBox customMessageBox = new CustomMessageBox
                    {
                        Owner = game,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    customMessageBox.ShowDialog();


                    //InitializeGame();
                }
            }
        }

        private void GetRickRolled()
        {
            if (RandomChoiceGenerator(30))
            {
                PlaySoundTrack("Secret sound track");
            }
        }


        private void OnSoundTrackClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                string newSoundTrack = menuItem.Header.ToString();
                PlaySoundTrack(newSoundTrack);
            }
        }

        private void ImageClick(object sender, RoutedEventArgs e)
        {

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            string path = Path.Combine(baseDirectory, "Resources", "normal.png");
            bitmap.UriSource = new Uri(path);
            bitmap.EndInit();
            game.Face.Source = bitmap;
            game.Face.Width = 50;
            game.Face.Height = 50;
            InitializeGame();
        }

        private void PlaySoundTrack(string track)
        {
            Dictionary<string, string> soundTracks = new Dictionary<string, string>{
                { "Sound track 1", "lobby-classic-game.mp3" },
                { "Sound track 2", "soundtrack1.mp3" },
                { "Sound track 3", "miiplaza.mp3" },
                { "Sound track 4", "jojo_ending_edited.mp3" },
                { "Sound track 5", "canzoni preferite.mp3" },
                { "Secret sound track", "secret-track.MP3" },
                { "among us sound" , "among-us-role-reveal.mp3"},
                {"among us eject", "Among us Eject Sound Effect.mp3" }
            };

            if (soundTracks.ContainsKey(track))
            {
                string soundTrack = soundTracks[track];
                string path = Path.Combine(baseDirectory, "Resources", soundTrack);
                mediaPlayer.Open(new Uri(path));
                mediaPlayer.Play();
            }
            else
            {
                mediaPlayer.Stop();
            }


        }


        private int LoadBestTimeFromFile()
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


        private bool CheckIfFinished(ObservableCollection<List<Cell>> map)
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

        private void PlaceMines()
        {
            Random random = new Random();
            int minePlaced = 0;

            while (minePlaced < mines)
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
        }
        /*
        private ObservableCollection<List<Cell>> PlaceMines(int minesLeft, ObservableCollection<List<Cell>> map)
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
        */
        private void outputMap(ObservableCollection<List<Cell>> map)
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



        private ObservableCollection<List<Cell>> RandomShuffle(ObservableCollection<List<Cell>> map)
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


        private void UpdateNeighbourMines(ObservableCollection<List<Cell>> map)
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


        private ObservableCollection<List<Cell>> DeepCopy(ObservableCollection<List<Cell>> map)
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


        private Tuple<Cell, Cell> SwapCells(Cell cell1, Cell cell2)
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
        private void RevealAdjcentCells(int row, int column)
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


        private void RevealAllMines(ObservableCollection<List<Cell>> map)
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
        private void SpawningMovingObject()
        {
            Trace.WriteLine("I am called");
            amongus = new MovingObject
            {
                IsHitTestVisible = true
            };

            game.AmongusCanvas.Children.Add(amongus);
            amongusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            amongusTimer.Tick += AmongusTimerTick;
            amongusTimer.Start();
        }

        private void AmongusTimerTick(object sender, EventArgs e)
        {
            if (amongus.IsVisible)
            {
                UpdateAmongusPosition();
                AmongusRandomClick();
            }
            else
            {
                if (RandomChoiceGenerator(20) && amongusMode)
                {
                    SpawningMovingObject();
                }
            }
        }

        private void UpdateAmongusPosition()
        {
            Random random = new Random();
            if (amongus != null)
            {
                int newX = random.Next(0, (int)game.GameArea.ActualWidth - (int)amongus.Width);
                int newY = random.Next(0, (int)game.GameArea.ActualHeight - (int)amongus.Height);

                Canvas.SetLeft(amongus, newX);
                Canvas.SetTop(amongus, newY);

            }
        }

        private void ToggleAmongus(object sender, RoutedEventArgs e)
        {
            amongusMode = !amongusMode;
            if (!amongusMode)
            {
                amongusTimer.Stop();
                game.AmongusCanvas.Children.Clear();
                MessageBox.Show("The imposter has been ejected");
                PlaySoundTrack("among us eject");
                return;
            }
            MessageBox.Show("A imposter has infiltrated your game, it will click on random cells until you click it to make it go away. It has a chance to respawn");
            PlaySoundTrack("among us sound");
            SpawningMovingObject();
        }

        private void AmongusRandomClick()
        {
            Random random = new Random();
            if (RandomChoiceGenerator(20))
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

