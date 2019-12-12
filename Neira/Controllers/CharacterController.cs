using Destiny2;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Neira.Models;
using Neira.Services;
using Neira.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neira.Controllers
{
    [Authorize]
    public class CharacterController : Controller
    {
        private readonly IDestiny2 _destiny;
        private readonly IMaxPowerService _maxPower;
        private readonly IRecommendations _recommendations;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IOptions<BungieSettings> _bungie;
        private readonly ILogger _logger;

        public CharacterController(IServiceProvider service)
        {
            _destiny = service.GetRequiredService<IDestiny2>();
            _maxPower = service.GetRequiredService<IMaxPowerService>();
            _recommendations = service.GetRequiredService<IRecommendations>();
            _contextAccessor = service.GetRequiredService<IHttpContextAccessor>();
            _bungie = service.GetRequiredService<IOptions<BungieSettings>>();
            _logger = service.GetRequiredService<ILogger<CharacterController>>();
        }

        [HttpGet("{type}/{id}/{characterId}")]
        public async Task<IActionResult> Details(int type, long id, long characterId)
        {
            var membershipType = (BungieMembershipType)type;
            _logger.LogInformation($"{membershipType}/{id}/{characterId}");

            var accessToken = await _contextAccessor.HttpContext.GetTokenAsync("access_token");

            var maxGear = await _maxPower.ComputeMaxPowerAsync(membershipType, id, characterId);
            if (maxGear == null)
            {
                _logger.LogWarning("Couldn't find max gear. Redirecting to Account Index");
                var url = Url.RouteUrl("AccountIndex");
                return Redirect(url);
            }
            var characterTask = _destiny.GetCharacterInfo(accessToken, membershipType, id, characterId,
                DestinyComponentType.Characters);
            var profileTask = _destiny.GetProfile(accessToken, membershipType, id,
                DestinyComponentType.ProfileProgression);

            await Task.WhenAll(characterTask, profileTask);

            var character = characterTask.Result;
            var profile = profileTask.Result;
            var lowestItems = FindLowestItems(maxGear.Values).ToList();

            var maxPower = _maxPower.ComputePower(maxGear.Values);
            var model = new CharacterViewModel()
            {
                Type = membershipType,
                AccountId = id,
                Items = maxGear.Values,
                LowestItems = lowestItems,
                BasePower = maxPower,
                BonusPower = profile.ProfileProgression.Data.SeasonalArtifact.PowerBonus,
                EmblemPath = _bungie.Value.BaseUrl + character.Character.Data.EmblemPath,
                EmblemBackgroundPath = _bungie.Value.BaseUrl + character.Character.Data.EmblemBackgroundPath,
                Recommendations = _recommendations.GetRecommendations(maxGear.Values, maxPower),
                Engrams = _recommendations.GetEngramPowerLevels(maxPower)
            };

            return View(model);
        }

        private IEnumerable<Item> FindLowestItems(IEnumerable<Item> items)
        {
            var minPower = items.Min(item => item.PowerLevel);
            var lowestItems = items.OrderBy(item => item.PowerLevel)
                                   .TakeWhile(item => item.PowerLevel == minPower);
            if (lowestItems.Count() == items.Count())
            {
                // All items are max power.
                return Enumerable.Empty<Item>();
            }

            return lowestItems;
        }
    }
}