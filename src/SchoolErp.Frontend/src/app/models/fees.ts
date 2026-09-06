export enum PaymentMode {
  Cash = 0,
  BankTransfer = 1,
  MobileMoney = 2,
  Cheque = 3,
  Card = 4
}

export enum FeeInvoiceStatus {
  Pending = 0,
  PartiallyPaid = 1,
  Paid = 2,
  Overdue = 3,
  Cancelled = 4
}

export enum PaymentStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Refunded = 3
}

export enum BillingFrequency {
  OneTime = 0,
  Monthly = 1,
  Termly = 2,
  Annually = 3
}

export enum DiscountType {
  Percentage = 0,
  FixedAmount = 1
}

export interface InvoiceItemDto {
  id?: string;
  feeCategoryId: string;
  categoryName?: string;
  description: string;
  amount: number;
  displayOrder?: number;
}

export interface PaymentTransactionDto {
  id: string;
  feeInvoiceId: string;
  invoiceNumber: string;
  transactionReference: string;
  paymentMode: PaymentMode;
  amount: number;
  paymentDateUtc: string;
  notes?: string;
  status: PaymentStatus;
  receivedByUserName?: string;
}

export interface FeeInvoiceDto {
  id: string;
  invoiceNumber: string;
  studentId: string;
  studentName: string;
  admissionNumber: string;
  sectionName?: string;
  description?: string;
  amount: number;
  amountPaid: number;
  discountAmount: number;
  balance: number;
  issuedOnUtc: string;
  dueDateUtc: string;
  status: FeeInvoiceStatus;
  academicYear?: string;
  term?: string;
  items: InvoiceItemDto[];
  paymentTransactions: PaymentTransactionDto[];
}

export interface CreateInvoiceItemRequest {
  feeCategoryId: string;
  description: string;
  amount: number;
  displayOrder?: number;
}

export interface CreateInvoiceRequest {
  studentId: string;
  academicYear: string;
  term: string;
  description?: string;
  dueDateUtc: string;
  discountId?: string;
  items: CreateInvoiceItemRequest[];
  sendNotification?: boolean;
  generatePdf?: boolean;
}

export interface CreatePaymentRequest {
  feeInvoiceId: string;
  transactionReference: string;
  paymentMode: PaymentMode;
  amount: number;
  paymentDateUtc: string;
  notes?: string;
}

export interface FeeCategoryDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
}

export interface CreateFeeCategoryRequest {
  name: string;
  code: string;
  description?: string;
}

export interface FeeTemplateItemDto {
  id?: string;
  feeCategoryId: string;
  categoryName?: string;
  amount: number;
  isOptional?: boolean;
  displayOrder?: number;
}

export interface FeeTemplateDto {
  id: string;
  name: string;
  description?: string;
  billingFrequency: BillingFrequency;
  isActive: boolean;
  items: FeeTemplateItemDto[];
  targetSectionIds: string[];
}

export interface CreateFeeTemplateRequest {
  name: string;
  description?: string;
  billingFrequency: BillingFrequency;
  targetSectionIds: string[];
  items: {
    feeCategoryId: string;
    amount: number;
    isOptional?: boolean;
    displayOrder?: number;
  }[];
}

export interface DiscountDto {
  id: string;
  code: string;
  name: string;
  description?: string;
  type: DiscountType;
  value: number;
  validFromUtc?: string;
  validUntilUtc?: string;
  isActive: boolean;
  maxUses: number;
  timesUsed: number;
}

export interface CreateDiscountRequest {
  code: string;
  name: string;
  description?: string;
  type: DiscountType;
  value: number;
  validFromUtc?: string;
  validUntilUtc?: string;
  maxUses?: number;
}

export interface StudentBalanceDto {
  studentId: string;
  studentName: string;
  admissionNumber: string;
  totalOutstanding: number;
  totalPaid: number;
  overdueInvoices: number;
  recentInvoices: FeeInvoiceDto[];
}
