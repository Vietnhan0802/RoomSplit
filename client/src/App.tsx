import { AuthProvider } from './contexts/AuthContext';
import { ThemeProvider } from './contexts/ThemeContext';
import AppRouter from './routes/AppRouter';
import ToastContainer from './components/ui/Toast';
import NetworkStatus from './components/ui/NetworkStatus';
import InstallPWA from './components/shared/InstallPWA';
import SWUpdatePrompt from './components/shared/SWUpdatePrompt';

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <AppRouter />
        <ToastContainer />
        <NetworkStatus />
        <InstallPWA />
        <SWUpdatePrompt />
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
