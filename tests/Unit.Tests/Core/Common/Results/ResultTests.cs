using Core.Common.Results;

namespace Unit.Tests.Core.Common.Results;

public class ResultTests
{
    #region Result<T> tests

    [Fact]
    public void IsSuccess_ReturnsTrue_WhenValueIsNotNull()
    {
        var result = new Result<string>("test");
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("test");
    }

    [Fact]
    public void IsFailure_ReturnsTrue_WhenExceptionIsNotNull()
    {
        var expectedException = new Exception("test exception");
        var result = new Result<string>(expectedException);
        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBe(expectedException);
    }

    [Fact]
    public void IsNull_ReturnsTrue_WhenValueAndExceptionAreNull()
    {
        var result = new Result<string>();
        result.IsNull.ShouldBeTrue();
    }

    [Fact]
    public void Match_CallsCorrectFuncBasedOnResultState()
    {
        var successResult = new Result<string>("test");
        var failureResult = new Result<string>(new Exception());
        var nullResult = new Result<string>();

        successResult.Match(value => value, _ => "failure", () => "null").ShouldBe("test");
        failureResult.Match(value => value, _ => "failure", () => "null").ShouldBe("failure");
        nullResult.Match(value => value, _ => "failure", () => "null").ShouldBe("null");
    }

    [Fact]
    public void Match_ThrowsInvalidOperationException_WhenNullResultAndNoOnNullFuncProvided()
    {
        var nullResult = new Result<string>();
        var exception = Should.Throw<InvalidOperationException>(() => nullResult.Match(value => value, _ => "failure"));
        exception.Message.ShouldBe("Result is null, but no onNull function was provided.");
    }

    [Fact]
    public void ImplicitOperatorFromValue_ReturnsSuccessResult()
    {
        Result<string> result = "test";
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("test");
    }

    [Fact]
    public void ImplicitOperatorFromNull_ReturnsNullResult()
    {
        Result<string> result = default(string);
        result.IsNull.ShouldBeTrue();
    }

    [Fact]
    public void ImplicitOperatorFromException_ReturnsFailureResult()
    {
        var exception = new Exception("test exception");
        Result<string> result = exception;
        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBe(exception);
    }

    #endregion

    #region Result tests

    [Fact]
    public void Success_Should_Set_IsSuccess_To_True()
    {
        var result = Result.Success();
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Failure_Should_Set_IsFailure_To_True_And_Set_Exception()
    {
        var exception = new Exception();
        var result = Result.Failure(exception);

        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBe(exception);
    }

    [Fact]
    public void Implicit_Conversion_From_Exception_Should_Set_IsFailure_To_True_And_Set_Exception()
    {
        var exception = new Exception();
        Result result = exception;

        result.IsFailure.ShouldBeTrue();
        result.Exception.ShouldBe(exception);
    }

    [Fact]
    public void Match_Should_Call_Correct_Func_Based_On_Result_State()
    {
        var successResult = Result.Success();
        var failureResult = Result.Failure(new Exception());

        successResult.Match(() => "success", _ => "failure").ShouldBe("success");
        failureResult.Match(() => "success", _ => "failure").ShouldBe("failure");
    }

    #endregion
}
