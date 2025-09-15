import React from 'react';
import { Nav } from 'react-bootstrap';
import '../styles/layout.css'; // ✅ Import the shared layout styles

const SidebarComponent = () => {
  return (
    <div className="sidebar-custom">  {/* ✅ Apply custom class */}
      <h6 className="mb-4 text-muted">Navigation</h6>
      <Nav className="flex-column">
        <Nav.Link href="/home">🏠 Home</Nav.Link>
        <Nav.Link href="/form">📝 New Form</Nav.Link>
        <Nav.Link href="/suppliers">📦 Suppliers</Nav.Link>
      </Nav>
    </div>
  );
};

export default SidebarComponent;
