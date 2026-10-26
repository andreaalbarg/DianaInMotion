let _backendUrl = ''

export function setBackendUrl(url: string) {
  _backendUrl = url
}

export function apiBase(path: string): string {
  return _backendUrl ? `${_backendUrl}${path}` : path
}
