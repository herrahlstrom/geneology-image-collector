﻿using Microsoft.Extensions.Configuration;

namespace GeneologyImageCollector;

internal class AppSettings
{
    private readonly IConfiguration _config;

    public AppSettings(IConfiguration config)
    {
        _config = config;
    }

    public string RootPath => _config[nameof(RootPath)];
}