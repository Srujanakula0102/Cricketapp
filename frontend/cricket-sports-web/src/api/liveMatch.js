import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'
const guid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i

export function subscribeToMatch(matchId, handlers) {
  if (!guid.test(matchId)) return () => {}
  const connection = new HubConnectionBuilder().withUrl(`${API_BASE}/hubs/matches`).withAutomaticReconnect().configureLogging(LogLevel.Warning).build()
  connection.on('ScoreUpdated', handlers.onScore)
  connection.on('DeliveryRecorded', handlers.onDelivery)
  connection.on('MatchCompleted', handlers.onMatchCompleted)
  connection.start().then(() => connection.invoke('JoinMatch', matchId)).catch(() => {})
  return () => { connection.stop().catch(() => {}) }
}
