import { useEffect, useState } from 'react'
import { getAdminUsers, updateUserRoles } from '../api/adminUsers'

const roles = ['User', 'Scorer', 'Admin']
export default function AdminUsers({ session }) {
  const [items, setItems] = useState([]); const [status, setStatus] = useState('loading'); const [message, setMessage] = useState('')
  const load = () => { setStatus('loading'); getAdminUsers(session.accessToken).then(data => { setItems(data); setStatus('ready') }).catch(error => { setMessage(error.message); setStatus('error') }) }
  useEffect(() => { load() }, [])
  const toggle = async (user, role) => { const next = user.roles.includes(role) ? user.roles.filter(item => item !== role) : [...user.roles, role]; if (!next.length) { setMessage('Every account must have at least one role.'); return } try { const updated = await updateUserRoles(user.id, next, session.accessToken); setItems(items.map(item => item.id === updated.id ? updated : item)); setMessage(`Roles updated for ${updated.displayName}.`) } catch (error) { setMessage(error.message) } }
  return <div className="admin-block"><div className="manage-head"><h2>Manage Users</h2><button onClick={load}>Refresh</button></div><p className="admin-help">Choose roles for each account. At least one role is required; you cannot remove your own Admin role.</p>{message && <p className="admin-message">{message}</p>}{status === 'loading' && <div className="admin-empty">Loading users…</div>}{status === 'error' && <div className="admin-empty">Users could not be loaded.</div>}{status === 'ready' && !items.length && <div className="admin-empty">No registered accounts yet.</div>}{items.length > 0 && <div className="admin-list user-list">{items.map(user => <article key={user.id}><div><b>{user.displayName}</b><span>{user.email}</span></div><div className="role-pills">{roles.map(role => <label key={role}><input type="checkbox" checked={user.roles.includes(role)} onChange={() => toggle(user, role)} /> {role}</label>)}</div></article>)}</div>}</div>
}
