namespace Stockr.Domain.Enums;

public enum UserRole
{
    // Níveis de sistema
    SystemAdmin = 0,        // Administrador do sistema (acesso a tudo)
    TenantAdmin = 1,        // Administrador do tenant (acesso total ao tenant)
    
    // Perfis operacionais
    Manager = 10,           // Gerente (acesso a vendas, estoque, relatórios)
    Seller = 20,            // Vendedor (vendas, consulta estoque, clientes)
    StockController = 30,   // Estoquista (gestão de estoque, produtos, fornecedores)
    Cashier = 40,           // Operador de caixa (vendas básicas, consultas)
    Viewer = 50,            // Visualizador (apenas consultas e relatórios)
    
    // Mantido para compatibilidade
    User = 99               // Usuário genérico (acesso mínimo)
}