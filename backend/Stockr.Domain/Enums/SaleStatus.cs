namespace Stockr.Domain.Enums;

public enum SaleStatus
{
    // Estados iniciais
    Draft = 0,           // Rascunho - pode ser editado livremente
    Pending = 1,         // Aguardando confirmação
    
    // Estados de processamento
    Confirmed = 2,       // Confirmada - estoque reservado
    PendingPayment = 3,  // Aguardando pagamento
    Paid = 4,           // Pagamento confirmado
    
    // Estados finais
    Completed = 9,       // Finalizada com sucesso
    Cancelled = 10,      // Cancelada
}