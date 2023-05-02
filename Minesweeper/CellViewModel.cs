using System.ComponentModel;

public class CellViewModel : INotifyPropertyChanged
{
    private bool _IsRevealed;
    public bool IsRevealed
    {
        get { return _IsRevealed; }
        set
        {
            _IsRevealed = value;
            OnPropertyChanged(nameof(IsRevealed));

        }
    }

    private int _X;
    public int X
    {
        get { return _X; }
        set
        {
            _X = value;
            OnPropertyChanged(nameof(X));
        }
    }

    private int _Y;
    public int Y
    {
        get { return _Y; }
        set
        {
            _Y = value;
            OnPropertyChanged(nameof(Y));
        }
    }

    private int _NeighbouringMines;
    public int NeighbouringMines
    {
        get { return _NeighbouringMines; }
        set
        {
            _NeighbouringMines = value;
            OnPropertyChanged(nameof(_NeighbouringMines));
        }
    }

    private bool _IsMine;
    public bool IsMine
    {
        get { return _IsMine; }
        set
        {
            _IsMine = value;
            OnPropertyChanged(nameof(IsMine));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
