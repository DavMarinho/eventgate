import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, ApiError } from '../api/client';
import type { EventResponse } from '../api/types';
import { useAuth } from '../auth/AuthContext';

export default function StaffPortalPage() {
  const { role } = useAuth();
  const isOrganizer = role === 'Organizer';
  const [events, setEvents] = useState<EventResponse[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.listEvents().then(setEvents).catch((e: ApiError) => setError(e.message));
  }, []);

  return (
    <section>
      <h1>Área da equipe</h1>
      <p className="muted">
        Acesso restrito. Você está como <strong>{role}</strong>.
        {role === 'Validator' && ' Validadores só fazem a portaria.'}
      </p>

      <div className="portal-grid">
        <Link to="/gate" className="portal-card">
          <span className="portal-icon">🛂</span>
          <span className="portal-title">Portaria</span>
          <span className="portal-desc">Ler QR / validar entradas</span>
        </Link>
        {isOrganizer && (
          <>
            <Link to="/events/new" className="portal-card">
              <span className="portal-icon">➕</span>
              <span className="portal-title">Novo evento</span>
              <span className="portal-desc">Criar um evento</span>
            </Link>
            <Link to="/courses/new" className="portal-card">
              <span className="portal-icon">🎓</span>
              <span className="portal-title">Cursos</span>
              <span className="portal-desc">Adicionar curso à lista</span>
            </Link>
            <Link to="/staff/new" className="portal-card">
              <span className="portal-icon">👥</span>
              <span className="portal-title">Adicionar equipe</span>
              <span className="portal-desc">Criar conta de organizador/validador</span>
            </Link>
          </>
        )}
      </div>

      <h2 style={{ marginTop: '2rem' }}>Eventos</h2>
      {error && <div className="alert error">{error}</div>}
      {events.length === 0 ? (
        <p className="muted">Nenhum evento ainda.</p>
      ) : (
        <div className="portal-events">
          {events.map((ev) => (
            <div key={ev.id} className="card portal-event">
              <div>
                <p className="portal-event-name">{ev.name}</p>
                <p className="muted">
                  {new Date(ev.startsAt).toLocaleDateString('pt-BR')} · {ev.registeredCount}/{ev.capacity} inscritos
                </p>
              </div>
              <div className="row-buttons">
                <Link to={`/events/${ev.id}/dashboard`} className="btn">
                  📊 Dashboard
                </Link>
                <Link to={`/events/${ev.id}/registrations`} className="btn">
                  📋 Inscritos
                </Link>
                <Link to={`/events/${ev.id}/stats`} className="btn">
                  ✔️ Presença
                </Link>
                {isOrganizer && (
                  <Link to={`/events/${ev.id}/speakers`} className="btn">
                    🎤 Palestrantes
                  </Link>
                )}
                {isOrganizer && (
                  <Link to={`/events/${ev.id}/sessions`} className="btn">
                    📅 Palestras
                  </Link>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
