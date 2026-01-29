using SmartAbTest.Abstractions.Fallback;

namespace SmartAbTest.Fallbacks;

public static class Fallback
{
    public static IFallbackPolicy Throw => ThrowFallbackPolicy.Instance;

    public static IFallbackPolicy ToStateless => StatelessFallbackPolicy.Instance;

    public static IFallbackPolicy ToVariant(string variantName)
    {
        return new DefaultVariantFallbackPolicy(variantName);
    }
}
