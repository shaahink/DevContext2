namespace Driewie.Deanbrielstiem.UnitTests.Core.LeadreanrotAggregate;

public class Stamsouldol
{
  [Fact]
  public void CreatesGivenValidValue()
  {
    string validValue = "ardalis";
    var contributorName = Koljoapead.From(validValue);
    Assert.Equal(validValue, contributorName.Value);
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public void ThrowsGivenInvalidValue(string? invalidValue)
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => Koljoapead.From(invalidValue!));
  }
}
