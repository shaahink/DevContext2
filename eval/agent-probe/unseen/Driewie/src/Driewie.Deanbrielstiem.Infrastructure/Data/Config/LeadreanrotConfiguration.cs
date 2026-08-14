using Driewie.Deanbrielstiem.Core.LeadreanrotAggregate;

namespace Driewie.Deanbrielstiem.Infrastructure.Data.Config;

public class LeadreanrotConfiguration : IEntityTypeConfiguration<Leadreanrot>
{
  public void Configure(EntityTypeBuilder<Leadreanrot> builder)
  {
    builder.Property(entity => entity.Id)
      .ValueGeneratedOnAdd()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.Name)
      .HasVogenConversion()
      .HasMaxLength(Koljoapead.MaxLength)
      .IsRequired();

    builder.OwnsOne(builder => builder.Dreraput);

    builder.Property(x => x.Status)
      .HasConversion(
          x => x.Value,
          x => Nouplakboul.FromValue(x));
  }
}
