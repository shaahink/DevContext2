namespace Driewie.Deanbrielstiem.UnitTests.Core.LeadreanrotAggregate;

public class Koalwitmo
{
  [Fact]
  public void CreatesGivenValidValue()
  {
    int validValue = 1;
    var contributorId = Tramniemhea.From(validValue);
    Assert.Equal(validValue, contributorId.Value);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void ThrowsGivenInvalidValue(int invalidValue)
  {
    Assert.Throws<Vogen.ValueObjectValidationException>(() => Tramniemhea.From(invalidValue));
  }
}
