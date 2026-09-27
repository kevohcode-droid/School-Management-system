export interface StudentMarkDto {
  id: string;
  studentId: string;
  studentName: string;
  admissionNumber: string;
  subject: string;
  examTerm: string;
  scoreObtained: number;
  maxScore: number;
  percentage: number;
  dateRecorded: string;
  reviewStatus: string;
  reviewComment?: string | null;
}

export interface SaveStudentMarkItemDto {
  id?: string;
  studentId: string;
  subject: string;
  examTerm: string;
  scoreObtained: number;
  maxScore: number;
  dateRecorded?: string;
}

export interface BulkSaveResponse {
  message: string;
  count: number;
}

export interface StudentMarkEntryRow {
  markId?: string;
  studentId: string;
  admissionNumber: string;
  studentName: string;
  scoreObtained: number | null;
  maxScore: number;
  percentage: number;
  gradeBadge: string;
  statusBadge: string;
  remarks: string;
  reviewStatus?: string;
  reviewComment?: string;
  validationError?: string;
  isModified?: boolean;
}
