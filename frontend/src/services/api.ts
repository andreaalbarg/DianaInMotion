import axios from 'axios'

const api = axios.create({ baseURL: '/api' })

export async function fetchUpcomingEvents() {
  const { data } = await api.get<import('../types').Event[]>('/events/upcoming')
  return data
}

export async function fetchPastEvents() {
  const { data } = await api.get<import('../types').Event[]>('/events/past')
  return data
}

export async function registerForEvent(eventId: string, name: string, email: string) {
  const { data } = await api.post(`/events/${eventId}/register`, { name, email })
  return data as { message: string }
}
