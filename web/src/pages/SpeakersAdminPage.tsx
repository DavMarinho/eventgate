import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { SpeakerResponse } from '../api/types';
import Field from '../components/Field';

export default function SpeakersAdminPage() {
  const { id } = useParams<{ id: string }>();
  const [speakers, setSpeakers] = useState<SpeakerResponse[]>([]);
  const [name, setName] = useState('');
  const [role, setRole] = useState('');
  const [talk, setTalk] = useState('');
  const [photoUrl, setPhotoUrl] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  function load() {
    if (!id) return;
    api.listSpeakers(id).then(setSpeakers).catch((e: ApiError) => setError(e.message));
  }

  useEffect(load, [id]);

  async function handleAdd(e: React.FormEvent) {
    e.preventDefault();
    if (!id) return;
    setError(null);
    setBusy(true);
    try {
      await api.createSpeaker(id, {
        name,
        role: role || undefined,
        talk: talk || undefined,
        photoUrl: photoUrl || undefined,
        sortOrder: speakers.length,
      });
      setName('');
      setRole('');
      setTalk('');
      setPhotoUrl('');
      load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  async function handleDelete(speakerId: string) {
    if (!confirm('Remover este palestrante?')) return;
    try {
      await api.deleteSpeaker(speakerId);
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
      <h1>Palestrantes</h1>
      <p className="muted">Aparecem na página inicial (Home) do evento.</p>

      <form className="card" onSubmit={handleAdd}>
        <h2>Adicionar palestrante</h2>
        <Field label="Nome">
          <input required minLength={2} value={name} onChange={(e) => setName(e.target.value)} />
        </Field>
        <Field label="Cargo / instituição">
          <input value={role} onChange={(e) => setRole(e.target.value)} placeholder="Ex.: USP · Computação" />
        </Field>
        <Field label="Título da palestra">
          <input value={talk} onChange={(e) => setTalk(e.target.value)} />
        </Field>
        <Field label="URL da foto (opcional)" hint="Sem foto, mostra as iniciais.">
          <input value={photoUrl} onChange={(e) => setPhotoUrl(e.target.value)} placeholder="https://…" />
        </Field>
        {error && <div className="alert error">{error}</div>}
        <button type="submit" className="btn primary" disabled={busy}>
          {busy ? 'Salvando…' : 'Adicionar'}
        </button>
      </form>

      {speakers.length === 0 ? (
        <p className="muted">Nenhum palestrante ainda (a Home usa exemplos até você cadastrar).</p>
      ) : (
        <div className="portal-events">
          {speakers.map((s) => (
            <div key={s.id} className="card portal-event">
              <div>
                <p className="portal-event-name">{s.name}</p>
                <p className="muted">
                  {s.role}
                  {s.talk ? ` · “${s.talk}”` : ''}
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
