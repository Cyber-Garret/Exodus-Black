using System;
using Microsoft.Extensions.DependencyInjection;

using Neuromatrix.Models;

namespace Neuromatrix.Services
{
    public class ConfigurationService
    {
        #region Private fields
        private readonly IServiceProvider _services;
        #endregion

        public ConfigurationService(IServiceProvider services)
        {
            _services = services;
        }

        public void Configure()
        {
            Settings.Token = _services.GetRequiredService<Configuration>().Token;
            Settings.Owner = _services.GetRequiredService<Configuration>().Owner;
            Settings.Guild = _services.GetRequiredService<Configuration>().Guild;
            Settings.XurChannel = _services.GetRequiredService<Configuration>().XurChannel;
            Settings.Version = _services.GetRequiredService<Configuration>().Version;
        }
    }
}
