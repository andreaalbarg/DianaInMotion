export interface Event {
  id: string
  title: string
  description: string
  category: 'basics' | 'advanced'
  scheduledAt: string
  speaker: string | null
  speakerPhotoUrl: string | null
  recordingUrl: string | null
  isPast: boolean
  registrationCount: number
}
