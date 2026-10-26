import axios from 'axios'

declare const __BACKEND_URL__: string
const _backendBase = typeof __BACKEND_URL__ !== 'undefined' && __BACKEND_URL__
  ? __BACKEND_URL__
  : ''
const api = axios.create({ baseURL: `${_backendBase}/api` })

export async function fetchUpcomingEvents() {
  const { data } = await api.get<import('../types').Event[]>('/events/upcoming')
  return data
}

export async function fetchPastEvents() {
  const { data } = await api.get<import('../types').Event[]>('/events/past')
  return data
}

export async function fetchFeaturedEvents() {
  const { data } = await api.get<import('../types').Event[]>('/events/featured')
  return data
}

export async function registerForEvent(eventId: string, name: string, email: string) {
  const { data } = await api.post(`/events/${eventId}/register`, { name, email })
  return data as { message: string }
}

export async function subscribeNewsletter(email: string) {
  const { data } = await api.post('/subscribers', { email })
  return data as { message: string }
}
