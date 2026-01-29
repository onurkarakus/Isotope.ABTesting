using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartAbTest.Abstractions.Builders;
using SmartAbTest.Abstractions.Contexts;
using SmartAbTest.Abstractions.Fallback;
using SmartAbTest.Abstractions.Results;
using SmartAbTest.Abstractions.Storage;
using SmartAbTest.Abstractions.Strategies;
using SmartAbTest.Configuration;
using SmartAbTest.Enums;
using SmartAbTest.Exceptions;
using SmartAbTest.Fallbacks;
using SmartAbTest.Models;
using SmartAbTest.Validators;

namespace SmartAbTest.Builders;

/// <summary>
/// IExperimentBuilder arayüzünün somut uygulaması.
/// </summary>
public sealed class ExperimentBuilder : IExperimentBuilder
{
    // --- BAĞIMLILIKLAR (Dependencies) ---
    private readonly string _experimentId;
    private readonly IStateStore _stateStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExperimentBuilder> _logger;
    private readonly ABTestingOptions _options;

    // --- BUILDER STATE (Konfigürasyon Durumu) ---
    private IReadOnlyList<VariantDefinition>? _variants;
    private IAllocationStrategy? _algorithm;
    private IFallbackPolicy? _fallbackPolicy;
    private bool _requirePersistence;
    private TimeSpan? _ttl;

    public ExperimentBuilder(string experimentId,
        IStateStore stateStore,
        IServiceProvider serviceProvider,
        ILogger<ExperimentBuilder> logger,
        IOptions<ABTestingOptions> options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(experimentId);
        ArgumentNullException.ThrowIfNull(stateStore);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _experimentId = experimentId;
        _stateStore = stateStore;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;

        logger.LogDebug("ExperimentBuilder created for ExperimentId: {ExperimentId}", experimentId);
    }

    public IExperimentBuilder WithVariants(params (string VariantName, int Weight)[] variants)
    {
        ArgumentNullException.ThrowIfNull(variants);

        var validationResult = WeightValidator.Validate(_experimentId, variants);

        _variants = validationResult.ValidVariants;

        if (validationResult.WasNormalized)
        {
            _logger.LogWarning("Variant weights for ExperimentId: {ExperimentId} were normalized. Original sum: {OriginalSum}", _experimentId, validationResult.OriginalSum);
        }

        return this;
    }

    public IExperimentBuilder UseAlgorithm<TStrategy>()
        where TStrategy : class, IAllocationStrategy
    {
        _algorithm = ResolveStrategy<TStrategy>();

        return this;
    }

    public IExperimentBuilder UseAlgorithm<TStrategy>(Action<TStrategy> configure)
        where TStrategy : class, IAllocationStrategy
    {
        ArgumentNullException.ThrowIfNull(configure);

        var strategy = ResolveStrategy<TStrategy>();
        configure(strategy);
        _algorithm = strategy;

        return this;
    }

    public IExperimentBuilder UseAlgorithm(
        Func<AllocationContext, CancellationToken, ValueTask<AllocationResult>> algorithm)
    {
        ArgumentNullException.ThrowIfNull(algorithm);

        _algorithm = new DelegateAllocationStrategy(algorithm);

        return this;
    }

    public IExperimentBuilder UseAlgorithm(
        Func<AllocationContext, CancellationToken, ValueTask<AllocationResult>> algorithm,
        Action<CustomAlgorithmOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(algorithm);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new CustomAlgorithmOptions();
        configure(options);

        _algorithm = new DelegateAllocationStrategy(algorithm, options);

        return this;
    }

    public IExperimentBuilder OnFailure(IFallbackPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        _fallbackPolicy = policy;

        return this;
    }

    public IExperimentBuilder OnFailure(
        Func<FallbackContext, CancellationToken, ValueTask<AllocationResult>> fallbackHandler)
    {
        ArgumentNullException.ThrowIfNull(fallbackHandler);

        _fallbackPolicy = new DelegateFallbackPolicy(fallbackHandler);
        return this;
    }

    public IExperimentBuilder RequirePersistence()
    {
        _requirePersistence = true;

        return this;
    }

    public IExperimentBuilder WithTtl(TimeSpan? ttl)
    {
        _ttl = ttl;

        return this;
    }

    public async Task<AllocationResult> GetVariantAsync(string subjectKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(subjectKey);

        ValidateConfiguration();

        return await ExecuteAllocationAsync(subjectKey, cancellationToken);
    }

    public async Task<AllocationResult> GetVariantAsync(
       Func<CancellationToken, Task<string>> keyResolver,
       CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(keyResolver);

        ValidateConfiguration();

        string subjectKey;

        try
        {
            subjectKey = await keyResolver(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Experiment '{ExperimentId}': Key resolver execution failed - {ErrorMessage}",
                _experimentId, ex.Message);
            
            throw ExceptionFactory.KeyResolutionFailed(_experimentId, ex);
        }

        if (string.IsNullOrWhiteSpace(subjectKey))
        {           
            _logger.LogError("Experiment '{ExperimentId}': Key resolver returned null or empty subject key.",
                _experimentId);
         
            throw ExceptionFactory.KeyResolverReturnedNull(_experimentId); ;
        }

        return await ExecuteAllocationAsync(subjectKey, cancellationToken);
    }

