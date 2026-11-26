import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './context/AuthContext'
import { ChakraProvider, defaultSystem } from '@chakra-ui/react'

// Register service worker for PWA
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then(registration => {
        console.log('SW registered: ', registration)
      })
      .catch(error => {
        console.log('Error during SW registration: ', error)
      })
  });
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ChakraProvider value={defaultSystem}>
      <AuthProvider>
        <App />
      </AuthProvider>
    </ChakraProvider>
  </StrictMode>,
)
