import { useEffect, useRef, useState } from 'react';
import { BrowserMultiFormatReader, type IScannerControls } from '@zxing/browser';
import { api, ApiError } from '../api/client';
import type { EventResponse, GateLookupResponse, SessionResponse } from '../api/types';

const MAIN = ''; // ponto "entrada principal"

export default function GatePanelPage() {
  const [code, setCode] = useState('');
  const [scanning, setScanning] = useState(false);
  const [lookup, setLookup] = useState<GateLookupResponse | null>(null);
  const [result, setResult] = useState<{ ok: boolean; reason: string } | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  // Ponto de validação: evento + (entrada principal ou uma palestra)
  const [events, setEvents] = useState<EventResponse[]>([]);
  const [eventId, setEventId] = useState('');
  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [sessionId, setSessionId] = useState<string>(MAIN);

  const videoRef = useRef<HTMLVideoElement>(null);
  const controlsRef = useRef<IScannerControls | null>(null);

  useEffect(() => {
    api.listEvents().then((evs) => {
      setEvents(evs);
      if (evs[0]) setEventId(evs[0].id);
    }).catch(() => undefined);
  }, []);

  useEffect(() => {
    setSessionId(MAIN);
    setSessions([]);
    if (eventId) api.listSessions(eventId).then(setSessions).catch(() => undefined);
  }, [eventId]);

  useEffect(() => {
    if (!scanning) return;
    const reader = new BrowserMultiFormatReader();
    let active = true;
    reader
      .decodeFromVideoDevice(undefined, videoRef.current!, (res, _err, controls) => {
        controlsRef.current = controls;
        if (res && active) {
          active = false;
          controls.stop();
          setScanning(false);
          const text = res.getText();
          setCode(text);
          void doLookup(text);
        }
      })
      .catch(() => {
        setError('Não foi possível acessar a câmera.');
        setScanning(false);
      });
    return () => controlsRef.current?.stop();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [scanning]);

  async function doLookup(value: string) {
    const c = value.trim();
    if (!c) return;
    setError(null);
    setResult(null);
    setLookup(null);
    setBusy(true);
    try {
      setLookup(await api.lookup(c));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  async function handleConfirm() {
    setBusy(true);
    setError(null);
    try {
      if (sessionId === MAIN) {
        const r = await api.validateCode(code.trim());
        setResult({ ok: r.valid, reason: r.reason });
        if (r.valid && lookup) setLookup({ ...lookup, alreadyCheckedIn: true, status: 'CheckedIn' });
      } else {
        const r = await api.attendSession(sessionId, code.trim());
        setResult({ ok: r.success, reason: r.reason });
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  function reset() {
    setCode('');
    setLookup(null);
    setResult(null);
    setError(null);
  }

  const isMain = sessionId === MAIN;
  const confirmDisabled = busy || (isMain && lookup?.alreadyCheckedIn === true);

  return (
    <section className="narrow">
      <h1>Portaria</h1>
      <p className="muted">Escolha onde você está validando, depois escaneie ou digite o código.</p>

      <div className="card">
        <div className="row-buttons" style={{ gap: 12 }}>
          <label className="field" style={{ flex: 1, marginBottom: 0 }}>
            <span className="field-label">Evento</span>
            <select value={eventId} onChange={(e) => setEventId(e.target.value)}>
              {events.map((ev) => (
                <option key={ev.id} value={ev.id}>
                  {ev.name}
                </option>
              ))}
            </select>
          </label>
          <label className="field" style={{ flex: 1, marginBottom: 0 }}>
            <span className="field-label">Ponto</span>
            <select value={sessionId} onChange={(e) => setSessionId(e.target.value)}>
              <option value={MAIN}>Entrada principal</option>
              {sessions.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.title}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <form
        className="card"
        onSubmit={(e) => {
          e.preventDefault();
          void doLookup(code);
        }}
      >
        <input
          className="code-input"
          value={code}
          onChange={(e) => setCode(e.target.value.toUpperCase())}
          placeholder="CÓDIGO"
          aria-label="Código de acesso"
        />
        <div className="row-buttons">
          <button type="submit" className="btn primary" disabled={busy}>
            {busy ? 'Consultando…' : 'Consultar'}
          </button>
          <button type="button" className="btn" onClick={() => setScanning((s) => !s)}>
            {scanning ? 'Parar câmera' : '📷 Escanear'}
          </button>
        </div>
        {scanning && <video ref={videoRef} className="scanner" muted playsInline />}
        {error && <div className="alert error">{error}</div>}
      </form>

      {lookup && !lookup.found && <div className="alert error">{lookup.reason}</div>}

      {lookup?.found && (
        <div className={`card lookup ${lookup.alreadyCheckedIn ? 'lookup-warn' : ''}`}>
          <div className="lookup-head">
            {lookup.photoDataUri ? (
              <img className="lookup-photo" src={lookup.photoDataUri} alt="Foto do participante" />
            ) : (
              <div className="lookup-photo placeholder">?</div>
            )}
            <div>
              <p className="lookup-name">{lookup.participantName}</p>
              <p className="muted">
                {lookup.course}
                {lookup.semester ? ` · ${lookup.semester}º sem` : ''}
              </p>
              <span className="badge badge-ok">
                {isMain ? 'Entrada principal' : sessions.find((s) => s.id === sessionId)?.title}
              </span>
            </div>
          </div>
          <dl className="data-list">
            <dt>Entrou no evento</dt>
            <dd>{lookup.alreadyCheckedIn ? 'Sim' : 'Ainda não'}</dd>
            <dt>E-mail</dt>
            <dd>{lookup.email}</dd>
          </dl>

          {result ? (
            <>
              <div className={`alert ${result.ok ? 'ok' : 'error'}`}>{result.reason}</div>
              <button className="btn" onClick={reset}>
                Próximo
              </button>
            </>
          ) : (
            <div className="row-buttons">
              <button className="btn primary" onClick={handleConfirm} disabled={confirmDisabled}>
                {isMain ? '✔️ Confirmar entrada' : '🎤 Marcar presença'}
              </button>
              <button className="btn" onClick={reset}>
                Cancelar
              </button>
            </div>
          )}
        </div>
      )}
    </section>
  );
}
