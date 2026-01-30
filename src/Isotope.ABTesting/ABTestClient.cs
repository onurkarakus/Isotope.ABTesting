using Isotope.ABTesting.Abstractions;
using Isotope.ABTesting.Abstractions.Builders;
using Isotope.ABTesting.Abstractions.Storage;
using Isotope.ABTesting.Builders;
using Isotope.ABTesting.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isotope.ABTesting;

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
