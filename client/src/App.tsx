import { AuthProvider } from './contexts/AuthContext';
import { ThemeProvider } from './contexts/ThemeContext';
import AppRouter from './routes/AppRouter';
import ToastContainer from './components/ui/Toast';
import NetworkStatus from './components/ui/NetworkStatus';

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <AppRouter />
        <ToastContainer />
        <NetworkStatus />
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
