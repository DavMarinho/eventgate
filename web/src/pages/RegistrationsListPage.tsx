import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { RegistrationListItem } from '../api/types';

export default function RegistrationsListPage() {
  const { id } = useParams<{ id: string }>();
  const [items, setItems] = useState<RegistrationListItem[]>([]);
  const [search, setSearch] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  function load(term?: string) {
    if (!id) return;
    setLoading(true);
    setError(null);
    api
      .listRegistrations(id, term)
      .then(setItems)
      .catch((e: ApiError) => setError(e.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => load(), [id]);

  return (
    <section>
      <Link to={`/events/${id}`} className="back">
        ← Evento
      </Link>
      <h1>Inscritos</h1>

      <form
        className="search-bar"
        onSubmit={(e) => {
          e.preventDefault();
          load(search);
        }}
      >
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Buscar por nome ou e-mail…"
        />
        <button type="submit" className="btn">
          Buscar
        </button>
      </form>

      {error && <div className="alert error">{error}</div>}
      {loading ? (
        <p className="muted">Carregando…</p>
      ) : items.length === 0 ? (
        <p className="muted">Nenhum inscrito encontrado.</p>
      ) : (
        <table className="data-table">
          <thead>
            <tr>
              <th>Nome</th>
              <th>E-mail</th>
              <th>Curso</th>
              <th>Sem.</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {items.map((r) => (
              <tr key={r.id}>
                <td>{r.participantName}</td>
                <td>{r.participantEmail}</td>
                <td>{r.course}</td>
                <td>{r.semester ?? '—'}</td>
                <td>
                  <span className={`badge ${r.status === 'CheckedIn' ? 'badge-ok' : 'badge-danger'}`}>
                    {r.status === 'CheckedIn' ? 'Presente' : r.status === 'Cancelled' ? 'Cancelado' : 'Inscrito'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      <p className="muted">{items.length} registro(s).</p>
    </section>
  );
}
