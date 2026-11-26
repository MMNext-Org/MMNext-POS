import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './context/AuthContext'
import { ColorModeProvider, ChakraProvider, Toastr } from '@chakra-ui/react';

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
    <ColorModeProvider>
      <ChakraProvider>
        <AuthProvider>
          <App />
          <Toastr />
        </AuthProvider>
      </ChakraProvider>
    </ColorModeProvider>
  </StrictMode>,
)
