namespace AltecSystem.Application.DTOs.Product;

public record ProductImportResponse
{
    public int TotalCount { get; init; }

    public List<ProductItem> Items { get; init; } = [];

    public record ProductItem
    {
        public Guid Id { get; init; }
        public string Categoria { get; init; } = string.Empty;
        public string Codigo { get; init; } = string.Empty;
        public int Stock { get; init; }
        public decimal PrecioMayorista { get; init; }
        public string Descripcion { get; init; } = string.Empty;
        public string Foto { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public bool IsImport { get; init; }
    }
}