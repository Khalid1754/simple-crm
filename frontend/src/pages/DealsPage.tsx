import { useEffect, useState, type FormEvent } from 'react'
import { createDeal, deleteDeal, getCustomers, getDeals, updateDeal } from '../api'
import { dealStageLabels, dealStages, type Customer, type Deal, type DealStage } from '../types'
import { formatSek } from '../utils'

const emptyForm = {
  title: '',
  amount: '',
  stage: 'Lead' as DealStage,
  expectedCloseDate: '',
  customerId: '',
}

export default function DealsPage() {
  const [deals, setDeals] = useState<Deal[]>([])
  const [customers, setCustomers] = useState<Customer[]>([])
  const [search, setSearch] = useState('')
  const [stageFilter, setStageFilter] = useState<DealStage | ''>('')
  const [error, setError] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [form, setForm] = useState(emptyForm)

  useEffect(() => {
    getCustomers()
      .then(setCustomers)
      .catch((err) => setError(err.message))
  }, [])

  useEffect(() => {
    let ignore = false
    getDeals(search, stageFilter)
      .then((data) => {
        if (!ignore) setDeals(data)
      })
      .catch((err) => setError(err.message))
    return () => {
      ignore = true
    }
  }, [search, stageFilter])

  async function reload() {
    setDeals(await getDeals(search, stageFilter))
  }

  function startCreate() {
    setForm(emptyForm)
    setEditingId(0)
  }

  function startEdit(d: Deal) {
    setForm({
      title: d.title,
      amount: String(d.amount),
      stage: d.stage,
      expectedCloseDate: d.expectedCloseDate ?? '',
      customerId: String(d.customerId),
    })
    setEditingId(d.id)
  }

  async function handleSave(e: FormEvent) {
    e.preventDefault()
    setError('')
    const data = {
      title: form.title.trim(),
      amount: Number(form.amount),
      stage: form.stage,
      // <input type="date"> ger "2026-12-31", vilket backenden förstår som DateOnly.
      expectedCloseDate: form.expectedCloseDate || null,
      customerId: Number(form.customerId),
    }
    try {
      if (editingId) {
        await updateDeal(editingId, data)
      } else {
        await createDeal(data)
      }
      setEditingId(null)
      await reload()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  async function handleDelete(d: Deal) {
    if (!confirm(`Ta bort affären "${d.title}"?`)) return
    try {
      await deleteDeal(d.id)
      await reload()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  // Summera värdet på affärerna som visas just nu.
  const total = deals.reduce((sum, d) => sum + d.amount, 0)

  return (
    <section>
      <div className="page-header">
        <h2>Affärer</h2>
        <button onClick={startCreate}>+ Ny affär</button>
      </div>

      {error && <p className="error">{error}</p>}

      {editingId !== null && (
        <form className="card form-grid" onSubmit={handleSave}>
          <h3>{editingId ? 'Redigera affär' : 'Ny affär'}</h3>
          <label>
            Titel *
            <input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} required />
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
            Belopp (kr) *
            <input
              type="number"
              min="0"
              value={form.amount}
              onChange={(e) => setForm({ ...form, amount: e.target.value })}
              required
            />
          </label>
          <label>
            Steg
            <select value={form.stage} onChange={(e) => setForm({ ...form, stage: e.target.value as DealStage })}>
              {dealStages.map((s) => (
                <option key={s} value={s}>
                  {dealStageLabels[s]}
                </option>
              ))}
            </select>
          </label>
          <label>
            Förväntat avslut
            <input
              type="date"
              value={form.expectedCloseDate}
              onChange={(e) => setForm({ ...form, expectedCloseDate: e.target.value })}
            />
          </label>
          <div className="form-actions">
            <button type="submit">Spara</button>
            <button type="button" className="secondary" onClick={() => setEditingId(null)}>
              Avbryt
            </button>
          </div>
        </form>
      )}

      <div className="filters">
        <input
          className="search"
          placeholder="Sök på titel eller kund…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select value={stageFilter} onChange={(e) => setStageFilter(e.target.value as DealStage | '')}>
          <option value="">Alla steg</option>
          {dealStages.map((s) => (
            <option key={s} value={s}>
              {dealStageLabels[s]}
            </option>
          ))}
        </select>
      </div>

      <table>
        <thead>
          <tr>
            <th>Titel</th>
            <th>Kund</th>
            <th>Steg</th>
            <th className="number">Belopp</th>
            <th>Förväntat avslut</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {deals.map((d) => (
            <tr key={d.id}>
              <td>{d.title}</td>
              <td>{d.customerName}</td>
              <td>
                <span className={`badge stage-${d.stage.toLowerCase()}`}>{dealStageLabels[d.stage]}</span>
              </td>
              <td className="number">{formatSek(d.amount)}</td>
              <td>{d.expectedCloseDate}</td>
              <td className="row-actions">
                <button className="secondary" onClick={() => startEdit(d)}>
                  Redigera
                </button>
                <button className="danger" onClick={() => handleDelete(d)}>
                  Ta bort
                </button>
              </td>
            </tr>
          ))}
          {deals.length === 0 && (
            <tr>
              <td colSpan={6} className="muted">
                Inga affärer hittades.
              </td>
            </tr>
          )}
        </tbody>
        {deals.length > 0 && (
          <tfoot>
            <tr>
              <td colSpan={3}>Totalt</td>
              <td className="number">{formatSek(total)}</td>
              <td colSpan={2}></td>
            </tr>
          </tfoot>
        )}
      </table>
    </section>
  )
}
