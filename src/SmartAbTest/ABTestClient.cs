using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAbTest.Abstractions;
using SmartAbTest.Abstractions.Builders;
using SmartAbTest.Abstractions.Storage;
using SmartAbTest.Builders;
using SmartAbTest.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartAbTest;

public sealed class ABTestClient : IABTestClient
{
    private readonly IStateStore _stateStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExperimentBuilder> _logger;
    private readonly IOptions<ABTestingOptions> _options;

    public ABTestClient(IStateStore stateStore, IServiceProvider serviceProvider, ILogger<ExperimentBuilder> logger, IOptions<ABTestingOptions> options)
    {
        ArgumentNullException.ThrowIfNull(stateStore);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _stateStore = stateStore;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options;
    }

    public IExperimentBuilder Experiment(string experimentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(experimentId);

        return new ExperimentBuilder(experimentId, _stateStore, _serviceProvider, _logger, _options);
    }
}
