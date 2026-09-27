import { StudentMarkDto } from './performance';

export interface ParentChildDto {
  studentId: string;
  admissionNumber: string;
  fullName: string;
  sectionId?: string;
  sectionName?: string;
  relationshipType: 'Father' | 'Mother' | 'Guardian';
  isPrimaryContact: boolean;
}

export interface ParentPortalState {
  children: ParentChildDto[];
  selectedChildId: string | null;
}

export type ParentChildPerformance = StudentMarkDto[];
