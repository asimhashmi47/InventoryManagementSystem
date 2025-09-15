// src/index.js
import React from 'react';
import ReactDOM from 'react-dom';
import 'bootstrap/dist/css/bootstrap.min.css';     // (What) Bootstrap's default styles
//import 'semantic-ui-css/semantic.min.css';         // (What) Semantic UI default styles
import App from './App';

// (What) Renders <App /> into the <div id="root"> in public/index.html
// What: The main entry point for the React app. It renders the App component into the root element.
// Why: Every React application needs a root file to bootstrap the React DOM.
// How: We import Bootstrap and Semantic UI CSS globally so all components can use their styles.

ReactDOM.render(<App />, document.getElementById('root'));
