import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FeesService } from '../../services/fees.service';
import { StudentService } from '../../services/student.service';
import { AuthService } from '../../services/auth.service';
import {
  FeeInvoiceDto,
  PaymentTransactionDto,
  FeeCategoryDto,
  FeeTemplateDto,
  DiscountDto,
  PaymentMode,
  FeeInvoiceStatus,
  CreatePaymentRequest,
  CreateInvoiceRequest,
  CreateFeeCategoryRequest
} from '../../models/fees';
import { Student } from '../../models/student';

export interface ConfigurablePaymentMethod {
  id: string;
  name: string;
  icon: string;
  description: string;
  enabled: boolean;
  mode: PaymentMode;
  details: { label: string; value: string }[];
}

@Component({
  selector: 'app-fees',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './fees.html',
  styleUrls: ['./fees.css']
})
export class FeesComponent implements OnInit {
  activeTab: 'payments' | 'invoices' | 'categories' | 'templates' | 'discounts' | 'methods' = 'payments';

  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  // Data Collections
  invoices: FeeInvoiceDto[] = [];
  payments: PaymentTransactionDto[] = [];
  categories: FeeCategoryDto[] = [];
  templates: FeeTemplateDto[] = [];
  discounts: DiscountDto[] = [];
  students: Student[] = [];

  // Filter States
  searchQuery: string = '';
  selectedPaymentMode: string = 'ALL';
  selectedInvoiceStatus: string = 'ALL';

