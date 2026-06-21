import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import Field from '../components/Field';

export default function CreateEventPage() {
  const navigate = useNavigate();
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [location, setLocation] = useState('');
  const [startsAt, setStartsAt] = useState('');
  const [capacity, setCapacity] = useState(50);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const ev = await api.createEvent({
        name,
        description: description || undefined,
        location: location || undefined,
        // datetime-local não tem fuso; converte para ISO com o fuso local.
        startsAt: new Date(startsAt).toISOString(),
        capacity,
      });
      navigate(`/events/${ev.id}`);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="narrow">
      <h1>Novo evento</h1>
      <form className="card" onSubmit={handleSubmit}>
        <Field label="Nome">
          <input required minLength={3} value={name} onChange={(e) => setName(e.target.value)} />
        </Field>
        <Field label="Descrição">
          <textarea
            rows={3}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </Field>
        <Field label="Local">
          <input value={location} onChange={(e) => setLocation(e.target.value)} />
        </Field>
        <Field label="Data e hora" hint="Deve ser no futuro.">
          <input
            required
            type="datetime-local"
            value={startsAt}
            onChange={(e) => setStartsAt(e.target.value)}
          />
        </Field>
        <Field label="Lotação">
          <input
            required
            type="number"
            min={1}
            value={capacity}
            onChange={(e) => setCapacity(Number(e.target.value))}
          />
        </Field>
        {error && <div className="alert error">{error}</div>}
        <button type="submit" className="btn primary" disabled={busy}>
          {busy ? 'Criando…' : 'Criar evento'}
        </button>
      </form>
    </section>
  );
}
