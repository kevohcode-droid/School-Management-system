// ── Student Portal Models ─────────────────────────────────────────────────────

export interface StudentProfile {
  studentId: string;
  admissionNumber: string;
  fullName: string;
  firstName: string;
  lastName: string;
  email: string | null;
  dateOfBirth: string | null;
  gender: string;
  sectionId: string | null;
  sectionName: string | null;
  enrollmentDate: string;
}

export interface StudentPortalMark {
  id: string;
  subject: string;
  examTerm: string;
  scoreObtained: number;
  maxScore: number;
  percentage: number;
  dateRecorded: string;
}

export interface TermReport {
  term: string;
  marks: StudentPortalMark[];
  average: number;
  totalMarks: number;
  totalMax: number;
}

export interface StudentPerformanceResponse {
  marks: StudentPortalMark[];
  grouped: TermReport[];
}

export interface StudentAttendanceRecord {
  date: string;
  status: string; // 'Present' | 'Absent' | 'Late' | 'Excused'
  remarks: string | null;
  classId: string;
}

export interface AttendanceSummary {
  total: number;
  present: number;
  absent: number;
  late: number;
  excused: number;
  percentage: number;
}

export interface StudentAttendanceResponse {
  records: StudentAttendanceRecord[];
  summary: AttendanceSummary;
}

export interface StudentInvoiceItem {
  description: string;
  categoryName?: string;
  amount: number;
}

export interface StudentPayment {
  transactionReference: string;
  amount: number;
  paymentDate: string;
  mode: string;
  status?: string;
}

export interface StudentInvoice {
  id: string;
  invoiceNumber: string;
  description: string | null;
  amount: number;
  amountPaid: number;
  discountAmount?: number;
  balance: number;
  dueDateUtc: string;
  issuedOnUtc: string;
  status: string; // 'Pending' | 'PartiallyPaid' | 'Paid' | 'Overdue' | 'Cancelled'
  academicYear: string | null;
  term: string | null;
  items: StudentInvoiceItem[];
  payments: StudentPayment[];
}

export interface StudentFeesResponse {
  invoices: StudentInvoice[];
  totalOutstanding: number;
  totalPaid: number;
  overdueCount: number;
}

export interface StudentAnnouncement {
  title: string;
  createdAt: string;
}

export interface StudentDashboard {
  attendancePercentage: number;
  latestTermAverage: number;
  latestTerm: string;
  feeBalance: number;
  overdueInvoices: number;
  announcements: StudentAnnouncement[];
}
