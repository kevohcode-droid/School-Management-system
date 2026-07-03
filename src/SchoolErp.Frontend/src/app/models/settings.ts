export interface TenantSetting {
  id: string;
  tenantId: string;
  category: string;
  key: string;
  value: string;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface UpdateSettingsRequest {
  category: string;
  settings: { [key: string]: string };
}
