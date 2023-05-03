using Minesweeper;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

public class MainViewModel : INotifyPropertyChanged
{
    private ObservableCollection<List<Cell>> _map;
    public ObservableCollection<List<Cell>> Map
    {
        get { return _map; }
        set
        {
            _map = value;
            OnPropertyChanged("Map");
        }
    }

    private bool _IsGaming = true;
    public bool IsGaming
    {
        get { return _IsGaming; }
        set
        {
            _IsGaming = value;
            OnPropertyChanged(nameof(IsGaming));
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
