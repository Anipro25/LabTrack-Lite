import React, { useEffect, useState } from 'react'
import { Routes, Route, Link, useNavigate } from 'react-router-dom'
import axios from 'axios'

const statusPalette = {
  Open: '#f97316',
  InProgress: '#38bdf8',
  Resolved: '#22c55e',
  Closed: '#a855f7'
}

function useAuth() {
  const [token, setToken] = useState(localStorage.getItem('jwt') || '')
  const login = async (email, password) => {
    const res = await axios.post('/api/auth/login', { email, password })
    localStorage.setItem('jwt', res.data.token)
    setToken(res.data.token)
  }
  const logout = () => {
    localStorage.removeItem('jwt')
    setToken('')
  }
  useEffect(() => {
    axios.defaults.headers.common.Authorization = token ? `Bearer ${token}` : ''
  }, [token])
  return { token, login, logout }
}

const Nav = ({ logout }) => (
  <nav className="nav" aria-label="Primary">
    <div className="nav-brand">📋 LabTrack</div>
    <div className="nav-links">
      <Link to="/">Dashboard</Link>
      <Link to="/assets">Assets</Link>
      <Link to="/tickets">Tickets</Link>
      <Link to="/chatbot">Chatbot</Link>
    </div>
    <button className="btn secondary" onClick={logout} aria-label="Logout">Logout</button>
  </nav>
)

