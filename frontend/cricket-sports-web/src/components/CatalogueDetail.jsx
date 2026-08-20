import { useEffect, useState } from 'react'
import { getCatalogItem } from '../api/catalog'

const config = {
  Teams: { resource: 'teams', label: 'Team' },
  Players: { resource: 'players', label: 'Player' },
  Tournaments: { resource: 'tournaments', label: 'Tournament' }
}

export default function CatalogueDetail({ selection, back }) {
  const options = config[selection.kind]; const [item, setItem] = useState(null); const [status, setStatus] = useState('loading')
  useEffect(() => { setStatus('loading'); getCatalogItem(options.resource, selection.id).then(data => { setItem(data); setStatus('ready') }).catch(() => setStatus('error')) }, [options.resource, selection.id])
  return <main className="listing detail-page"><button className="back" onClick={back}>← All {selection.kind.toLowerCase()}</button>{status === 'loading' && <div className="state-card">Loading {options.label.toLowerCase()}…</div>}{status === 'error' && <div className="state-card">This {options.label.toLowerCase()} could not be loaded.</div>}{status === 'ready' && <Detail kind={selection.kind} item={item} />}</main>
}

function Detail({ kind, item }) {
  if (kind === 'Teams') return <section className="profile"><p>TEAM PROFILE</p><div className="profile-heading"><div className="initials large">{item.shortName}</div><div><h1>{item.name}</h1><span>{item.countryOrRegion}</span></div></div><div className="profile-stats"><article><small>SHORT NAME</small><b>{item.shortName}</b></article><article><small>STATUS</small><b>{item.isActive ? 'Active squad' : 'Inactive squad'}</b></article><article><small>REGION</small><b>{item.countryOrRegion}</b></article></div></section>
  if (kind === 'Players') return <section className="profile"><p>PLAYER PROFILE</p><div className="profile-heading"><div className="initials large">{item.fullName.split(' ').map(part => part[0]).join('').slice(0, 2)}</div><div><h1>{item.fullName}</h1><span>{item.role} · {item.teamName || item.countryOrRegion || 'Independent player'}</span></div></div><div className="profile-stats"><article><small>ROLE</small><b>{item.role}</b></article><article><small>TEAM</small><b>{item.teamName || 'Not assigned'}</b></article><article><small>BATTING STYLE</small><b>{item.battingStyle || 'Not listed'}</b></article><article><small>BOWLING STYLE</small><b>{item.bowlingStyle || 'Not listed'}</b></article></div></section>
  return <section className="profile"><p>TOURNAMENT PROFILE</p><div className="profile-heading"><div><h1>{item.name}</h1><span>{item.format} · {item.season}</span></div></div><div className="profile-stats"><article><small>DATES</small><b>{new Date(item.startDate).toLocaleDateString()} — {new Date(item.endDate).toLocaleDateString()}</b></article><article><small>FORMAT</small><b>{item.format}</b></article><article><small>TEAMS</small><b>{item.teams.length} participating</b></article></div><h2 className="squad-title">Participating teams</h2><div className="team-chips">{item.teams.length ? item.teams.map(team => <span key={team.id}>{team.shortName} · {team.name}</span>) : <span>No teams have been added yet.</span>}</div></section>
}