    private async Task<AllocationResult> ExecuteAllocationAsync(string subjectKey, CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(subjectKey);
        var effectiveTtl = _ttl ?? _options.DefaultTtl;

        var shouldPersist = _algorithm!.RequiresState || _requirePersistence;

        if (shouldPersist)
        {
            try
            {
                var cachedVariant = await _stateStore.GetAsync(cacheKey, cancellationToken);

                if (cachedVariant != null)
                {
                    _logger.LogDebug("Cache hit for ExperimentId: {ExperimentId}, SubjectKey: {SubjectKey}, Variant: {Variant}",
                        _experimentId, subjectKey, cachedVariant);

                    return AllocationResult.From(
                        variant: _variants!.First(v => string.Equals(v.Name, cachedVariant, StringComparison.OrdinalIgnoreCase)),
                        experimentId: _experimentId,
                        subjectKey: subjectKey,
                        allocationSource: AllocationSource.Cached);
                }

                _logger.LogDebug("Cache miss for ExperimentId: {ExperimentId}, SubjectKey: {SubjectKey}",
                    _experimentId, subjectKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read from state store for ExperimentId: {ExperimentId}, SubjectKey: {SubjectKey}. Proceeding without cache.",
                    _experimentId, subjectKey);

                return await ExecuteFallbackAsync(subjectKey, ex, cancellationToken);
            }
        }

        string variantName;

        try
        {
            var context = new AllocationContext(
                ExperimentId: _experimentId,
                SubjectKey: subjectKey,
                Variants: _variants!,
                ServiceProvider: _serviceProvider);

            var allocationResult = await _algorithm.AllocateAsync(context, cancellationToken);

            variantName = allocationResult.Variant!.Name;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Experiment '{ExperimentId}': Algorithm execution failed - {ErrorMessage}",
                _experimentId, ex.Message);

            throw ExceptionFactory.AlgorithmExecutionFailed(_experimentId, ex);
        }

        if (shouldPersist)
        {
            try
            {
                await _stateStore.SetAsync(cacheKey, variantName, effectiveTtl, cancellationToken);

                _logger.LogDebug("Persisted allocation for ExperimentId: {ExperimentId}, SubjectKey: {SubjectKey}, Variant: {Variant}, TTL: {Ttl}",
                    _experimentId, subjectKey, variantName, effectiveTtl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Experiment '{ExperimentId}': Failed to write to state store - {ErrorMessage}",
                    _experimentId, ex.Message);
            }
        }

        _logger.LogInformation("Experiment '{ExperimentId}': Assigned variant '{VariantName}' to subject '{SubjectKey}'",
            _experimentId, variantName, subjectKey);

        return AllocationResult.From(
            variant: _variants!.First(v => string.Equals(v.Name, variantName, StringComparison.OrdinalIgnoreCase)),
            experimentId: _experimentId,
            subjectKey: subjectKey,
            allocationSource: AllocationSource.Calculated);

    }

    private string BuildCacheKey(string subjectKey)
    {
        var serviceName = _options.ServiceName ?? "default";
        return $"{serviceName}:exp:{_experimentId}:assign:{subjectKey}";
    }

    private async Task<AllocationResult> ExecuteFallbackAsync(string subjectKey, Exception originalException, CancellationToken cancellationToken)
    {
        var fallbackContext = new FallbackContext(_experimentId, subjectKey, _variants!, originalException, _serviceProvider);

        string variantName;

        try
        {
            var fallbackAllocationResult = await _fallbackPolicy!.ExecuteAsync(fallbackContext, cancellationToken);

            variantName = fallbackAllocationResult.Variant!.Name;
        }
        catch (ABTestingException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallback execution failed for ExperimentId: {ExperimentId}, SubjectKey: {SubjectKey}",
                _experimentId, subjectKey);

            throw ExceptionFactory.FallbackExecutionFailed(_experimentId, ex);
        }

        _logger.LogInformation("Experiment '{ExperimentId}': Fallback triggered for subject '{SubjectKey}'. Returning '{VariantName}'. Reason: {Reason}",
            _experimentId, subjectKey, variantName, originalException.Message);

        return AllocationResult.From(
            variant: _variants!.First(v => string.Equals(v.Name, variantName, StringComparison.OrdinalIgnoreCase)),
            experimentId: _experimentId,
            subjectKey: subjectKey,
            allocationSource: AllocationSource.Fallback);
    }

    private void ValidateConfiguration()
    {
        if (_variants == null || _variants.Count == 0)
        {
            throw ExceptionFactory.MissingConfiguration(_experimentId, "Variants");
        }

        if (_algorithm == null)
        {
            throw ExceptionFactory.MissingConfiguration(_experimentId, "Algorithm");
        }

        if (_fallbackPolicy == null)
        {
            throw ExceptionFactory.MissingConfiguration(_experimentId, "FallbackPolicy");
        }

        if (_fallbackPolicy is DefaultVariantFallbackPolicy defaultPolicy)
        {
            var variantExists = _variants.Any(v =>
                string.Equals(v.Name, defaultPolicy.VariantName, StringComparison.OrdinalIgnoreCase));

            if (!variantExists)
            {
                throw ExceptionFactory.FallbackVariantNotFound(
                    _experimentId,
                    defaultPolicy.VariantName,
                    _variants.Select(v => v.Name));
            }

            AlgorithmFallbackValidator.Validate(
            _experimentId,
            _algorithm,
            _fallbackPolicy,
            _logger);
        }
    }

    private TStrategy ResolveStrategy<TStrategy>()
        where TStrategy : class, IAllocationStrategy
    {
        var strategy = _serviceProvider.GetService(typeof(TStrategy)) as TStrategy;

        if (strategy != null)
        {
            return strategy;
        }

        try
        {
            return Activator.CreateInstance<TStrategy>();
        }
        catch (Exception ex)
        {
            throw ExceptionFactory.StrategyResolutionFailed(_experimentId, typeof(TStrategy), ex);
        }
    }




}
