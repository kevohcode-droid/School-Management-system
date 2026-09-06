import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  FeeInvoiceDto,
  PaymentTransactionDto,
  CreateInvoiceRequest,
  CreatePaymentRequest,
  FeeCategoryDto,
  CreateFeeCategoryRequest,
  FeeTemplateDto,
  CreateFeeTemplateRequest,
  DiscountDto,
  CreateDiscountRequest,
  StudentBalanceDto
} from '../models/fees';

@Injectable({
  providedIn: 'root'
})
export class FeesService {
  constructor(private apiService: ApiService) {}

  // Invoices
  getInvoices(): Observable<FeeInvoiceDto[]> {
    return this.apiService.get<FeeInvoiceDto[]>('/fees/invoices');
  }

  getInvoiceById(id: string): Observable<FeeInvoiceDto> {
    return this.apiService.get<FeeInvoiceDto>(`/fees/invoices/${id}`);
  }

  createInvoice(request: CreateInvoiceRequest): Observable<FeeInvoiceDto> {
    return this.apiService.post<FeeInvoiceDto>('/fees/invoices', request);
  }

  getStudentBalance(studentId: string): Observable<StudentBalanceDto> {
    return this.apiService.get<StudentBalanceDto>(`/fees/students/${studentId}/balance`);
  }

  // Payments
  getPayments(): Observable<PaymentTransactionDto[]> {
    return this.apiService.get<PaymentTransactionDto[]>('/fees/payments');
  }

  recordPayment(request: CreatePaymentRequest): Observable<PaymentTransactionDto> {
    return this.apiService.post<PaymentTransactionDto>('/fees/payments', request);
  }

  // Fee Categories
  getCategories(): Observable<FeeCategoryDto[]> {
    return this.apiService.get<FeeCategoryDto[]>('/fees/categories');
  }

  createCategory(request: CreateFeeCategoryRequest): Observable<FeeCategoryDto> {
    return this.apiService.post<FeeCategoryDto>('/fees/categories', request);
  }

  // Templates
  getTemplates(): Observable<FeeTemplateDto[]> {
    return this.apiService.get<FeeTemplateDto[]>('/fees/templates');
  }

  createTemplate(request: CreateFeeTemplateRequest): Observable<FeeTemplateDto> {
    return this.apiService.post<FeeTemplateDto>('/fees/templates', request);
  }

  generateInvoicesFromTemplate(templateId: string, academicYear: string, term: string): Observable<any> {
    return this.apiService.post<any>(`/fees/templates/${templateId}/generate-invoices?academicYear=${encodeURIComponent(academicYear)}&term=${encodeURIComponent(term)}`, {});
  }

  // Discounts
  getDiscounts(): Observable<DiscountDto[]> {
    return this.apiService.get<DiscountDto[]>('/fees/discounts');
  }

  createDiscount(request: CreateDiscountRequest): Observable<DiscountDto> {
    return this.apiService.post<DiscountDto>('/fees/discounts', request);
  }
}
