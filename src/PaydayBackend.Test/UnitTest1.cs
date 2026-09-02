using FluentAssertions;

namespace PaydayBackend.Test;

public class UnitTest1
{
    [Fact]
    public void TestOfTests_Dummy_ShouldBeGood()
    {
        string result = "this should be a basic test";
        result
            .Should()
            .NotBeEmpty(because: "It shouldn't be!")
            .And.BeLowerCased(because: "why not?");
    }
}