  // Pagination
  invoicePageSize = 15;
  invoicePage = 1;
  get totalInvoicePages(): number { return Math.ceil(this.filteredInvoices.length / this.invoicePageSize) || 1; }
  get pagedInvoices(): FeeInvoiceDto[] {
    const s = (this.invoicePage - 1) * this.invoicePageSize;
    return this.filteredInvoices.slice(s, s + this.invoicePageSize);
  }
  get invoicePageNumbers(): number[] {
    const total = this.totalInvoicePages; const cur = this.invoicePage; const pages: number[] = [];
    if (total <= 7) { for (let i = 1; i <= total; i++) pages.push(i); }
    else {
      pages.push(1); if (cur > 3) pages.push(-1);
      for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) pages.push(i);
      if (cur < total - 2) pages.push(-1); pages.push(total);
    }
    return pages;
  }
  goToInvoicePage(p: number): void { this.invoicePage = p; }

  paymentPageSize = 15;
  paymentPage = 1;
  get totalPaymentPages(): number { return Math.ceil(this.filteredPayments.length / this.paymentPageSize) || 1; }
  get pagedPayments(): PaymentTransactionDto[] {
    const s = (this.paymentPage - 1) * this.paymentPageSize;
    return this.filteredPayments.slice(s, s + this.paymentPageSize);
  }
  get paymentPageNumbers(): number[] {
    const total = this.totalPaymentPages; const cur = this.paymentPage; const pages: number[] = [];
    if (total <= 7) { for (let i = 1; i <= total; i++) pages.push(i); }
    else {
      pages.push(1); if (cur > 3) pages.push(-1);
      for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) pages.push(i);
      if (cur < total - 2) pages.push(-1); pages.push(total);
    }
    return pages;
  }
  goToPaymentPage(p: number): void { this.paymentPage = p; }

  // Modal Controls
  showRecordPaymentModal: boolean = false;
  showCreateInvoiceModal: boolean = false;
  showCreateCategoryModal: boolean = false;
  showCreateTemplateModal: boolean = false;
  showReceiptModal: boolean = false;
  showConfigureMethodModal: boolean = false;

  // Selected Items
  selectedReceipt: PaymentTransactionDto | null = null;
  associatedInvoice: FeeInvoiceDto | null = null;
  selectedMethodToConfig: ConfigurablePaymentMethod | null = null;

  // Record Payment Form
  paymentForm: CreatePaymentRequest = {
    feeInvoiceId: '',
    transactionReference: '',
    paymentMode: PaymentMode.MobileMoney,
    amount: 0,
    paymentDateUtc: new Date().toISOString().substring(0, 10),
    notes: ''
  };
  selectedInvoiceBalance: number = 0;

  // Create Invoice Form
  invoiceForm: CreateInvoiceRequest = {
    studentId: '',
    academicYear: '2026',
    term: 'Term 1',
    description: 'Term Tuition Fee Invoice',
    dueDateUtc: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().substring(0, 10),
    items: [
      { feeCategoryId: '', description: 'Tuition Fee', amount: 500 }
    ]
  };

  // Create Category Form
  categoryForm: CreateFeeCategoryRequest = {
    name: '',
    code: '',
    description: ''
  };

  // Payment Methods Configuration List
  paymentMethods: ConfigurablePaymentMethod[] = [
    {
      id: 'mpesa',
      name: 'Mobile Money (M-Pesa / Airtel Money)',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">smartphone</span>',
      description: 'Instant mobile payment via M-Pesa Paybill, Till Number or STK Push.',
      enabled: true,
      mode: PaymentMode.MobileMoney,
      details: [
        { label: 'Paybill Number', value: '522522' },
        { label: 'Account Name', value: 'School ERP Tuition' },
        { label: 'STK Push Gateway', value: 'Active (Safaricom Daraja API)' }
      ]
    },
    {
      id: 'bank',
      name: 'Bank Transfer & Direct Deposit',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">account_balance</span>',
      description: 'Direct wire transfer or cash deposit into school bank accounts.',
      enabled: true,
      mode: PaymentMode.BankTransfer,
      details: [
        { label: 'Bank Name', value: 'Equity Bank' },
        { label: 'Account Number', value: '011029384756' },
        { label: 'Swift / Branch Code', value: 'EQBLKENA' }
      ]
    },
    {
      id: 'cash',
      name: 'Cash Payment Desk',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">attach_money</span>',
      description: 'Physical cash payments handled at school finance counters.',
      enabled: true,
      mode: PaymentMode.Cash,
      details: [
        { label: 'Counter Desk', value: 'Finance Office Counter 1' },
        { label: 'Max Cash Limit', value: 'KSh 500,000 per transaction' }
      ]
    },
    {
      id: 'cheque',
      name: 'Bank Cheque',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">note</span>',
      description: 'Physical banker cheques with clearing period verification.',
      enabled: true,
      mode: PaymentMode.Cheque,
      details: [
        { label: 'Payee Name', value: 'School Management Board' },
        { label: 'Clearing Period', value: '3 Business Days' }
      ]
    },
    {
      id: 'stripe',
      name: 'Credit / Debit Card (Stripe / Visa / MasterCard)',
      icon: '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">credit_card</span>',
      description: 'Online card payment gateway integration via Stripe API.',
      enabled: true,
      mode: PaymentMode.Card,
      details: [
        { label: 'Payment Gateway', value: 'Stripe API v3' },
        { label: 'Accepted Cards', value: 'Visa, MasterCard, Amex' },
        { label: 'Auto-Receipts', value: 'Enabled' }
      ]
    }
  ];

  // Enums for Template Access
  PaymentModeEnum = PaymentMode;
  FeeInvoiceStatusEnum = FeeInvoiceStatus;

  paymentModeOptions = [
    { label: 'Mobile Money (M-Pesa / Airtel)', value: PaymentMode.MobileMoney },
    { label: 'Bank Transfer / Deposit', value: PaymentMode.BankTransfer },
    { label: 'Cash Payment', value: PaymentMode.Cash },
    { label: 'Cheque', value: PaymentMode.Cheque },
    { label: 'Credit / Debit Card', value: PaymentMode.Card }
  ];

  constructor(
    public feesService: FeesService,
    public studentService: StudentService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.detectRouteTab();
    this.loadData();
    this.loadStudents();
  }

  detectRouteTab(): void {
    const url = this.router.url;
    if (url.includes('payment-methods')) {
      this.activeTab = 'methods';
    } else if (url.includes('fee-structures')) {
      this.activeTab = 'templates';
    } else if (url.includes('payments') || url.includes('receipts')) {
      this.activeTab = 'payments';
    } else if (url.includes('fees')) {
      this.activeTab = 'invoices';
    }
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.feesService.getPayments().subscribe({
      next: (data) => {
        this.payments = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching payments:', err);
        this.isLoading = false;
      }
    });

    this.feesService.getInvoices().subscribe({
      next: (data) => this.invoices = data,
      error: (err) => console.error('Error fetching invoices:', err)
    });

    this.feesService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
        if (data.length > 0 && this.invoiceForm.items[0]) {
          this.invoiceForm.items[0].feeCategoryId = data[0].id;
        }
      },
      error: (err) => console.error('Error fetching categories:', err)
    });

    this.feesService.getTemplates().subscribe({
      next: (data) => this.templates = data,
      error: (err) => console.error('Error fetching templates:', err)
    });

    this.feesService.getDiscounts().subscribe({
      next: (data) => this.discounts = data,
      error: (err) => console.error('Error fetching discounts:', err)
    });
  }

  loadStudents(): void {
    this.studentService.getStudents().subscribe({
      next: (data) => this.students = data,
      error: (err) => console.error('Error fetching students:', err)
    });
  }

  setActiveTab(tab: 'payments' | 'invoices' | 'categories' | 'templates' | 'discounts' | 'methods'): void {
    this.activeTab = tab;
    this.errorMessage = '';
    this.successMessage = '';
  }

  // Summary Metrics
  get totalRevenueCollected(): number {
    return this.payments.reduce((sum, p) => sum + (p.amount || 0), 0);
  }

  get totalOutstandingBalance(): number {
    return this.invoices.reduce((sum, i) => sum + (i.balance || 0), 0);
  }

  get totalInvoicesCount(): number {
    return this.invoices.length;
  }

  get overdueInvoicesCount(): number {
    return this.invoices.filter(i => i.status === FeeInvoiceStatus.Overdue || (i.balance > 0 && new Date(i.dueDateUtc) < new Date())).length;
  }

  // Filtered Payments
  get filteredPayments(): PaymentTransactionDto[] {
    return this.payments.filter(p => {
      const matchesSearch = !this.searchQuery ||
        p.transactionReference.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        p.invoiceNumber.toLowerCase().includes(this.searchQuery.toLowerCase());
      
      const matchesMode = this.selectedPaymentMode === 'ALL' ||
        p.paymentMode.toString() === this.selectedPaymentMode ||
        this.getPaymentModeName(p.paymentMode).toLowerCase() === this.selectedPaymentMode.toLowerCase();

      return matchesSearch && matchesMode;
    });
  }

  // Filtered Invoices
  get filteredInvoices(): FeeInvoiceDto[] {
    return this.invoices.filter(i => {
      const matchesSearch = !this.searchQuery ||
        i.invoiceNumber.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        i.studentName.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        i.admissionNumber.toLowerCase().includes(this.searchQuery.toLowerCase());

      const matchesStatus = this.selectedInvoiceStatus === 'ALL' ||
        i.status.toString() === this.selectedInvoiceStatus;

      return matchesSearch && matchesStatus;
    });
  }

  // Payment Methods Control
  togglePaymentMethod(method: ConfigurablePaymentMethod): void {
    method.enabled = !method.enabled;
    this.successMessage = `${method.name} has been ${method.enabled ? 'enabled' : 'disabled'}.`;
  }

  openConfigureMethodModal(method: ConfigurablePaymentMethod): void {
    this.selectedMethodToConfig = method;
    this.showConfigureMethodModal = true;
  }

  closeConfigureMethodModal(): void {
    this.showConfigureMethodModal = false;
    this.selectedMethodToConfig = null;
  }

  saveMethodConfiguration(): void {
    if (this.selectedMethodToConfig) {
      this.successMessage = `Configuration for ${this.selectedMethodToConfig.name} updated successfully!`;
    }
    this.closeConfigureMethodModal();
  }

  // Record Payment Modal
  openRecordPaymentModal(invoice?: FeeInvoiceDto): void {
    this.errorMessage = '';
    this.successMessage = '';
    
    if (invoice) {
      this.paymentForm.feeInvoiceId = invoice.id;
      this.selectedInvoiceBalance = invoice.balance;
      this.paymentForm.amount = invoice.balance;
    } else {
      this.paymentForm.feeInvoiceId = this.invoices.length > 0 ? this.invoices[0].id : '';
      this.onInvoiceSelectChange();
    }

    this.paymentForm.transactionReference = 'TRX-' + Math.floor(100000 + Math.random() * 900000);
    this.showRecordPaymentModal = true;
  }

  closeRecordPaymentModal(): void {
    this.showRecordPaymentModal = false;
  }

  onInvoiceSelectChange(): void {
    const found = this.invoices.find(i => i.id === this.paymentForm.feeInvoiceId);
    if (found) {
      this.selectedInvoiceBalance = found.balance;
      this.paymentForm.amount = found.balance;
    } else {
      this.selectedInvoiceBalance = 0;
      this.paymentForm.amount = 0;
    }
  }

  submitRecordPayment(): void {
    if (!this.paymentForm.feeInvoiceId) {
      this.errorMessage = 'Please select an invoice.';
      return;
    }
    if (!this.paymentForm.amount || this.paymentForm.amount <= 0) {
      this.errorMessage = 'Please enter a valid payment amount.';
      return;
    }
    if (this.paymentForm.amount > this.selectedInvoiceBalance) {
      this.errorMessage = `Payment amount exceeds remaining invoice balance of KSh ${this.selectedInvoiceBalance.toLocaleString()}.`;
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.feesService.recordPayment(this.paymentForm).subscribe({
      next: (recorded) => {
        this.successMessage = `Payment of KSh ${recorded.amount.toLocaleString()} recorded successfully! Ref: ${recorded.transactionReference}`;
        this.showRecordPaymentModal = false;
        this.isLoading = false;
        this.loadData();
      },
      error: (err) => {
        this.errorMessage = err.error?.errors?.[0] || err.message || 'Failed to record payment.';
        this.isLoading = false;
      }
    });
  }

  // Create Invoice Modal
  openCreateInvoiceModal(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.invoiceForm.studentId = this.students.length > 0 ? this.students[0].id : '';
    this.showCreateInvoiceModal = true;
  }

  closeCreateInvoiceModal(): void {
    this.showCreateInvoiceModal = false;
  }

  addInvoiceItem(): void {
    const defaultCatId = this.categories.length > 0 ? this.categories[0].id : '';
    this.invoiceForm.items.push({ feeCategoryId: defaultCatId, description: 'Fee Item', amount: 100 });
  }

  removeInvoiceItem(index: number): void {
    if (this.invoiceForm.items.length > 1) {
      this.invoiceForm.items.splice(index, 1);
    }
  }

  get invoiceFormTotal(): number {
    return this.invoiceForm.items.reduce((sum, item) => sum + (item.amount || 0), 0);
  }

  submitCreateInvoice(): void {
    if (!this.invoiceForm.studentId) {
      this.errorMessage = 'Please select a student.';
      return;
    }
    if (this.invoiceForm.items.length === 0 || this.invoiceFormTotal <= 0) {
      this.errorMessage = 'Please add at least one valid invoice item.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.feesService.createInvoice(this.invoiceForm).subscribe({
      next: (inv) => {
        this.successMessage = `Invoice ${inv.invoiceNumber} created successfully!`;
        this.showCreateInvoiceModal = false;
        this.isLoading = false;
        this.loadData();
      },
      error: (err) => {
        this.errorMessage = err.error?.errors?.[0] || err.message || 'Failed to create invoice.';
        this.isLoading = false;
      }
    });
  }

  // Create Category Modal
  openCreateCategoryModal(): void {
    this.categoryForm = { name: '', code: '', description: '' };
    this.showCreateCategoryModal = true;
  }

  closeCreateCategoryModal(): void {
    this.showCreateCategoryModal = false;
  }

  submitCreateCategory(): void {
    if (!this.categoryForm.name.trim() || !this.categoryForm.code.trim()) {
      this.errorMessage = 'Category name and code are required.';
      return;
    }

    this.isLoading = true;
    this.feesService.createCategory(this.categoryForm).subscribe({
      next: (cat) => {
        this.successMessage = `Fee Category '${cat.name}' created!`;
        this.showCreateCategoryModal = false;
        this.isLoading = false;
        this.loadData();
      },
      error: (err) => {
        this.errorMessage = err.error?.errors?.[0] || err.message || 'Failed to create fee category.';
        this.isLoading = false;
      }
    });
  }

  // Receipt Modal
  viewReceipt(payment: PaymentTransactionDto): void {
    this.selectedReceipt = payment;
    this.associatedInvoice = this.invoices.find(i => i.id === payment.feeInvoiceId) || null;
    this.showReceiptModal = true;
  }

  closeReceiptModal(): void {
    this.showReceiptModal = false;
    this.selectedReceipt = null;
    this.associatedInvoice = null;
  }

  printReceipt(): void {
    window.print();
  }

  // Helpers
  getPaymentModeName(mode: number | PaymentMode): string {
    switch (mode) {
      case PaymentMode.Cash: return 'Cash';
      case PaymentMode.BankTransfer: return 'Bank Transfer';
      case PaymentMode.MobileMoney: return 'Mobile Money (M-Pesa)';
      case PaymentMode.Cheque: return 'Cheque';
      case PaymentMode.Card: return 'Credit/Debit Card';
      default: return 'Other';
    }
  }

  getPaymentModeIcon(mode: number | PaymentMode): string {
    switch (mode) {
      case PaymentMode.Cash: return '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">attach_money</span>';
      case PaymentMode.BankTransfer: return '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">account_balance</span>';
      case PaymentMode.MobileMoney: return '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">smartphone</span>';
      case PaymentMode.Cheque: return '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">note</span>';
      case PaymentMode.Card: return '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">credit_card</span>';
      default: return '<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">payments</span>';
    }
  }

  getInvoiceStatusBadge(status: FeeInvoiceStatus): string {
    switch (status) {
      case FeeInvoiceStatus.Paid:           return 'badge badge-paid';
      case FeeInvoiceStatus.PartiallyPaid:  return 'badge badge-partial';
      case FeeInvoiceStatus.Overdue:        return 'badge badge-overdue';
      case FeeInvoiceStatus.Cancelled:      return 'badge badge-neutral';
      default:                              return 'badge badge-pending';
    }
  }

  getInvoiceStatusText(status: FeeInvoiceStatus): string {
    switch (status) {
      case FeeInvoiceStatus.Paid: return 'Paid';
      case FeeInvoiceStatus.PartiallyPaid: return 'Partial';
      case FeeInvoiceStatus.Overdue: return 'Overdue';
      case FeeInvoiceStatus.Cancelled: return 'Cancelled';
      default: return 'Pending';
    }
  }

  goBack(): void {
    this.router.navigate(['/dashboard']);
  }
}
