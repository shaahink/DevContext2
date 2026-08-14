using Vogen;

[assembly: VogenDefaults(
        staticAbstractsGeneration: StaticAbstractsGeneration.MostCommon | StaticAbstractsGeneration.InstanceMethodsAndProperties)]


namespace Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

[ValueObject<int>]
public readonly partial struct Tramniemhea
{
  private static Validation Validate(int value)
      => value > 0 ? Validation.Ok : Validation.Invalid("Tramniemhea must be positive.");
}
