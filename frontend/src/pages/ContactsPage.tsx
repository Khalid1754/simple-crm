import { useEffect, useState, type FormEvent } from 'react'
import { createContact, deleteContact, getContacts, getCustomers, updateContact } from '../api'
import type { Contact, Customer } from '../types'
import { emptyToNull } from '../utils'

const emptyForm = { firstName: '', lastName: '', email: '', phone: '', jobTitle: '', customerId: '' }

export default function ContactsPage() {
  const [contacts, setContacts] = useState<Contact[]>([])
  // Kunderna behövs till rullistan i formuläret.
  const [customers, setCustomers] = useState<Customer[]>([])
  const [search, setSearch] = useState('')
  const [error, setError] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState(emptyForm)

  // Hämta kunderna en gång när sidan visas.
  useEffect(() => {
    getCustomers()
      .then(setCustomers)
      .catch((err) => setError(err.message))
  }, [])

  useEffect(() => {
    let ignore = false
    getContacts(search)
      .then((data) => {
        if (!ignore) setContacts(data)
      })
      .catch((err) => setError(err.message))
    return () => {
      ignore = true
    }
  }, [search])

  async function reload() {
    setContacts(await getContacts(search))
  }

  function startCreate() {
    setForm(emptyForm)
    setEditingId(0)
  }

  function startEdit(c: Contact) {
    setForm({
      firstName: c.firstName,
      lastName: c.lastName,
      email: c.email ?? '',
      phone: c.phone ?? '',
      jobTitle: c.jobTitle ?? '',
      customerId: String(c.customerId),
    })
    setEditingId(c.id)
  }

  async function handleSave(e: FormEvent) {
    e.preventDefault()
    setError('')
    const data = {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: emptyToNull(form.email),
      phone: emptyToNull(form.phone),
      jobTitle: emptyToNull(form.jobTitle),
      customerId: Number(form.customerId),
    }
    try {
      if (editingId) {
        await updateContact(editingId, data)
      } else {
        await createContact(data)
      }
      setEditingId(null)
      await reload()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  async function handleDelete(c: Contact) {
    if (!confirm(`Ta bort ${c.firstName} ${c.lastName}?`)) return
    try {
      await deleteContact(c.id)
      await reload()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  return (
    <section>
      <div className="page-header">
        <h2>Kontakter</h2>
        <button onClick={startCreate}>+ Ny kontakt</button>
      </div>

      {error && <p className="error">{error}</p>}

      {editingId !== null && (
        <form className="card form-grid" onSubmit={handleSave}>
          <h3>{editingId ? 'Redigera kontakt' : 'Ny kontakt'}</h3>
          <label>
            Förnamn *
            <input value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
          </label>
          <label>
            Efternamn *
            <input value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
          </label>
          <label>
            Kund *
            <select value={form.customerId} onChange={(e) => setForm({ ...form, customerId: e.target.value })} required>
              <option value="">Välj kund…</option>
              {customers.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Titel
            <input value={form.jobTitle} onChange={(e) => setForm({ ...form, jobTitle: e.target.value })} />
          </label>
          <label>
            E-post
            <input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
          </label>
          <label>
            Telefon
            <input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
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
        placeholder="Sök på namn, e-post eller kund…"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      <table>
        <thead>
          <tr>
            <th>Namn</th>
            <th>Titel</th>
            <th>Kund</th>
            <th>E-post</th>
            <th>Telefon</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {contacts.map((c) => (
            <tr key={c.id}>
              <td>
                {c.firstName} {c.lastName}
              </td>
              <td>{c.jobTitle}</td>
              <td>{c.customerName}</td>
              <td>{c.email}</td>
              <td>{c.phone}</td>
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
          {contacts.length === 0 && (
            <tr>
              <td colSpan={6} className="muted">
                Inga kontakter hittades.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </section>
  )
}
