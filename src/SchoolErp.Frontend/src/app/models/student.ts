export interface Student {
  id: string;
  tenantId: string;
  admissionNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  gender: number | string;
  dateOfBirth: string;
  enrollmentDate: string;
  sectionId?: string;
  sectionName?: string;
  section?: Section;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface Section {
  id: string;
  tenantId: string;
  name: string;
  gradeLevel: string;
  capacity: number;
}

export interface CreateStudentRequest {
  admissionNumber: string;
  admissionDate: string | null;
  firstName: string;
  middleName?: string | null;
  lastName: string;
  email: string | null;
  gender: number;
  dateOfBirth: string | null;
  nationality?: string | null;
  academicYear?: string | null;
  sectionId?: string | null;
  // Guardian / Parent details
  guardianName?: string | null;
  guardianRelationship?: string | null;
  guardianPhone?: string | null;
  guardianEmail?: string | null;
}

export interface UpdateStudentRequest {
  firstName: string;
  middleName?: string | null;
  lastName: string;
  email: string | null;
  gender: number;
  dateOfBirth: string | null;
  nationality?: string | null;
  sectionId?: string | null;
  guardianName?: string | null;
  guardianRelationship?: string | null;
  guardianPhone?: string | null;
  guardianEmail?: string | null;
}
