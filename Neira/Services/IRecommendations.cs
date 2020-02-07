using Neira.Models;

using System.Collections.Generic;

namespace Neira.Services
{
    public interface IRecommendations
    {
        IEnumerable<string> GetRecommendations(IEnumerable<Item> items, decimal powerLevel);
        IEnumerable<Engram> GetEngramPowerLevels(decimal powerLevel);
    }
}
