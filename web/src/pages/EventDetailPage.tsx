import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import QRCode from 'qrcode';
import { api, ApiError } from '../api/client';
import type { CourseResponse, EventResponse, RegistrationResponse } from '../api/types';
import Field from '../components/Field';
import { useAuth } from '../auth/AuthContext';

const OUTRO = '__outro__';

export default function EventDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { isAuthenticated } = useAuth();

  const [event, setEvent] = useState<EventResponse | null>(null);
  const [courses, setCourses] = useState<CourseResponse[]>([]);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [birthDate, setBirthDate] = useState('');
  const [courseSel, setCourseSel] = useState('');
  const [courseOther, setCourseOther] = useState('');
  const [semester, setSemester] = useState('');
  const [photo, setPhoto] = useState<File | null>(null);
  const [photoPreview, setPhotoPreview] = useState<string | null>(null);
  const [consent, setConsent] = useState(false);

  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [result, setResult] = useState<RegistrationResponse | null>(null);
  const [qrDataUrl, setQrDataUrl] = useState<string | null>(null);

  const isOutro = courseSel === OUTRO;

  useEffect(() => {
    if (!id) return;
    api.getEvent(id).then(setEvent).catch((e: ApiError) => setLoadError(e.message));
    api.listCourses().then(setCourses).catch(() => setCourses([]));
  }, [id]);

  useEffect(() => {
    if (!photo) {
      setPhotoPreview(null);
      return;
    }
    const url = URL.createObjectURL(photo);
    setPhotoPreview(url);
    return () => URL.revokeObjectURL(url);
  }, [photo]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!id) return;
    setFormError(null);

    if (!courseSel) return setFormError('Selecione um curso.');
    if (isOutro && !courseOther.trim()) return setFormError('Informe o curso em "Outro".');
    if (!isOutro && !semester) return setFormError('Informe o semestre.');
    if (!photo) return setFormError('Envie uma foto.');

    const fd = new FormData();
    fd.append('participantName', name);
    fd.append('participantEmail', email);
    fd.append('birthDate', birthDate);
    fd.append('consentAccepted', String(consent));
    fd.append('photo', photo);
    if (isOutro) {
      fd.append('courseOther', courseOther.trim());
    } else {
      fd.append('courseId', courseSel);
      fd.append('semester', semester);
    }

    setSubmitting(true);
    try {
      const reg = await api.register(id, fd);
      const qr = await QRCode.toDataURL(reg.accessCode, { width: 240, margin: 2 });
      setQrDataUrl(qr);
      setResult(reg);
    } catch (err) {
      setFormError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setSubmitting(false);
    }
  }

  if (loadError) return <div className="alert error">{loadError}</div>;
  if (!event) return <p className="muted">Carregando…</p>;

  if (result) {
    return (
      <section className="card success-card">
        <h1>Inscrição confirmada! 🎉</h1>
        <p>{result.participantName}, apresente este QR Code (ou o código) na portaria.</p>
        {qrDataUrl && <img className="qr-img" src={qrDataUrl} alt="QR Code do código de acesso" />}
        <div className="code-box" aria-label="Código de acesso">
          {result.accessCode}
        </div>
        {qrDataUrl && (
          <a className="btn primary" href={qrDataUrl} download={`eventgate-${result.accessCode}.png`}>
            Baixar QR Code
          </a>
        )}
        <p className="muted">
          Enviamos também por e-mail. Pode consultar/excluir seus dados em{' '}
          <Link to="/my-data">Meus dados (LGPD)</Link>.
        </p>
      </section>
    );
  }

  const full = event.registeredCount >= event.capacity;

  return (
    <section className="detail">
      <div>
        <Link to="/eventos" className="back">
          ← Eventos
        </Link>
        <h1>{event.name}</h1>
        {event.description && <p>{event.description}</p>}
        <ul className="meta">
          {event.location && <li>📍 {event.location}</li>}
          <li>🗓️ {new Date(event.startsAt).toLocaleString('pt-BR')}</li>
          <li>
            👥 {event.registeredCount}/{event.capacity} inscritos
          </li>
        </ul>
        {isAuthenticated && (
          <div className="staff-links">
            <Link to={`/events/${event.id}/dashboard`}>📊 Dashboard</Link>
            <Link to={`/events/${event.id}/registrations`}>📋 Inscritos</Link>
            <Link to={`/events/${event.id}/stats`}>✔️ Presença</Link>
          </div>
        )}
      </div>

      <form className="card" onSubmit={handleSubmit}>
        <h2>Inscrição</h2>
        {full ? (
          <div className="alert error">Evento lotado. Não há vagas disponíveis.</div>
        ) : (
          <>
            <Field label="Nome completo">
              <input required minLength={2} value={name} onChange={(e) => setName(e.target.value)} />
            </Field>
            <Field label="E-mail">
              <input required type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
            </Field>
            <Field label="Data de nascimento">
              <input required type="date" value={birthDate} onChange={(e) => setBirthDate(e.target.value)} />
            </Field>
            <Field label="Curso">
              <select required value={courseSel} onChange={(e) => setCourseSel(e.target.value)}>
                <option value="">Selecione…</option>
                {courses.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
                <option value={OUTRO}>Outro / não listado</option>
              </select>
            </Field>
            {isOutro ? (
              <Field label="Qual curso / instituição?">
                <input value={courseOther} onChange={(e) => setCourseOther(e.target.value)} placeholder="Ex.: Engenharia — UNICAMP" />
              </Field>
            ) : (
              courseSel && (
                <Field label="Semestre">
                  <input type="number" min={1} max={12} value={semester} onChange={(e) => setSemester(e.target.value)} />
                </Field>
              )
            )}
            <Field label="Foto" hint="JPG ou PNG, até 3 MB. Usada só para conferência na portaria.">
              <input
                required
                type="file"
                accept="image/png,image/jpeg"
                onChange={(e) => setPhoto(e.target.files?.[0] ?? null)}
              />
            </Field>
            {photoPreview && <img className="photo-preview" src={photoPreview} alt="Prévia da foto" />}
            <label className="consent">
              <input type="checkbox" checked={consent} onChange={(e) => setConsent(e.target.checked)} />
              <span>
                Aceito o tratamento dos meus dados (nome, e-mail, foto, nascimento e curso) para
                participar deste evento, conforme a LGPD.
              </span>
            </label>
            {formError && <div className="alert error">{formError}</div>}
            <button type="submit" className="btn primary" disabled={submitting || !consent}>
              {submitting ? 'Enviando…' : 'Inscrever-me'}
            </button>
          </>
        )}
      </form>
    </section>
  );
}
