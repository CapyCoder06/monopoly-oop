using Monopoly.Domain.Core;
using Monopoly.Domain.Events;


namespace Monopoly.Domain.Abstractions;

public abstract class Card
{
    public string Title { get; }
    public string Description { get; }
    protected Card(string title, string description)
    {
        Title = title;
        Description = description;
    }
    public abstract void Resolve(GameContext ctx, IDomainEventBus bus, Player player);
}
