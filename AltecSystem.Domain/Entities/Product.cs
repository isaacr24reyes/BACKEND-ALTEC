using System;

namespace AltecSystem.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Categoria { get; set; }
    public string Codigo { get; set; }
    public int Stock { get; set; }
    public decimal Pvp { get; set; }
    public decimal PrecioMayorista { get; set; }
    public decimal PrecioImportacion { get; set; }
    public string Descripcion { get; set; }
    public byte[] Foto { get; set; }  // Aquí se almacenará la foto como datos binarios
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string UpdatedBy { get; set; }
}