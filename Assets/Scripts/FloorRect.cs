public struct FloorRect
{
    public int x1, y1, x2, y2;

    public FloorRect(int x1, int y1, int x2, int y2)
    {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
    }

    public int Width => x2 - x1 + 1;
    public int Height => y2 - y1 + 1;


    public override string ToString()
    {
        return $"FloorRect({x1}, {y1}, {x2}, {y2})";
    }
}