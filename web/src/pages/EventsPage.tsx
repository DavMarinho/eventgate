import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { EventResponse } from '../api/types';

export default function EventsPage() {
  const [events, setEvents] = useState<EventResponse[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .listEvents()
      .then(setEvents)
      .catch((e: ApiError) => setError(e.message));
  }, []);

  return (
    <section>
      <h1>Eventos</h1>
      <p className="muted">Escolha um evento para se inscrever e receber seu código de acesso.</p>

      {error && <div className="alert error">{error}</div>}
      {!events && !error && <p className="muted">Carregando…</p>}
      {events && events.length === 0 && <p className="muted">Nenhum evento cadastrado ainda.</p>}

      <div className="grid">
        {events?.map((ev) => {
          const full = ev.registeredCount >= ev.capacity;
          return (
            <Link to={`/events/${ev.id}`} key={ev.id} className="card event-card">
              <h3>{ev.name}</h3>
              {ev.location && <p className="muted">📍 {ev.location}</p>}
              <p className="muted">🗓️ {new Date(ev.startsAt).toLocaleString('pt-BR')}</p>
              <div className="capacity">
                <span className={`badge ${full ? 'badge-danger' : 'badge-ok'}`}>
                  {full ? 'Lotado' : `${ev.capacity - ev.registeredCount} vagas`}
                </span>
                <span className="muted">
                  {ev.registeredCount}/{ev.capacity}
                </span>
              </div>
            </Link>
          );
        })}
      </div>
    </section>
  );
}
