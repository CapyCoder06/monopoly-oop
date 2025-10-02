using Monopoly.Domain.Abstractions;
public class Deck<TCard> where TCard : Card
{
    private readonly Queue<TCard> _q;
    public Deck(IEnumerable<TCard> cards) => _q = new Queue<TCard>(cards);
    public TCard Draw()
    {
        var c = _q.Dequeue();
        _q.Enqueue(c);
        return c;
    }
}