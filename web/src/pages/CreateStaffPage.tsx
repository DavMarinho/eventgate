import { useState } from 'react';
import { api, ApiError } from '../api/client';
import Field from '../components/Field';

export default function CreateStaffPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('Validator');
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setOk(null);
    setBusy(true);
    try {
      const staff = await api.registerStaff(email.trim(), password, role);
      setOk(`Conta criada: ${staff.email} (${staff.role}).`);
      setEmail('');
      setPassword('');
      setRole('Validator');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="narrow">
      <h1>Nova conta de equipe</h1>
      <p className="muted">Cria um login para a equipe. Validador só valida na portaria; organizador faz tudo.</p>
      <form className="card" onSubmit={handleSubmit}>
        <Field label="E-mail">
          <input required type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        </Field>
        <Field label="Senha" hint="Mínimo 8 caracteres.">
          <input required type="password" minLength={8} value={password} onChange={(e) => setPassword(e.target.value)} />
        </Field>
        <Field label="Perfil">
          <select value={role} onChange={(e) => setRole(e.target.value)}>
            <option value="Validator">Validador</option>
            <option value="Organizer">Organizador</option>
          </select>
        </Field>
        {error && <div className="alert error">{error}</div>}
        {ok && <div className="alert ok">{ok}</div>}
        <button type="submit" className="btn primary" disabled={busy}>
          {busy ? 'Criando…' : 'Criar conta'}
        </button>
      </form>
    </section>
  );
}
