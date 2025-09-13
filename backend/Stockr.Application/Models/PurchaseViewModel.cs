namespace Stockr.Application.Models;

public class PurchaseViewModel
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? Notes { get; set; }
    public string InvoiceNumber { get; set; }
    public ICollection<PurchaseItemViewModel> PurchaseItems { get; set; } = new List<PurchaseItemViewModel>();
}

public class PurchaseItemViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}