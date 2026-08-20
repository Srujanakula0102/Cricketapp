const API_BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'
export async function getMatches() { const response = await fetch(`${API_BASE}/api/matches?page=1&pageSize=20`); if (!response.ok) throw new Error('Unable to load matches'); return response.json() }
