import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StudentPortalService } from '../../../services/student-portal.service';
import { StudentFeesResponse, StudentInvoice, StudentPayment } from '../../../models/student-portal';

export const STUDENT_FEES_SAMPLE_STATE = {
  admissionNumber: 'ADM-2026-0142',
  term: 'Term 3, 2026',
  dueDate: '2026-10-15',
  totalBilled: 45000,
  totalPaid: 30000,
  outstandingBalance: 15000,
  feeBreakdown: [
    { description: 'Tuition', category: 'Tuition', amount: 30000 },
    { description: 'Computer Lab', category: 'Facilities', amount: 4000 },
    { description: 'Activity Fee', category: 'Activities', amount: 2500 },
    { description: 'Exam Fee', category: 'Examinations', amount: 5000 },
    { description: 'Library Access', category: 'Learning Resources', amount: 3500 }
  ],
  payments: [
    { date: '2026-08-20', reference: 'QGH7M2K9AB', method: 'M-Pesa', amount: 20000, status: 'Completed' },
    { date: '2026-09-12', reference: 'BNK-2026-0912-04', method: 'Bank Transfer', amount: 10000, status: 'Verified' }
  ]
} as const;

interface StudentPaymentRow {
  invoice: StudentInvoice;
  payment: StudentPayment;
}

@Component({
  selector: 'app-student-fees',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './student-fees.html',
  styleUrls: ['./student-fees.css']
})
export class StudentFeesComponent implements OnInit {
  feesData: StudentFeesResponse | null = null;
  selectedTermKey = '';
  receiptToPrint: StudentPaymentRow | null = null;
  loading = true;
  error: string | null = null;

  constructor(private studentPortalService: StudentPortalService) {}

  ngOnInit(): void {
    this.loadFees();
  }

  loadFees(): void {
    this.loading = true;
    this.error = null;
    this.studentPortalService.getFees().subscribe({
      next: (data) => {
        this.feesData = data;
        this.selectedTermKey = this.termOptions[0]?.key ?? '';
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load financial statements. Please try again later.';
        this.loading = false;
      }
    });
  }

  get termOptions(): { key: string; label: string; latestIssue: number }[] {
    const options = new Map<string, { key: string; label: string; latestIssue: number }>();
    for (const invoice of this.feesData?.invoices ?? []) {
      const key = `${invoice.academicYear ?? ''}|${invoice.term ?? ''}`;
      const existing = options.get(key);
      const latestIssue = new Date(invoice.issuedOnUtc).getTime();
      if (!existing || latestIssue > existing.latestIssue) {
        const label = [invoice.term, invoice.academicYear].filter(Boolean).join(' · ') || 'Unspecified term';
        options.set(key, { key, label, latestIssue });
      }
    }
    return [...options.values()].sort((a, b) => b.latestIssue - a.latestIssue);
  }

  get currentTermInvoices(): StudentInvoice[] {
    if (!this.feesData) return [];
    if (!this.selectedTermKey) return this.feesData.invoices;
    return this.feesData.invoices.filter(invoice =>
      `${invoice.academicYear ?? ''}|${invoice.term ?? ''}` === this.selectedTermKey
    );
  }

  get selectedTermLabel(): string {
    return this.termOptions.find(term => term.key === this.selectedTermKey)?.label ?? 'All statement periods';
  }

  get totalBilled(): number {
    return this.currentTermInvoices.reduce(
      (total, invoice) => total + invoice.amount - (invoice.discountAmount ?? 0), 0
    );
  }

  get totalPaid(): number {
    return this.currentTermInvoices.reduce((total, invoice) =>
      total + invoice.payments
        .filter(payment => ['completed', 'verified'].includes(payment.status?.toLowerCase() ?? ''))
        .reduce((paid, payment) => paid + payment.amount, 0), 0
    );
  }

  get outstandingBalance(): number {
    return Math.max(this.totalBilled - this.totalPaid, 0);
  }

  get nextDueDate(): string | null {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const dueDates = this.currentTermInvoices
      .filter(invoice => invoice.balance > 0 && new Date(invoice.dueDateUtc).getTime() >= today.getTime())
      .map(invoice => invoice.dueDateUtc)
      .sort((a, b) => new Date(a).getTime() - new Date(b).getTime());
    return dueDates[0] ?? null;
  }

  get billedItems(): { description: string; category: string; amount: number }[] {
    return this.currentTermInvoices.flatMap(invoice =>
      invoice.items.map(item => ({
        description: item.description,
        category: item.categoryName || 'School fees',
        amount: item.amount
      }))
    );
  }

  get paymentHistory(): StudentPaymentRow[] {
    return this.currentTermInvoices.flatMap(invoice =>
      invoice.payments.map(payment => ({ invoice, payment }))
    ).sort((a, b) => new Date(b.payment.paymentDate).getTime() - new Date(a.payment.paymentDate).getTime());
  }

  getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Paid':          return 'badge badge-success';
      case 'Pending':       return 'badge badge-warning';
      case 'Overdue':       return 'badge badge-danger';
      case 'PartiallyPaid': return 'badge badge-info';
      case 'Cancelled':     return 'badge badge-neutral';
      default:              return 'badge badge-neutral';
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-KE', {
      style: 'currency',
      currency: 'KES',
      minimumFractionDigits: 2
    }).format(value);
  }

  getPaymentStatusClass(status: string | undefined): string {
    return ['completed', 'verified'].includes(status?.toLowerCase() ?? '')
      ? 'status-badge status-badge--verified'
      : 'status-badge status-badge--pending';
  }

  getPaymentStatusLabel(status: string | undefined): string {
    if (!status) return 'Recorded';
    return status.toLowerCase() === 'completed' ? 'Completed' : status;
  }

  downloadStatementPdf(): void {
    window.print();
  }

  printStatement(): void {
    window.print();
  }

  downloadReceipt(row: StudentPaymentRow): void {
    this.receiptToPrint = row;
    window.addEventListener('afterprint', this.clearReceipt, { once: true });
    window.setTimeout(() => window.print(), 0);
  }

  private readonly clearReceipt = (): void => {
    this.receiptToPrint = null;
  };
}
