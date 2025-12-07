using System.Drawing;

public class Ship
{
    private Point[] _cells;
    private bool[] _hits;
    private bool _isHorizontal;

    public Ship(Point[] cells, bool isHorizontal = true)
    {
        _cells = cells;
        _hits = new bool[cells.Length];
        _isHorizontal = isHorizontal;
    }

    public Point[] Cells
    {
        get { return _cells; }
        set { _cells = value; }
    }

    public bool IsHorizontal
    {
        get { return _isHorizontal; }
        set { _isHorizontal = value; }
    }

    public bool Hit(Point p)
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            if (_cells[i] == p)
            {
                _hits[i] = true;
                return true;
            }
        }
        return false;
    }

    public bool IsSunk()
    {
        for (int i = 0; i < _hits.Length; i++)
            if (!_hits[i]) return false;
        return true;
    }
}
