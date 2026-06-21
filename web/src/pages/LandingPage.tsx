import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import type { EventResponse } from '../api/types';

// Conteúdo de divulgação — placeholder editável. Troque por dados reais do evento.
// Cada palestrante aceita `photo` (URL); sem foto, mostra as iniciais.
interface Speaker {
  name: string;
  role: string;
  talk: string;
  photo?: string;
}

const SPEAKERS: Speaker[] = [
  { name: 'Dra. Helena Costa', role: 'USP · Computação', talk: 'IA generativa na prática' },
  { name: 'Rafael Andrade', role: 'CTO, TechBR', talk: 'Escalando sistemas para milhões' },
  { name: 'Profa. Lia Moreira', role: 'USP · Engenharia', talk: 'Robótica e automação' },
  { name: 'Marina Tavares', role: 'Eng. de Dados, DataLab', talk: 'Dados que viram decisão' },
  { name: 'Caio Fernandes', role: 'Founder, StartUSP', talk: 'Do TCC à startup' },
  { name: 'Dr. Paulo Reis', role: 'USP · Segurança', talk: 'Cibersegurança hoje' },
];

const AGENDA = [
  { time: '09:00', title: 'Credenciamento e abertura' },
  { time: '10:00', title: 'Keynote: o futuro da tecnologia' },
  { time: '13:30', title: 'Trilhas: IA, Dados, Segurança' },
  { time: '16:00', title: 'Painel com a indústria' },
  { time: '18:00', title: 'Networking e encerramento' },
];

const AVATAR_COLORS = ['#5b8cff', '#2fbf71', '#d4537e', '#ba7517', '#7f77dd', '#1d9e75'];

function initials(name: string): string {
  return name
    .replace(/^(Dra?\.?|Profa?\.?)\s+/i, '')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase())
    .join('');
}

export default function LandingPage() {
  const [featured, setFeatured] = useState<EventResponse | null>(null);
  const [speakers, setSpeakers] = useState<Speaker[]>(SPEAKERS);
  const [agenda, setAgenda] = useState(AGENDA);

  useEffect(() => {
    api
      .listEvents()
      .then((evs) => {
        const f = evs[0] ?? null;
        setFeatured(f);
        if (f) {
          // Palestrantes reais do evento; se não houver, mantém os de exemplo.
          api
            .listSpeakers(f.id)
            .then((list) => {
              if (list.length) {
                setSpeakers(
                  list.map((s) => ({
                    name: s.name,
                    role: s.role ?? '',
                    talk: s.talk ?? '',
                    photo: s.photoUrl ?? undefined,
                  })),
                );
              }
            })
            .catch(() => undefined);

          // Programação real (palestras cadastradas); senão, mantém o exemplo.
          api
            .listSessions(f.id)
            .then((list) => {
              if (list.length) {
                setAgenda(
                  list.map((s) => ({
                    time: new Date(s.startsAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
                    title: s.speaker ? `${s.title} — ${s.speaker}` : s.title,
                  })),
                );
              }
            })
            .catch(() => undefined);
        }
      })
      .catch(() => setFeatured(null));
  }, []);

  const ctaTo = featured ? `/events/${featured.id}` : '/eventos';

  return (
    <div className="landing">
      <section className="hero">
        <div className="hero-inner">
          <span className="hero-kicker">Evento aberto ao público · USP</span>
          <h1 className="hero-title">{featured?.name ?? 'Semana de Tecnologia USP'}</h1>
          <p className="hero-sub">
            Três dias de palestras, workshops e networking com quem constrói o futuro da tecnologia.
            Entrada gratuita mediante inscrição.
          </p>
          <div className="hero-chips">
            <span className="chip">
              🗓️ {featured ? new Date(featured.startsAt).toLocaleDateString('pt-BR') : 'Em breve'}
            </span>
            <span className="chip">📍 {featured?.location ?? 'Cidade Universitária — USP'}</span>
            <span className="chip">🎟️ Gratuito</span>
          </div>
          <div className="hero-cta">
            <Link to={ctaTo} className="btn primary big">
              Inscreva-se agora
            </Link>
            <Link to="/eventos" className="btn big">
              Ver eventos
            </Link>
          </div>
        </div>
      </section>

      <section className="landing-section">
        <h2>Sobre o evento</h2>
        <p className="lead">
          Reunimos estudantes, pesquisadores e profissionais para discutir as tendências que estão
          transformando a tecnologia. Palestras de especialistas, trilhas temáticas e espaço para
          troca de ideias — tudo num só lugar, dentro da USP.
        </p>
        <div className="highlights">
          <div className="highlight">
            <span className="highlight-num">+50</span>
            <span className="highlight-label">palestrantes</span>
          </div>
          <div className="highlight">
            <span className="highlight-num">3</span>
            <span className="highlight-label">dias</span>
          </div>
          <div className="highlight">
            <span className="highlight-num">100%</span>
            <span className="highlight-label">gratuito</span>
          </div>
        </div>
      </section>

      <section className="landing-section">
        <h2>Palestrantes</h2>
        <div className="speaker-grid">
          {speakers.map((s, i) => (
            <div key={s.name} className="speaker-card">
              {s.photo ? (
                <img className="speaker-photo" src={s.photo} alt={s.name} />
              ) : (
                <div className="speaker-photo initials" style={{ background: AVATAR_COLORS[i % AVATAR_COLORS.length] }}>
                  {initials(s.name)}
                </div>
              )}
              <p className="speaker-name">{s.name}</p>
              <p className="speaker-role">{s.role}</p>
              <p className="speaker-talk">“{s.talk}”</p>
            </div>
          ))}
        </div>
      </section>

      <section className="landing-section">
        <h2>Programação</h2>
        <ul className="agenda">
          {agenda.map((a) => (
            <li key={a.time} className="agenda-item">
              <span className="agenda-time">{a.time}</span>
              <span className="agenda-title">{a.title}</span>
            </li>
          ))}
        </ul>
      </section>

      <section className="cta-band">
        <h2>Garanta sua vaga</h2>
        <p>As inscrições são gratuitas e limitadas à lotação do evento.</p>
        <Link to={ctaTo} className="btn primary big">
          Quero participar
        </Link>
      </section>
    </div>
  );
}
