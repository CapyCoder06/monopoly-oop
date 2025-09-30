namespace Monopoly.Domain.Abstractions
{
public abstract class Card
{
    public string Title { get; }
    protected Card(string title) => Title = title;

    public virtual void Resolve(object context) { }
}
}
