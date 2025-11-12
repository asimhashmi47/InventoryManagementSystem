import React from 'react';
import NavbarComponent from './Navbar';
import SidebarComponent from './Sidebar';
import { Container } from 'react-bootstrap';
import '../styles/layout.css';

const Layout = ({ children }) => {
  return (
    <div className="d-flex flex-column vh-100">
      <NavbarComponent />

      <div className="d-flex flex-grow-1">
        <SidebarComponent />

        <Container fluid className="p-4 back-color-alice-blue" style={{ overflowY: 'auto' }}>
          {children} {/* This renders the page-specific content */}
        </Container>
      </div>
    </div>
  );
};

export default Layout;
