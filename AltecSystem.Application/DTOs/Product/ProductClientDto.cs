namespace AltecSystem.Application.DTOs.Product;

public  class ProductClientDto
{
    public Guid Id { get; set; }
    public string Categoria { get; set; }
    public string Codigo { get; set; }
    public int Stock { get; set; }
    public decimal Pvp { get; set; }
    public string Descripcion { get; set; }
    public string Foto { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}