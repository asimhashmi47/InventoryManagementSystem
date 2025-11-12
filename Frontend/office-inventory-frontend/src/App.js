import React from 'react';
import { BrowserRouter, Route, Switch, Redirect } from 'react-router-dom';

import HomePage from './pages/HomePage';
import LoginPage from './pages/LoginPage';
import PrintStickers from "./pages/PrintStickers";

/**
 * Custom PrivateRoute component
 * 
 * What: A wrapper around the regular Route component that checks if the user is authenticated.
 * Why: To restrict access to certain pages (like HomePage) unless the user is logged in.
 * How: It reads the 'userData' from localStorage. If present, it renders the requested component; 
 *      otherwise, it redirects the user to the LoginPage.
 */
const PrivateRoute = ({ component: Component, ...rest }) => {
  const isLoggedIn = !!localStorage.getItem('userData'); // Convert stored value to boolean
  return (
    <Route
      {...rest}
      render={(props) =>
        isLoggedIn ? (
          <Component {...props} />
        ) : (
          <Redirect to="/login" />
        )
      }
    />
  );
};

/**
 * App Component
 * 
 * What: The root component that defines the routing for your application.
 * Why: To manage navigation between different pages in a single-page application (SPA).
 * How: By using BrowserRouter, Switch, and Route from 'react-router-dom' to set up routes.
 *      The PrivateRoute ensures that the HomePage is only accessible when a user is logged in.
 */
function App() {
  return (
    <BrowserRouter>
      <Switch>
        {/* If the user is not logged in, any attempt to access protected pages 
            will redirect them to the login page */}
        <Route path="/login" component={LoginPage} />

        {/* Protected Home route — accessible only after login */}
        <PrivateRoute exact path="/" component={HomePage} />
        <PrivateRoute path="/home" component={HomePage} />
        <PrivateRoute path="/print-stickers" component={PrintStickers} />
        {/* Future Note:
            If you're working on additional pages (e.g., /inventory, /settings), 
            add them as <PrivateRoute path="/inventory" component={InventoryPage} />.
            This allows deep linking if the user is logged in.
        */}
      </Switch>
    </BrowserRouter>
  );
}

export default App;
