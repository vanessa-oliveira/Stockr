namespace Stockr.Application.Models;

public class CustomerViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string? CPF { get; set; }
    public string? CNPJ { get; set; }
}