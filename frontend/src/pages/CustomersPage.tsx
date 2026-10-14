import { useEffect, useState, type FormEvent } from 'react'
import { createCustomer, deleteCustomer, getCustomers, updateCustomer } from '../api'
import type { Customer } from '../types'
import { emptyToNull } from '../utils'

// Formulärets fält. Alla är text så att de passar i <input>.
const emptyForm = { name: '', orgNumber: '', email: '', phone: '', city: '' }

export default function CustomersPage() {
  const [customers, setCustomers] = useState<Customer[]>([])
  const [search, setSearch] = useState('')
  const [error, setError] = useState('')

  // null = formuläret är stängt, 0 = ny kund, annars id på kunden vi redigerar.
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState(emptyForm)

  // Hämta kunder varje gång söktexten ändras.
  useEffect(() => {
    let ignore = false // Skydd om ett gammalt svar kommer tillbaka efter ett nyare.
    getCustomers(search)
      .then((data) => {
        if (!ignore) setCustomers(data)
      })
      .catch((err) => setError(err.message))
    return () => {
      ignore = true
    }
  }, [search])

  async function reload() {
    setCustomers(await getCustomers(search))
  }

  function startCreate() {
    setForm(emptyForm)
    setEditingId(0)
  }

  function startEdit(c: Customer) {
    setForm({
      name: c.name,
      orgNumber: c.orgNumber ?? '',
      email: c.email ?? '',
      phone: c.phone ?? '',
      city: c.city ?? '',
    })
    setEditingId(c.id)
  }

  async function handleSave(e: FormEvent) {
    e.preventDefault()
    setError('')
    const data = {
      name: form.name.trim(),
      orgNumber: emptyToNull(form.orgNumber),
      email: emptyToNull(form.email),
      phone: emptyToNull(form.phone),
      city: emptyToNull(form.city),
    }
    try {
      if (editingId) {
        await updateCustomer(editingId, data)
      } else {
        await createCustomer(data)
      }
      setEditingId(null)
      await reload()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  async function handleDelete(c: Customer) {
    if (!confirm(`Ta bort ${c.name}? Kundens kontakter och affärer tas också bort.`)) return
    try {
      await deleteCustomer(c.id)
      await reload()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  return (
    <section>
      <div className="page-header">
        <h2>Kunder</h2>
        <button onClick={startCreate}>+ Ny kund</button>
      </div>

      {error && <p className="error">{error}</p>}

      {editingId !== null && (
        <form className="card form-grid" onSubmit={handleSave}>
          <h3>{editingId ? 'Redigera kund' : 'Ny kund'}</h3>
          <label>
            Namn *
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          </label>
          <label>
            Org.nr
            <input value={form.orgNumber} onChange={(e) => setForm({ ...form, orgNumber: e.target.value })} />
          </label>
          <label>
            E-post
            <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </label>
          <label>
            Telefon
            <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
          </label>
          <label>
            Ort
            <input value={form.city} onChange={(e) => setForm({ ...form, city: e.target.value })} />
          </label>
          <div className="form-actions">
            <button type="submit">Spara</button>
            <button type="button" className="secondary" onClick={() => setEditingId(null)}>
              Avbryt
            </button>
          </div>
        </form>
      )}

      <input
        className="search"
        placeholder="Sök på namn, e-post, ort eller org.nr…"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      <table>
        <thead>
          <tr>
            <th>Namn</th>
            <th>Org.nr</th>
            <th>E-post</th>
            <th>Telefon</th>
            <th>Ort</th>
            <th>Kontakter</th>
            <th>Affärer</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {customers.map((c) => (
            <tr key={c.id}>
              <td>{c.name}</td>
              <td>{c.orgNumber}</td>
              <td>{c.email}</td>
              <td>{c.phone}</td>
              <td>{c.city}</td>
              <td>{c.contactCount}</td>
              <td>{c.dealCount}</td>
              <td className="row-actions">
                <button className="secondary" onClick={() => startEdit(c)}>
                  Redigera
                </button>
                <button className="danger" onClick={() => handleDelete(c)}>
                  Ta bort
                </button>
              </td>
            </tr>
          ))}
          {customers.length === 0 && (
            <tr>
              <td colSpan={8} className="muted">
                Inga kunder hittades.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </section>
  )
}
