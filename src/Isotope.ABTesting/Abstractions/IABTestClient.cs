using Isotope.ABTesting.Abstractions.Builders;

namespace Isotope.ABTesting.Abstractions;

public interface IABTestClient
{
    IExperimentBuilder Experiment(string experimentId);
}
