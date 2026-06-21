import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { SessionResponse } from '../api/types';
import Field from '../components/Field';

export default function SessionsAdminPage() {
  const { id } = useParams<{ id: string }>();
  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [title, setTitle] = useState('');
  const [speaker, setSpeaker] = useState('');
  const [room, setRoom] = useState('');
  const [startsAt, setStartsAt] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  function load() {
    if (!id) return;
    api.listSessions(id).then(setSessions).catch((e: ApiError) => setError(e.message));
  }

  useEffect(load, [id]);

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!id) return;
    setError(null);
    setBusy(true);
    try {
      await api.createSession(id, {
        title,
        speaker: speaker || undefined,
        room: room || undefined,
        startsAt: new Date(startsAt).toISOString(),
      });
      setTitle('');
      setSpeaker('');
      setRoom('');
      setStartsAt('');
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  async function handleDelete(sessionId: string) {
    if (!confirm('Remover esta palestra?')) return;
    try {
      await api.deleteSession(sessionId);
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    }
  }

  return (
    <section className="narrow">
      <Link to="/equipe" className="back">
        ← Equipe
      </Link>
      <h1>Palestras</h1>
      <p className="muted">Aparecem na programação da Home e viram pontos de presença na portaria.</p>

      <form className="card" onSubmit={handleAdd}>
        <h2>Adicionar palestra</h2>
        <Field label="Título">
          <input required minLength={2} value={title} onChange={(e) => setTitle(e.target.value)} />
        </Field>
        <Field label="Palestrante">
          <input value={speaker} onChange={(e) => setSpeaker(e.target.value)} />
        </Field>
        <Field label="Sala / local">
          <input value={room} onChange={(e) => setRoom(e.target.value)} placeholder="Ex.: Auditório 1" />
        </Field>
        <Field label="Início">
          <input required type="datetime-local" value={startsAt} onChange={(e) => setStartsAt(e.target.value)} />
        </Field>
        {error && <div className="alert error">{error}</div>}
        <button type="submit" className="btn primary" disabled={busy}>
          {busy ? 'Salvando…' : 'Adicionar'}
        </button>
      </form>

      {sessions.length === 0 ? (
        <p className="muted">Nenhuma palestra ainda.</p>
      ) : (
        <div className="portal-events">
          {sessions.map((s) => (
            <div key={s.id} className="card portal-event">
              <div>
                <p className="portal-event-name">{s.title}</p>
                <p className="muted">
                  {new Date(s.startsAt).toLocaleString('pt-BR')}
                  {s.room ? ` · ${s.room}` : ''}
                  {s.speaker ? ` · ${s.speaker}` : ''}
                </p>
              </div>
              <button className="btn danger" onClick={() => handleDelete(s.id)}>
                Remover
              </button>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