function Dashboard() {
  return (
    <div className="card" style={{ display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center', minHeight: 400, textAlign: 'center' }}>
      <h2>Welcome to LabTrack Lite</h2>
      <p>Manage lab assets, tickets, and get quick answers via chatbot.</p>
    </div>
  )
}

function Assets() {
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ name: '', code: '', location: '', category: '', description: '' })
  const [saving, setSaving] = useState(false)
  const [editingId, setEditingId] = useState(null)
  const [editForm, setEditForm] = useState({ name: '', code: '', location: '', category: '', description: '' })

  const loadAssets = () => {
    setLoading(true)
    axios.get('/api/assets?page=1&pageSize=20').then(res => {
      setItems(res.data.items || [])
    }).finally(() => setLoading(false))
  }

  useEffect(loadAssets, [])

  const createAsset = async (e) => {
    e.preventDefault()
    setSaving(true)
    try {
      await axios.post('/api/assets', form)
      setForm({ name: '', code: '', location: '', category: '', description: '' })
      setShowForm(false)
      loadAssets()
    } catch (err) {
      alert('Failed to create asset: ' + (err.response?.data || err.message))
    } finally {
      setSaving(false)
    }
  }

  const startEdit = (asset) => {
    setEditingId(asset.id)
    setEditForm({
      name: asset.name || '',
      code: asset.code || '',
      location: asset.location || '',
      category: asset.category || '',
      description: asset.description || ''
    })
  }

  const saveEdit = async (e) => {
    e.preventDefault()
    if (!editingId) return
    setSaving(true)
    try {
      await axios.put(`/api/assets/${editingId}`, editForm)
      setEditingId(null)
      loadAssets()
    } catch (err) {
      alert('Failed to update asset: ' + (err.response?.data || err.message))
    } finally {
      setSaving(false)
    }
  }

  const deleteAsset = async (id) => {
    if (!window.confirm('Delete this asset?')) return
    try {
      await axios.delete(`/api/assets/${id}`)
      loadAssets()
    } catch (err) {
      alert('Failed to delete asset: ' + (err.response?.data || err.message))
    }
  }

  return (
    <div className="card">
      <div className="flex" style={{ justifyContent: 'space-between', marginBottom: 16 }}>
        <h3>Assets</h3>
        <button className="btn" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancel' : '+ Create Asset'}
        </button>
      </div>

      {showForm && (
        <form className="form" onSubmit={createAsset} style={{ marginBottom: 24, padding: 16, background: 'rgba(255,255,255,0.03)', borderRadius: 8 }}>
          <h4 style={{ marginTop: 0 }}>New Asset</h4>
          <label className="label">
            <span>Name *</span>
            <input className="input" value={form.name} onChange={e => setForm({...form, name: e.target.value})} required />
          </label>
          <label className="label">
            <span>Code (e.g., QR-OSC-001)</span>
            <input className="input" value={form.code} onChange={e => setForm({...form, code: e.target.value})} />
          </label>
          <label className="label">
            <span>Location</span>
            <input className="input" value={form.location} onChange={e => setForm({...form, location: e.target.value})} />
          </label>
          <label className="label">
            <span>Category</span>
            <input className="input" value={form.category} onChange={e => setForm({...form, category: e.target.value})} />
          </label>
          <label className="label">
            <span>Description</span>
            <textarea className="input" rows={3} value={form.description} onChange={e => setForm({...form, description: e.target.value})} />
          </label>
          <button className="btn" type="submit" disabled={saving}>{saving ? 'Creating...' : 'Create Asset'}</button>
        </form>
      )}

      <div className="flex" style={{ justifyContent: 'space-between', marginBottom: 8 }}>
        <span className="badge">Page 1</span>
        <span style={{ fontSize: 14, opacity: 0.7 }}>{items.length} items</span>
      </div>
      {loading ? <p role="status">Loading...</p> : (
        <table className="table" aria-label="Assets table">
          <thead>
            <tr><th>Name</th><th>Code</th><th>Location</th><th>Category</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {items.map(a => (
              <tr key={a.id}>
                <td>{a.id === editingId ? <input className="input" value={editForm.name} onChange={e => setEditForm({...editForm, name: e.target.value})} /> : a.name}</td>
                <td>{a.id === editingId ? <input className="input" value={editForm.code} onChange={e => setEditForm({...editForm, code: e.target.value})} /> : <code style={{ fontSize: 13, background: 'rgba(255,255,255,0.05)', padding: '2px 6px', borderRadius: 4 }}>{a.code}</code>}</td>
                <td>{a.id === editingId ? <input className="input" value={editForm.location} onChange={e => setEditForm({...editForm, location: e.target.value})} /> : a.location}</td>
                <td>{a.id === editingId ? <input className="input" value={editForm.category} onChange={e => setEditForm({...editForm, category: e.target.value})} /> : a.category}</td>
                <td style={{ display: 'flex', gap: 8 }}>
                  {editingId === a.id ? (
                    <>
                      <button className="btn" onClick={saveEdit} disabled={saving}>Save</button>
                      <button className="btn secondary" onClick={() => setEditingId(null)}>Cancel</button>
                    </>
                  ) : (
                    <>
                      <button className="btn secondary" onClick={() => startEdit(a)}>Edit</button>
                      <button className="btn" style={{ background: '#ef4444' }} onClick={() => deleteAsset(a.id)}>Delete</button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

function Tickets() {
  const [items, setItems] = useState([])
  const [assets, setAssets] = useState([])
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ title: '', description: '', assetId: '' })
  const [saving, setSaving] = useState(false)
  const [editingId, setEditingId] = useState(null)
  const [editForm, setEditForm] = useState({ title: '', description: '', assetId: '', status: 'Open' })

  const loadTickets = async () => {
    setLoading(true)
    try {
      const res = await axios.get('/api/tickets?page=1&pageSize=20')
      console.log('Loaded tickets:', res.data)
      setItems(res.data.items || [])
    } catch (err) {
      console.error('Failed to load tickets:', err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadTickets()
    axios.get('/api/assets?page=1&pageSize=100').then(res => setAssets(res.data.items || []))
  }, [])

  const createTicket = async (e) => {
    e.preventDefault()
    setSaving(true)
    console.log('Creating ticket with form data:', form)
    try {
      const response = await axios.post('/api/tickets', form)
      console.log('Ticket created successfully:', response.data)
      console.log('Response status:', response.status)
      
      setForm({ title: '', description: '', assetId: '' })
      setShowForm(false)
      
      // Reload tickets
      console.log('Reloading tickets...')
      await loadTickets()
      console.log('After reload, items count:', items.length)
      
      alert('Ticket created successfully!')
    } catch (err) {
      console.error('Ticket creation error:', err)
      console.error('Error response:', err.response)
      alert('Failed to create ticket: ' + (err.response?.data?.title || err.response?.data || err.message))
    } finally {
      setSaving(false)
    }
  }

  const startEdit = (ticket) => {
    setEditingId(ticket.id)
    setEditForm({
      title: ticket.title || '',
      description: ticket.description || '',
      assetId: ticket.assetId || '',
      status: ticket.status || 'Open'
    })
  }

  const saveEdit = async (e) => {
    e.preventDefault()
    if (!editingId) return
    setSaving(true)
    try {
      await axios.put(`/api/tickets/${editingId}`, {
        title: editForm.title,
        description: editForm.description,
        assetId: editForm.assetId ? editForm.assetId : null,
        status: editForm.status
      })
      setEditingId(null)
      await loadTickets()
    } catch (err) {
      alert('Failed to update ticket: ' + (err.response?.data || err.message))
    } finally {
      setSaving(false)
    }
  }

  const deleteTicket = async (id) => {
    if (!window.confirm('Delete this ticket?')) return
    try {
      await axios.delete(`/api/tickets/${id}`)
      await loadTickets()
    } catch (err) {
      alert('Failed to delete ticket: ' + (err.response?.data || err.message))
    }
  }

  return (
    <div className="card">
      <div className="flex" style={{ justifyContent: 'space-between', marginBottom: 16 }}>
        <h3>Tickets</h3>
        <button className="btn" onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Cancel' : '+ Create Ticket'}
        </button>
      </div>

      {showForm && (
        <form className="form" onSubmit={createTicket} style={{ marginBottom: 24, padding: 16, background: 'rgba(255,255,255,0.03)', borderRadius: 8 }}>
          <h4 style={{ marginTop: 0 }}>New Ticket</h4>
          <label className="label">
            <span>Title *</span>
            <input className="input" value={form.title} onChange={e => setForm({...form, title: e.target.value})} required />
          </label>
          <label className="label">
            <span>Description</span>
            <textarea className="input" rows={3} value={form.description} onChange={e => setForm({...form, description: e.target.value})} />
          </label>
          <label className="label">
            <span>Asset *</span>
            <select className="input" value={form.assetId} onChange={e => setForm({...form, assetId: e.target.value})} required>
              <option value="">Select an asset...</option>
              {assets.map(a => <option key={a.id} value={a.id}>{a.name} ({a.code})</option>)}
            </select>
          </label>
          <button className="btn" type="submit" disabled={saving}>{saving ? 'Creating...' : 'Create Ticket'}</button>
        </form>
      )}

      <div className="flex" style={{ justifyContent: 'space-between', marginBottom: 8 }}>
        <span className="badge">Page 1</span>
        <span style={{ fontSize: 14, opacity: 0.7 }}>{items.length} items</span>
      </div>
      {loading ? <p role="status">Loading...</p> : (
        <table className="table" aria-label="Tickets table">
          <thead>
            <tr><th>Title</th><th>Status</th><th>Asset</th><th>Created</th><th>Actions</th></tr>
          </thead>
          <tbody>
            {items.map(t => (
              <tr key={t.id}>
                <td>{editingId === t.id ? <input className="input" value={editForm.title} onChange={e => setEditForm({...editForm, title: e.target.value})} /> : t.title}</td>
                <td>
                  {editingId === t.id ? (
                    <select className="input" value={editForm.status} onChange={e => setEditForm({...editForm, status: e.target.value})}>
                      <option value="Open">Open</option>
                      <option value="InProgress">InProgress</option>
                      <option value="Resolved">Resolved</option>
                      <option value="Closed">Closed</option>
                    </select>
                  ) : (
                    <span className="badge" style={{ borderColor: statusPalette[t.status], color: statusPalette[t.status] }}>{t.status}</span>
                  )}
                </td>
                <td>
                  {editingId === t.id ? (
                    <select className="input" value={editForm.assetId} onChange={e => setEditForm({...editForm, assetId: e.target.value})}>
                      <option value="">Select an asset...</option>
                      {assets.map(a => <option key={a.id} value={a.id}>{a.name} ({a.code})</option>)}
                    </select>
                  ) : (
                    t.asset?.name || '—'
                  )}
                </td>
                <td>{new Date(t.createdAt).toLocaleString()}</td>
                <td style={{ display: 'flex', gap: 8 }}>
                  {editingId === t.id ? (
                    <>
                      <button className="btn" onClick={saveEdit} disabled={saving}>Save</button>
                      <button className="btn secondary" onClick={() => setEditingId(null)}>Cancel</button>
                    </>
                  ) : (
                    <>
                      <button className="btn secondary" onClick={() => startEdit(t)}>Edit</button>
                      <button className="btn" style={{ background: '#ef4444' }} onClick={() => deleteTicket(t.id)}>Delete</button>
                    </>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}

function Chatbot() {
  const [q, setQ] = useState('')
  const [answer, setAnswer] = useState('')
  const [history, setHistory] = useState([])
  const [loading, setLoading] = useState(false)

  const ask = async (e) => {
    e.preventDefault()
    if (!q.trim()) return
    
    setLoading(true)
    const question = q.trim()
    setHistory(prev => [...prev, { type: 'question', text: question }])
    
    try {
      // Local smart responses before hitting API
      const lower = question.toLowerCase()
      let response = ''
      
      if (lower.includes('help') || lower.includes('what can you do')) {
        response = `I can help you with:\n• "list assets" - Show all lab assets\n• "list tickets" - Show open tickets\n• "how to create ticket" - Instructions for ticket creation\n• "asset status" - Check asset information\n• "ticket status" - Check ticket information`
      } else if (lower.includes('create') && (lower.includes('ticket') || lower.includes('issue'))) {
        response = 'To create a ticket: Go to the Tickets page → Click "+ Create Ticket" → Fill in title, description, and select an asset → Submit. You need Engineer or Admin role.'
      } else if (lower.includes('create') && lower.includes('asset')) {
        response = 'To create an asset: Go to the Assets page → Click "+ Create Asset" → Fill in name, code (e.g., QR-OSC-001), location, category, and description → Submit. You need Engineer or Admin role.'
      } else if (lower.includes('list') && lower.includes('asset')) {
        const res = await axios.get('/api/assets?page=1&pageSize=10')
        const assets = res.data.items || []
        response = assets.length ? `Found ${assets.length} assets:\n` + assets.map(a => `• ${a.name} (${a.code}) - ${a.location}`).join('\n') : 'No assets found.'
      } else if (lower.includes('list') && lower.includes('ticket')) {
        const res = await axios.get('/api/tickets?page=1&pageSize=10')
        const tickets = res.data.items || []
        response = tickets.length ? `Found ${tickets.length} tickets:\n` + tickets.map(t => `• ${t.title} [${t.status}]`).join('\n') : 'No tickets found.'
      } else if (lower.includes('open') && lower.includes('ticket')) {
        const res = await axios.get('/api/tickets?status=Open&page=1&pageSize=10')
        const tickets = res.data.items || []
        response = tickets.length ? `Found ${tickets.length} open tickets:\n` + tickets.map(t => `• ${t.title}`).join('\n') : 'No open tickets.'
      } else {
        // Fallback to API
        const res = await axios.get('/api/chatbot', { params: { q: question } })
        response = res.data
      }
      
      setAnswer(response)
      setHistory(prev => [...prev, { type: 'answer', text: response }])
    } catch (err) {
      const errMsg = 'Error: ' + (err.response?.data || err.message)
      setAnswer(errMsg)
      setHistory(prev => [...prev, { type: 'error', text: errMsg }])
    } finally {
      setLoading(false)
      setQ('')
    }
  }

  return (
    <div className="card">
      <h3>AI Assistant</h3>
      <p style={{ fontSize: 14, opacity: 0.7, marginBottom: 16 }}>Ask about assets, tickets, or how to use the system. Try "help" to see what I can do.</p>
      
      <div style={{ maxHeight: 400, overflowY: 'auto', marginBottom: 16, padding: 12, background: 'rgba(0,0,0,0.2)', borderRadius: 8 }}>
        {history.length === 0 ? (
          <p style={{ opacity: 0.5, textAlign: 'center', margin: 0 }}>Start by asking a question...</p>
        ) : (
          history.map((item, i) => (
            <div key={i} style={{ marginBottom: 12, paddingBottom: 12, borderBottom: i < history.length - 1 ? '1px solid rgba(255,255,255,0.05)' : 'none' }}>
              <strong style={{ color: item.type === 'question' ? '#38bdf8' : item.type === 'error' ? '#f97316' : '#22c55e', display: 'block', marginBottom: 4 }}>
                {item.type === 'question' ? '❓ You:' : item.type === 'error' ? '⚠️ Error:' : '🤖 Assistant:'}
              </strong>
              <div style={{ whiteSpace: 'pre-wrap', fontSize: 14, lineHeight: 1.6 }}>{item.text}</div>
            </div>
          ))
        )}
      </div>
      
      <form className="form" onSubmit={ask}>
        <label className="label">
          <span>Ask a question</span>
          <input className="input" value={q} onChange={e => setQ(e.target.value)} placeholder="e.g., list assets, how to create ticket, help" aria-label="Chatbot question" />
        </label>
        <button className="btn" type="submit" disabled={loading}>{loading ? 'Thinking...' : 'Send'}</button>
      </form>
    </div>
  )
}

function Login({ login }) {
  const [email, setEmail] = useState('admin@example.com')
  const [password, setPassword] = useState('Admin@123')
  const [loading, setLoading] = useState(false)
  const navigate = useNavigate()

  const submit = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      await login(email, password)
      navigate('/')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="card" style={{ maxWidth: 650, width: '100%' }}>
      <h2>Login (demo JWT)</h2>
      <form className="form" onSubmit={submit}>
        <label className="label">
          <span>Email</span>
          <input className="input" value={email} onChange={e => setEmail(e.target.value)} type="email" required />
        </label>
        <label className="label">
          <span>Password</span>
          <input className="input" value={password} onChange={e => setPassword(e.target.value)} type="password" required />
        </label>
        <button className="btn" type="submit" disabled={loading}>{loading ? 'Signing in...' : 'Sign in'}</button>
      </form>
    </div>
  )
}

export default function App() {
  const { token, login, logout } = useAuth()

  if (!token) return <div className="app-shell" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '100vh' }}><Login login={login} /></div>

  return (
    <div className="app-shell">
      <Nav logout={logout} />
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/assets" element={<Assets />} />
        <Route path="/tickets" element={<Tickets />} />
        <Route path="/chatbot" element={<Chatbot />} />
      </Routes>
    </div>
  )
}
