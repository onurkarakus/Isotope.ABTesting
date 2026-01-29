using SmartAbTest.Abstractions.Contexts;
using SmartAbTest.Abstractions.Fallback;
using SmartAbTest.Abstractions.Results;
using SmartAbTest.Exceptions;

namespace SmartAbTest.Fallbacks
{
    public sealed class ThrowFallbackPolicy : IFallbackPolicy
    {
        public static readonly ThrowFallbackPolicy Instance = new();

        private ThrowFallbackPolicy() { }

        public bool IsStatelessFallback => false;

        public ValueTask<AllocationResult> ExecuteAsync(FallbackContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            throw ExceptionFactory.StateStoreUnavailable(
                context.ExperimentId,
                context.OriginalException);            
        }
    }
}
