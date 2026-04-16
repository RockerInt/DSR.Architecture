using Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Observability;
using Xunit;

namespace Dsr.Architecture.Infrastructure.Persistence.EntityFramework.Tests;

public class CollectionComparerTests
{
    [Fact]
    public void AreEqual_BothEmpty_ReturnsTrue()
    {
        var a = Array.Empty<int>();
        var b = Array.Empty<int>();

        Assert.True(CollectionComparer.AreEqual<int>(a, b));
    }

    [Fact]
    public void AreEqual_SameInts_ReturnsTrue()
    {
        int[] a = [1, 2, 3];
        int[] b = [1, 2, 3];

        Assert.True(CollectionComparer.AreEqual<int>(a, b));
    }

    [Fact]
    public void AreEqual_DifferentInts_ReturnsFalse()
    {
        int[] a = [1, 2, 3];
        int[] b = [1, 2, 4];

        Assert.False(CollectionComparer.AreEqual<int>(a, b));
    }

    [Fact]
    public void AreEqual_DifferentLengths_ReturnsFalse()
    {
        int[] a = [1, 2];
        int[] b = [1, 2, 3];

        Assert.False(CollectionComparer.AreEqual<int>(a, b));
    }

    [Fact]
    public void AreEqual_Doubles_WithinTolerance_ReturnsTrue()
    {
        double[] a = [1.00001];
        double[] b = [1.00002];

        Assert.True(CollectionComparer.AreEqual<double>(a, b, tolerance: 0.001));
    }

    [Fact]
    public void AreEqual_Doubles_OutsideTolerance_ReturnsFalse()
    {
        double[] a = [1.0];
        double[] b = [2.0];

        Assert.False(CollectionComparer.AreEqual<double>(a, b, tolerance: 0.0001));
    }

    [Fact]
    public void AreEqual_Floats_WithinTolerance_ReturnsTrue()
    {
        float[] a = [1.0001f];
        float[] b = [1.0002f];

        Assert.True(CollectionComparer.AreEqual<float>(a, b, tolerance: 0.01));
    }

    [Fact]
    public void AreEqual_Strings_ReturnsTrue()
    {
        string[] a = ["hello", "world"];
        string[] b = ["hello", "world"];

        Assert.True(CollectionComparer.AreEqual<string>(a, b));
    }

    [Fact]
    public void AreEqual_Strings_ReturnsFalse()
    {
        string[] a = ["hello"];
        string[] b = ["world"];

        Assert.False(CollectionComparer.AreEqual<string>(a, b));
    }

    [Fact]
    public void AreEqual_NullElements_BothNull_ReturnsTrue()
    {
        string?[] a = [null];
        string?[] b = [null];

        Assert.True(CollectionComparer.AreEqual<string?>(a, b));
    }

    [Fact]
    public void AreEqual_NullVsNonNull_ReturnsFalse()
    {
        string?[] a = [null];
        string?[] b = ["value"];

        Assert.False(CollectionComparer.AreEqual<string?>(a, b));
    }

    [Fact]
    public void AreEqual_Decimals_ReturnsTrue()
    {
        decimal[] a = [1.23m];
        decimal[] b = [1.23m];

        Assert.True(CollectionComparer.AreEqual<decimal>(a, b));
    }

    [Fact]
    public void AreEqual_Decimals_ReturnsFalse()
    {
        decimal[] a = [1.23m];
        decimal[] b = [1.24m];

        Assert.False(CollectionComparer.AreEqual<decimal>(a, b));
    }

    [Fact]
    public void AreEqual_SameReference_ReturnsTrue()
    {
        var obj = new object();
        object[] a = [obj];
        object[] b = [obj];

        Assert.True(CollectionComparer.AreEqual<object>(a, b));
    }
}
