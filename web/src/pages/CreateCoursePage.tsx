import { useState } from 'react';
import { api, ApiError } from '../api/client';
import Field from '../components/Field';

export default function CreateCoursePage() {
  const [name, setName] = useState('');
  const [code, setCode] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [ok, setOk] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setOk(null);
    setBusy(true);
    try {
      const course = await api.createCourse(name.trim(), code.trim() || undefined);
      setOk(`Curso "${course.name}" criado.`);
      setName('');
      setCode('');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="narrow">
      <h1>Novo curso</h1>
      <p className="muted">Adiciona um curso à lista usada no autocomplete da inscrição.</p>
      <form className="card" onSubmit={handleSubmit}>
        <Field label="Nome">
          <input required minLength={2} value={name} onChange={(e) => setName(e.target.value)} />
        </Field>
        <Field label="Código (opcional)">
          <input value={code} onChange={(e) => setCode(e.target.value)} placeholder="Ex.: ECA, POLI…" />
        </Field>
        {error && <div className="alert error">{error}</div>}
        {ok && <div className="alert ok">{ok}</div>}
        <button type="submit" className="btn primary" disabled={busy}>
          {busy ? 'Criando…' : 'Criar curso'}
        </button>
      </form>
    </section>
  );
}
