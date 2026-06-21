import { lazy, Suspense } from 'react';
import { Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import ProtectedRoute from './components/ProtectedRoute';
import LandingPage from './pages/LandingPage';
import StaffPortalPage from './pages/StaffPortalPage';
import EventsPage from './pages/EventsPage';
import EventDetailPage from './pages/EventDetailPage';
import MyDataPage from './pages/MyDataPage';
import LoginPage from './pages/LoginPage';
import CreateEventPage from './pages/CreateEventPage';
import CreateCoursePage from './pages/CreateCoursePage';
import CreateStaffPage from './pages/CreateStaffPage';
import StatsPage from './pages/StatsPage';
import RegistrationsListPage from './pages/RegistrationsListPage';
import SpeakersAdminPage from './pages/SpeakersAdminPage';
import SessionsAdminPage from './pages/SessionsAdminPage';
import NotFoundPage from './pages/NotFoundPage';

// Code-split das telas com dependências pesadas (Chart.js, leitor de QR).
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const GatePanelPage = lazy(() => import('./pages/GatePanelPage'));

const staffRoles = ['Organizer', 'Validator'];

export default function App() {
  return (
    <Suspense fallback={<p className="muted" style={{ padding: '2rem' }}>Carregando…</p>}>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<LandingPage />} />
          <Route path="eventos" element={<EventsPage />} />
          <Route path="events/:id" element={<EventDetailPage />} />
          <Route path="my-data" element={<MyDataPage />} />
          <Route path="login" element={<LoginPage />} />

          <Route
            path="equipe"
            element={
              <ProtectedRoute roles={staffRoles}>
                <StaffPortalPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="gate"
            element={
              <ProtectedRoute roles={staffRoles}>
                <GatePanelPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="events/new"
            element={
              <ProtectedRoute roles={['Organizer']}>
                <CreateEventPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="courses/new"
            element={
              <ProtectedRoute roles={['Organizer']}>
                <CreateCoursePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="staff/new"
            element={
              <ProtectedRoute roles={['Organizer']}>
                <CreateStaffPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="events/:id/stats"
            element={
              <ProtectedRoute roles={staffRoles}>
                <StatsPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="events/:id/dashboard"
            element={
              <ProtectedRoute roles={staffRoles}>
                <DashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="events/:id/registrations"
            element={
              <ProtectedRoute roles={staffRoles}>
                <RegistrationsListPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="events/:id/speakers"
            element={
              <ProtectedRoute roles={['Organizer']}>
                <SpeakersAdminPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="events/:id/sessions"
            element={
              <ProtectedRoute roles={['Organizer']}>
                <SessionsAdminPage />
              </ProtectedRoute>
            }
          />

          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </Suspense>
  );
}
