export interface Tenant {
  id: string;
  name: string;
  code: string;
  contactEmail?: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface CreateTenantRequest {
  name: string;
  code: string;
  contactEmail?: string;
}
