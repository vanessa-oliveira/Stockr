export interface Customer {
  id: string;
  name: string;
  email: string;
  phone: string;
  cpf?: string;
  cnpj?: string;
  tenantId?: string;
}