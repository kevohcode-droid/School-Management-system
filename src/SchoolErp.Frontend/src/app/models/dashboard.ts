export interface DashboardSummary {
    totalStudents: number;
    totalStaff: number;
    totalClasses: number;
    attendancePercentage: number | null;
    pendingFees: number;
    todayCollections: number;
    newAdmissions: number;
    newStaff: number;
    monthlyCollections: number;
    activeUsers: number;
    lastLogin: string | null;
    notifications: number;
    academicYear: string | null;
    currentTerm: string | null;
    currentSemester: string | null;
}

export interface DashboardPublic {
    totalStudents: number;
    totalStaff: number;
    totalParents: number;
    totalClasses: number;
    academicYear: string | null;
    currentTerm: string | null;
    announcements: string[];
}