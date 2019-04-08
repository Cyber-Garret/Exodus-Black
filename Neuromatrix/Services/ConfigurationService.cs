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
            Global.Token = _services.GetRequiredService<Configuration>().Token;
            Global.Guild = _services.GetRequiredService<Configuration>().Guild;
            Global.XurChannel = _services.GetRequiredService<Configuration>().XurChannel;
            Global.Version = _services.GetRequiredService<Configuration>().Version;
        }
    }
}
