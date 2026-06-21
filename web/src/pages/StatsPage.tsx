import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { CheckInStatsResponse } from '../api/types';

export default function StatsPage() {
  const { id } = useParams<{ id: string }>();
  const [stats, setStats] = useState<CheckInStatsResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  function load() {
    if (!id) return;
    api
      .getStats(id)
      .then(setStats)
      .catch((e: ApiError) => setError(e.message));
  }

  useEffect(load, [id]);

  if (error) return <div className="alert error">{error}</div>;
  if (!stats) return <p className="muted">Carregando…</p>;

  const pct = stats.totalRegistered > 0 ? Math.round((stats.checkedIn / stats.totalRegistered) * 100) : 0;

  return (
    <section className="narrow">
      <Link to={`/events/${id}`} className="back">
        ← Evento
      </Link>
      <h1>Estatísticas de presença</h1>

      <div className="stats-grid">
        <div className="stat">
          <span className="stat-value">{stats.totalRegistered}</span>
          <span className="stat-label">Inscritos</span>
        </div>
        <div className="stat">
          <span className="stat-value">{stats.checkedIn}</span>
          <span className="stat-label">Presentes</span>
        </div>
        <div className="stat">
          <span className="stat-value">{stats.pending}</span>
          <span className="stat-label">Pendentes</span>
        </div>
        <div className="stat">
          <span className="stat-value">{stats.capacity}</span>
          <span className="stat-label">Lotação</span>
        </div>
      </div>

      <div className="progress" aria-label={`${pct}% presentes`}>
        <div className="progress-bar" style={{ width: `${pct}%` }} />
      </div>
      <p className="muted">{pct}% dos inscritos já fizeram check-in.</p>

      <button className="btn" onClick={load}>
        Atualizar
      </button>
    </section>
  );
}
