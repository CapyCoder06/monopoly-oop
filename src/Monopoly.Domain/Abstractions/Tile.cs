namespace Monopoly.Domain.Abstractions;

public abstract class Tile
{
    public int Index { get; }
    public string Name { get; }

    protected Tile(int index, string name)
    {
        Index = index; Name = name;
    }

    // OnLand sẽ hiện thực ở các mốc sau
    public virtual void OnLand(object context) { }
}
