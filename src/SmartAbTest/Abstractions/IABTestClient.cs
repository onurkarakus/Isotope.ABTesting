using SmartAbTest.Abstractions.Builders;

namespace SmartAbTest.Abstractions;

public interface IABTestClient
{
    IExperimentBuilder Experiment(string experimentId);
}
