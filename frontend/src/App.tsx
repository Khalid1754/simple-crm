import { useState } from 'react'
import { Navigate, NavLink, Route, Routes } from 'react-router-dom'
import { getToken, setToken } from './api'
import LoginPage from './pages/LoginPage'
import CustomersPage from './pages/CustomersPage'
import ContactsPage from './pages/ContactsPage'
import DealsPage from './pages/DealsPage'

export default function App() {
  // Är vi inloggade? Vi kollar om det finns en sparad token.
  const [loggedIn, setLoggedIn] = useState(getToken() !== null)

  function handleLogout() {
    setToken(null)
    setLoggedIn(false)
  }

  // Inte inloggad: visa bara inloggningssidan.
  if (!loggedIn) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage onLogin={() => setLoggedIn(true)} />} />
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    )
  }

  return (
    <div className="layout">
      <header className="topbar">
        <span className="logo">Simple CRM</span>
        <nav>
          <NavLink to="/customers">Kunder</NavLink>
          <NavLink to="/contacts">Kontakter</NavLink>
          <NavLink to="/deals">Affärer</NavLink>
        </nav>
        <button className="secondary" onClick={handleLogout}>
          Logga ut
        </button>
      </header>

      <main>
        <Routes>
          <Route path="/customers" element={<CustomersPage />} />
          <Route path="/contacts" element={<ContactsPage />} />
          <Route path="/deals" element={<DealsPage />} />
          {/* Allt annat (t.ex. "/" eller "/login") skickas till kundlistan. */}
          <Route path="*" element={<Navigate to="/customers" replace />} />
        </Routes>
      </main>
    </div>
  )
}
