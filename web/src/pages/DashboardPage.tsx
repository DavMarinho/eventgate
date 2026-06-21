import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import {
  ArcElement,
  BarElement,
  CategoryScale,
  Chart as ChartJS,
  Legend,
  LinearScale,
  Tooltip,
} from 'chart.js';
import { Bar, Doughnut } from 'react-chartjs-2';
import { api, ApiError } from '../api/client';
import type { CheckInStatsResponse, CourseStat, SemesterStat, SessionStat } from '../api/types';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Tooltip, Legend);

const DONUT_COLORS = ['#378ADD', '#1D9E75', '#D4537E', '#BA7517', '#D85A30', '#888780', '#7F77DD', '#639922'];

export default function DashboardPage() {
  const { id } = useParams<{ id: string }>();
  const [stats, setStats] = useState<CheckInStatsResponse | null>(null);
  const [byCourse, setByCourse] = useState<CourseStat[]>([]);
  const [bySemester, setBySemester] = useState<SemesterStat[]>([]);
  const [bySession, setBySession] = useState<SessionStat[]>([]);
  const [error, setError] = useState<string | null>(null);

  function load() {
    if (!id) return;
    setError(null);
    Promise.all([
      api.getStats(id),
      api.dashboardByCourse(id),
      api.dashboardBySemester(id),
      api.dashboardBySession(id),
    ])
      .then(([s, c, sem, sess]) => {
        setStats(s);
        setByCourse(c);
        setBySemester(sem);
        setBySession(sess);
      })
      .catch((e: ApiError) => setError(e.message));
  }

  useEffect(load, [id]);

  if (error) return <div className="alert error">{error}</div>;
  if (!stats) return <p className="muted">Carregando…</p>;

  const rate = stats.totalRegistered > 0 ? Math.round((stats.checkedIn / stats.totalRegistered) * 100) : 0;
  const topCourses = byCourse.slice(0, 10);

  return (
    <section>
      <Link to={`/events/${id}`} className="back">
        ← Evento
      </Link>
      <h1>Dashboard</h1>

      <div className="stats-grid">
        <Stat value={stats.totalRegistered} label="Inscritos" />
        <Stat value={stats.checkedIn} label="Presentes" />
        <Stat value={stats.pending} label="Pendentes" />
        <Stat value={`${rate}%`} label="Taxa presença" />
      </div>

      <div className="charts-grid">
        <div className="card">
          <h3>Inscrições por curso</h3>
          {topCourses.length === 0 ? (
            <p className="muted">Sem dados.</p>
          ) : (
            <Bar
              data={{
                labels: topCourses.map((c) => c.course),
                datasets: [{ label: 'Inscritos', data: topCourses.map((c) => c.registered), backgroundColor: '#378ADD', borderRadius: 4 }],
              }}
              options={{
                responsive: true,
                indexAxis: 'y',
                plugins: { legend: { display: false } },
                scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
              }}
            />
          )}
        </div>

        <div className="card">
          <h3>Por semestre</h3>
          {bySemester.length === 0 ? (
            <p className="muted">Sem dados.</p>
          ) : (
            <Doughnut
              data={{
                labels: bySemester.map((s) => (s.label === 'Sem semestre' ? s.label : `${s.label}º`)),
                datasets: [
                  {
                    data: bySemester.map((s) => s.registered),
                    backgroundColor: bySemester.map((_, i) => DONUT_COLORS[i % DONUT_COLORS.length]),
                    borderWidth: 0,
                  },
                ],
              }}
              options={{ responsive: true, plugins: { legend: { position: 'bottom' } } }}
            />
          )}
        </div>
      </div>

      {bySession.length > 0 && (
        <div className="card">
          <h3>Presença por palestra</h3>
          <Bar
            data={{
              labels: bySession.map((s) => s.title),
              datasets: [
                { label: 'Presentes', data: bySession.map((s) => s.attendance), backgroundColor: '#1D9E75', borderRadius: 4 },
              ],
            }}
            options={{
              responsive: true,
              indexAxis: 'y',
              plugins: { legend: { display: false } },
              scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
            }}
          />
        </div>
      )}

      <button className="btn" onClick={load}>
        Atualizar
      </button>
    </section>
  );
}

function Stat({ value, label }: { value: number | string; label: string }) {
  return (
    <div className="stat">
      <span className="stat-value">{value}</span>
      <span className="stat-label">{label}</span>
    </div>
  );
}
