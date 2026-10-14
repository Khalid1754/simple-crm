import { useState, type FormEvent } from 'react'
import { login, register, setToken } from '../api'

interface Props {
  onLogin: () => void
}

export default function LoginPage({ onLogin }: Props) {
  // Förifyllt med demo-användaren som skapas av backenden (SeedData.cs).
  const [email, setEmail] = useState('demo@crm.se')
  const [password, setPassword] = useState('Demo123!')
  const [isRegister, setIsRegister] = useState(false)
  const [error, setError] = useState('')

  async function handleSubmit(e: FormEvent) {
    e.preventDefault() // Stoppa webbläsarens vanliga formulär-skick (som laddar om sidan).
    setError('')
    try {
      const result = isRegister ? await register(email, password) : await login(email, password)
      setToken(result.token)
      onLogin()
    } catch (err) {
      setError((err as Error).message)
    }
  }

  return (
    <div className="login">
      <form className="card" onSubmit={handleSubmit}>
        <h1>Simple CRM</h1>
        <p className="muted">{isRegister ? 'Skapa ett konto' : 'Logga in för att fortsätta'}</p>

        <label>
          E-post
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>
          Lösenord
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </label>

        {error && <p className="error">{error}</p>}

        <button type="submit">{isRegister ? 'Registrera' : 'Logga in'}</button>
        <button type="button" className="link" onClick={() => setIsRegister(!isRegister)}>
          {isRegister ? 'Har du redan ett konto? Logga in' : 'Inget konto? Registrera dig'}
        </button>
      </form>
    </div>
  )
}
