import { Link } from 'react-router-dom';

export default function NotFoundPage() {
  return (
    <section className="narrow">
      <h1>404</h1>
      <p className="muted">Página não encontrada.</p>
      <Link to="/" className="btn primary">
        Voltar aos eventos
      </Link>
    </section>
  );
}
