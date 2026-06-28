namespace Core;

public sealed record Product(int Id, string Name, decimal Price);

public sealed record ProductDto(string Name, decimal Price);
