import axios from 'axios'

const adminApi = axios.create({ baseURL: '/api/admin' })

adminApi.interceptors.request.use(config => {
  const token = localStorage.getItem('admin_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export interface AdminEvent {
  id: string
  title: string
  description: string
  category: string
  scheduledAt: string
  speaker: string | null
  speakerPhotoUrl: string | null
  recordingUrl: string | null
  isPast: boolean
  registrationCount: number
}

export interface EventWriteRequest {
  title: string
  description: string
  category: string
  scheduledAt: string
  speaker?: string
  speakerPhotoUrl?: string
  recordingUrl?: string
}

export interface Registration {
  id: string
  name: string
  email: string
  eventId: string
  eventTitle: string
  registeredAt: string
}

export interface Subscriber {
  id: string
  email: string
  subscribedAt: string
}

export async function adminLogin(password: string): Promise<string> {
  const { data } = await adminApi.post<{ token: string }>('/auth/login', { password })
  return data.token
}

export async function fetchAdminEvents(): Promise<AdminEvent[]> {
  const { data } = await adminApi.get<AdminEvent[]>('/events')
  return data
}

export async function createEvent(req: EventWriteRequest): Promise<void> {
  await adminApi.post('/events', req)
}

export async function updateEvent(id: string, req: EventWriteRequest): Promise<void> {
  await adminApi.put(`/events/${id}`, req)
}

export async function deleteEvent(id: string): Promise<void> {
  await adminApi.delete(`/events/${id}`)
}

export async function fetchRegistrations(eventId?: string): Promise<Registration[]> {
  const params = eventId ? { eventId } : {}
  const { data } = await adminApi.get<Registration[]>('/registrations', { params })
  return data
}

export async function fetchSubscribers(): Promise<Subscriber[]> {
  const { data } = await adminApi.get<Subscriber[]>('/subscribers')
  return data
}

export async function deleteSubscriber(id: string): Promise<void> {
  await adminApi.delete(`/subscribers/${id}`)
}
