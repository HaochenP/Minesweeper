using System.ComponentModel;

public class CellViewModel : INotifyPropertyChanged
{
    private bool _isMine;
    public bool IsMine
    {
        get { return _isMine; }
        set
        {
            _isMine = value;
            OnPropertyChanged(nameof(IsMine));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
