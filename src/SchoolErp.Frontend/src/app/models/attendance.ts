export enum AttendanceStatus {
  Present = 1,
  Absent = 2,
  Late = 3,
  Excused = 4
}

export interface AttendanceRecord {
  studentId: string;
  studentName: string;
  status: AttendanceStatus;
  remarks?: string;
}

export interface MarkAttendanceDto {
  studentId: string;
  date: string;
  status: AttendanceStatus;
  remarks?: string;
}

export interface BulkMarkAttendanceDto {
  classId: string;
  date: string;
  records: MarkAttendanceDto[];
}

export interface StudentAttendanceDto {
  studentId: string;
  studentName: string;
  sectionId?: string;
  sectionName?: string;
}
