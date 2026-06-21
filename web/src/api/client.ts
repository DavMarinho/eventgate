import type {
  AccessByCodeRequest,
  CheckInStatsResponse,
  CourseResponse,
  CourseStat,
  EventResponse,
  GateLookupResponse,
  LoginResponse,
  RegistrationListItem,
  RegistrationResponse,
  SemesterStat,
  ValidateCodeResponse,
} from './types';

const BASE_URL = (import.meta.env.VITE_API_URL ?? 'http://localhost:5080').replace(/\/$/, '');

const TOKEN_KEY = 'eventgate.token';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null): void {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token);
  } else {
    localStorage.removeItem(TOKEN_KEY);
  }
}

/** Erro de API com mensagem amigável extraída do ProblemDetails. */
export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

interface RequestOptions {
  method?: string;
  body?: unknown;
  auth?: boolean;
  formData?: FormData;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, auth = false, formData } = options;

  const headers: Record<string, string> = {};
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }
  if (auth) {
    const token = getToken();
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
  }

  let res: Response;
  try {
    res = await fetch(`${BASE_URL}${path}`, {
      method,
      headers,
      // FormData: deixa o navegador definir o Content-Type (com boundary).
      body: formData ?? (body !== undefined ? JSON.stringify(body) : undefined),
    });
  } catch {
    throw new ApiError('Não foi possível conectar à API. Ela está rodando?', 0);
  }

  if (res.status === 204) {
    return undefined as T;
  }

  const text = await res.text();
  const data = text ? safeParse(text) : null;

  if (!res.ok) {
    const detail =
      (data && typeof data === 'object' && 'detail' in data && (data as { detail?: string }).detail) ||
      (data && typeof data === 'object' && 'title' in data && (data as { title?: string }).title) ||
      `Erro ${res.status}`;
    throw new ApiError(String(detail), res.status);
  }

  return data as T;
}

function safeParse(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

export const api = {
  // Auth
  login: (email: string, password: string) =>
    request<LoginResponse>('/api/auth/login', { method: 'POST', body: { email, password } }),

  // Courses
  listCourses: () => request<CourseResponse[]>('/api/courses'),
  createCourse: (name: string, code?: string) =>
    request<CourseResponse>('/api/courses', { method: 'POST', auth: true, body: { name, code } }),

  // Speakers
  listSpeakers: (eventId: string) =>
    request<import('./types').SpeakerResponse[]>(`/api/events/${eventId}/speakers`),
  createSpeaker: (
    eventId: string,
    payload: { name: string; role?: string; talk?: string; bio?: string; photoUrl?: string; sortOrder?: number },
  ) =>
    request<import('./types').SpeakerResponse>(`/api/events/${eventId}/speakers`, {
      method: 'POST',
      auth: true,
      body: payload,
    }),
  deleteSpeaker: (id: string) =>
    request<void>(`/api/speakers/${id}`, { method: 'DELETE', auth: true }),

  // Sessions (palestras)
  listSessions: (eventId: string) =>
    request<import('./types').SessionResponse[]>(`/api/events/${eventId}/sessions`),
  createSession: (
    eventId: string,
    payload: { title: string; speaker?: string; room?: string; startsAt: string; endsAt?: string },
  ) =>
    request<import('./types').SessionResponse>(`/api/events/${eventId}/sessions`, {
      method: 'POST',
      auth: true,
      body: payload,
    }),
  deleteSession: (id: string) => request<void>(`/api/sessions/${id}`, { method: 'DELETE', auth: true }),
  attendSession: (sessionId: string, accessCode: string) =>
    request<import('./types').SessionAttendResponse>(`/api/checkin/sessions/${sessionId}/attend`, {
      method: 'POST',
      auth: true,
      body: { accessCode },
    }),
  dashboardBySession: (eventId: string) =>
    request<import('./types').SessionStat[]>(`/api/dashboard/events/${eventId}/by-session`, { auth: true }),

  // Staff
  registerStaff: (email: string, password: string, role: string) =>
    request<{ id: string; email: string; role: string }>('/api/auth/register-staff', {
      method: 'POST',
      auth: true,
      body: { email, password, role },
    }),

  // Events
  listEvents: () => request<EventResponse[]>('/api/events'),
  getEvent: (id: string) => request<EventResponse>(`/api/events/${id}`),
  createEvent: (payload: {
    name: string;
    description?: string;
    location?: string;
    startsAt: string;
    capacity: number;
  }) => request<EventResponse>('/api/events', { method: 'POST', auth: true, body: payload }),

  // Registration (público, multipart por causa da foto)
  register: (eventId: string, form: FormData) =>
    request<RegistrationResponse>(`/api/events/${eventId}/registrations`, { method: 'POST', formData: form }),

  // LGPD
  getMyData: (payload: AccessByCodeRequest) =>
    request<RegistrationResponse>('/api/registrations/me', { method: 'POST', body: payload }),
  deleteMyData: (payload: AccessByCodeRequest) =>
    request<void>('/api/registrations/me', { method: 'DELETE', body: payload }),

  // Equipe
  listRegistrations: (eventId: string, search?: string) =>
    request<RegistrationListItem[]>(
      `/api/registrations/events/${eventId}${search ? `?search=${encodeURIComponent(search)}` : ''}`,
      { auth: true },
    ),
  lookup: (code: string) =>
    request<GateLookupResponse>(`/api/checkin/lookup?code=${encodeURIComponent(code)}`, { auth: true }),
  validateCode: (accessCode: string) =>
    request<ValidateCodeResponse>('/api/checkin/validate', { method: 'POST', auth: true, body: { accessCode } }),
  getStats: (eventId: string) =>
    request<CheckInStatsResponse>(`/api/checkin/events/${eventId}/stats`, { auth: true }),
  dashboardByCourse: (eventId: string) =>
    request<CourseStat[]>(`/api/dashboard/events/${eventId}/by-course`, { auth: true }),
  dashboardBySemester: (eventId: string) =>
    request<SemesterStat[]>(`/api/dashboard/events/${eventId}/by-semester`, { auth: true }),
};
