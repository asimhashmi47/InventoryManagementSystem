import React from 'react';
import { createRoot } from "react-dom/client";
import 'bootstrap/dist/css/bootstrap.min.css';
import App from './App';

// (What) Renders <App /> into the <div id="root"> in public/index.html
// What: The main entry point for the React app. It renders the App component into the root element.
// Why: Every React application needs a root file to bootstrap the React DOM.
// How: We import Bootstrap and Semantic UI CSS globally so all components can use their styles.

const container = document.getElementById("root");
const root = createRoot(container);
root.render(<App />);