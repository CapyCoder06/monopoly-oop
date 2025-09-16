using Monopoly.Application.DTO;
using Monopoly.Domain.Core;

namespace Monopoly.Application.Mappers;

public static class DomainToVm
{
    public static PlayerVM ToVm(this Player p) => new(p.Id, p.Name, p.Position, p.Cash);
}
