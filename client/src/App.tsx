import { AuthProvider } from './contexts/AuthContext';
import AppRouter from './routes/AppRouter';
import ToastContainer from './components/ui/Toast';

function App() {
  return (
    <AuthProvider>
      <AppRouter />
      <ToastContainer />
    </AuthProvider>
  );
}

export default App;
