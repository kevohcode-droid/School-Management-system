export interface SchoolClass {
  id: string;
  tenantId: string;
  name: string;
  code: string;
  capacity: number;
  classTeacherId?: string;
  classTeacherName?: string;
  room?: string;
  subjects: string[];
  createdAtUtc: string;
}

export interface CreateClassRequest {
  name: string;
  code: string;
  capacity: number;
  classTeacherId?: string | null;
  room?: string | null;
  subjects: string[];
}
