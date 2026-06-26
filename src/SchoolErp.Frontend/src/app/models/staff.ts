export interface Staff {
  id: string;
  tenantId: string;
  employeeId: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  gender: number | string;
  nationalId?: string;
  designation: string;
  department: string;
  dateOfJoining: string;
  employmentStatus: string;
  qualifications?: string;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface CreateStaffRequest {
  employeeId: string;
  firstName: string;
  lastName: string;
  email: string | null;
  phone: string | null;
  gender: number;
  nationalId?: string | null;
  designation: string;
  department: string;
  dateOfJoining: string | null;
  employmentStatus: string;
  qualifications?: string | null;
}
