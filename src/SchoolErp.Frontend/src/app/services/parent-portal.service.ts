import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { ParentChildDto, ParentChildPerformance, ParentPortalState } from '../models/parent-portal';

@Injectable({
  providedIn: 'root'
})
export class ParentPortalService {
  private readonly stateSubject = new BehaviorSubject<ParentPortalState>({
    children: [],
    selectedChildId: localStorage.getItem('selectedChildId')
  });

  readonly state$ = this.stateSubject.asObservable();

  constructor(private apiService: ApiService) {}

  get state(): ParentPortalState {
    return this.stateSubject.value;
  }

  loadChildren(): Observable<ParentChildDto[]> {
    return this.apiService.get<ParentChildDto[]>('/parent/children').pipe(
      tap(children => {
        const selectedChildId = this.resolveSelectedChildId(children || []);
        this.setState({ children: children || [], selectedChildId });
      })
    );
  }

  selectChild(studentId: string): void {
    localStorage.setItem('selectedChildId', studentId);
    this.setState({ ...this.state, selectedChildId: studentId });
  }

  getSelectedChildPerformance(): Observable<ParentChildPerformance> {
    if (!this.state.selectedChildId) {
      throw new Error('No child selected.');
    }

    return this.getChildPerformance(this.state.selectedChildId);
  }

  getChildPerformance(studentId: string): Observable<ParentChildPerformance> {
    return this.apiService.get<ParentChildPerformance>(`/parent/child/${studentId}/performance`);
  }

  private resolveSelectedChildId(children: ParentChildDto[]): string | null {
    const stored = localStorage.getItem('selectedChildId');
    if (stored && children.some(child => child.studentId === stored)) {
      return stored;
    }

    const primary = children.find(child => child.isPrimaryContact);
    return primary?.studentId || children[0]?.studentId || null;
  }

  private setState(state: ParentPortalState): void {
    if (state.selectedChildId) {
      localStorage.setItem('selectedChildId', state.selectedChildId);
    } else {
      localStorage.removeItem('selectedChildId');
    }

    this.stateSubject.next(state);
  }
}
