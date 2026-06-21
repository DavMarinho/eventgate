import { useState } from 'react';
import { api, ApiError } from '../api/client';
import type { RegistrationResponse } from '../api/types';
import Field from '../components/Field';

export default function MyDataPage() {
  const [accessCode, setAccessCode] = useState('');
  const [email, setEmail] = useState('');
  const [data, setData] = useState<RegistrationResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [info, setInfo] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleFetch(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setInfo(null);
    setData(null);
    setBusy(true);
    try {
      const res = await api.getMyData({ accessCode: accessCode.trim(), email: email.trim() });
      setData(res);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  async function handleDelete() {
    if (!confirm('Tem certeza? A exclusão dos seus dados é definitiva.')) return;
    setError(null);
    setBusy(true);
    try {
      await api.deleteMyData({ accessCode: accessCode.trim(), email: email.trim() });
      setData(null);
      setInfo('Seus dados foram excluídos. Obrigado!');
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Erro inesperado.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="narrow">
      <h1>Meus dados (LGPD)</h1>
      <p className="muted">
        Direito de acesso e direito ao esquecimento. Informe o código de acesso e o e-mail usados na
        inscrição. Esses dados vão no corpo da requisição — nunca na URL.
      </p>

      <form className="card" onSubmit={handleFetch}>
        <Field label="Código de acesso">
          <input
            required
            value={accessCode}
            onChange={(e) => setAccessCode(e.target.value.toUpperCase())}
            placeholder="EX: ABCD2345"
          />
        </Field>
        <Field label="E-mail da inscrição">
          <input
            required
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="voce@exemplo.com"
          />
        </Field>
        {error && <div className="alert error">{error}</div>}
        {info && <div className="alert ok">{info}</div>}
        <button type="submit" className="btn primary" disabled={busy}>
          {busy ? 'Consultando…' : 'Consultar meus dados'}
        </button>
      </form>

      {data && (
        <div className="card">
          <h2>Seus dados</h2>
          <dl className="data-list">
            <dt>Nome</dt>
            <dd>{data.participantName}</dd>
            <dt>E-mail</dt>
            <dd>{data.participantEmail}</dd>
            <dt>Código</dt>
            <dd>{data.accessCode}</dd>
            <dt>Status</dt>
            <dd>{data.status}</dd>
            <dt>Consentimento</dt>
            <dd>
              {data.consentAccepted ? 'Sim' : 'Não'}
              {data.consentAcceptedAt &&
                ` (${new Date(data.consentAcceptedAt).toLocaleString('pt-BR')})`}
            </dd>
            <dt>Inscrito em</dt>
            <dd>{new Date(data.createdAt).toLocaleString('pt-BR')}</dd>
          </dl>
          <button className="btn danger" onClick={handleDelete} disabled={busy}>
            Excluir meus dados
          </button>
        </div>
      )}
    </section>
  );
}
