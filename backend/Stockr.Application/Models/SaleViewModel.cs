using Stockr.Domain.Enums;

namespace Stockr.Application.Models;

public class SaleViewModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerViewModel? Customer { get; set; }
    public Guid SalespersonId { get; set; }
    public string SalespersonName { get; set; }
    public SaleStatus SaleStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SaleDate { get; set; }
    public ICollection<SaleItemViewModel> SaleItems { get; set; } = new List<SaleItemViewModel>();
}

public class SaleItemViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}