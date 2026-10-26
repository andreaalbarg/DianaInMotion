import axios from 'axios'
import { apiBase } from './config'

const adminApi = axios.create({ timeout: 15000 })

adminApi.interceptors.request.use(config => {
  config.baseURL = apiBase('/api/admin')
  const token = localStorage.getItem('admin_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

adminApi.interceptors.response.use(
  res => res,
  err => {
    if (err?.response?.status === 401) {
      localStorage.removeItem('admin_token')
    }
    return Promise.reject(err)
  }
)

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
  isFeatured: boolean
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

export async function setFeatured(id: string, isFeatured: boolean): Promise<void> {
  await adminApi.patch(`/events/${id}/featured`, { isFeatured })
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

export interface YouTubeSettings {
  apiKey: string
  channelId: string
}

export interface YouTubeVideoPreview {
  videoId: string
  title: string
  thumbnailUrl: string
  url: string
}

export async function fetchYouTubeSettings(): Promise<YouTubeSettings> {
  const { data } = await adminApi.get<YouTubeSettings>('/youtube')
  return data
}

export async function saveYouTubeSettings(settings: YouTubeSettings): Promise<void> {
  await adminApi.put('/youtube', settings)
}

export async function testYouTubeConnection(settings: YouTubeSettings): Promise<YouTubeVideoPreview[]> {
  const { data } = await adminApi.post<YouTubeVideoPreview[]>('/youtube/test', settings)
  return data
}
