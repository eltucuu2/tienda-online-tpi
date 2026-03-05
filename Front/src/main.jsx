import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    {/* ¡IMPORTANTE! Aquí SOLO debe estar <App />. 
        NO pongas <BrowserRouter> ni AuthProvider aquí, 
        porque ya están en App.jsx */}
    <App />
  </React.StrictMode>,
)