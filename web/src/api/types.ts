// Contratos espelhando os DTOs da API (JSON em camelCase).

export interface EventResponse {
  id: string;
  name: string;
  description?: string | null;
  location?: string | null;
  startsAt: string;
  capacity: number;
  registeredCount: number;
}

export interface CourseResponse {
  id: string;
  name: string;
  code?: string | null;
}

export interface SpeakerResponse {
  id: string;
  eventId: string;
  name: string;
  role?: string | null;
  talk?: string | null;
  bio?: string | null;
  photoUrl?: string | null;
  sortOrder: number;
}

export interface RegistrationResponse {
  id: string;
  eventId: string;
  participantName: string;
  participantEmail: string;
  accessCode: string;
  status: string;
  course: string;
  semester?: number | null;
  birthDate: string;
  consentAccepted: boolean;
  consentAcceptedAt?: string | null;
  createdAt: string;
  checkedInAt?: string | null;
}

export interface RegistrationListItem {
  id: string;
  participantName: string;
  participantEmail: string;
  course: string;
  semester?: number | null;
  status: string;
  createdAt: string;
  checkedInAt?: string | null;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  role: string;
}

export interface ValidateCodeResponse {
  valid: boolean;
  reason: string;
  participantName?: string | null;
  eventId?: string | null;
  checkedInAt?: string | null;
}

export interface GateLookupResponse {
  found: boolean;
  reason: string;
  registrationId?: string | null;
  participantName?: string | null;
  course?: string | null;
  semester?: number | null;
  birthDate?: string | null;
  email?: string | null;
  status?: string | null;
  alreadyCheckedIn: boolean;
  checkedInAt?: string | null;
  photoDataUri?: string | null;
}

export interface CheckInStatsResponse {
  eventId: string;
  capacity: number;
  totalRegistered: number;
  checkedIn: number;
  pending: number;
}

export interface SessionResponse {
  id: string;
  eventId: string;
  title: string;
  speaker?: string | null;
  room?: string | null;
  startsAt: string;
  endsAt?: string | null;
}

export interface SessionAttendResponse {
  success: boolean;
  reason: string;
  participantName?: string | null;
  sessionTitle?: string | null;
  alreadyAttended: boolean;
}

export interface SessionStat {
  title: string;
  attendance: number;
}

export interface CourseStat {
  course: string;
  registered: number;
  checkedIn: number;
}

export interface SemesterStat {
  label: string;
  registered: number;
}

export interface AccessByCodeRequest {
  accessCode: string;
  email: string;
}
