import React from "react";
import { Nav } from "react-bootstrap";
import { NavLink } from "react-router-dom";
import "../styles/layout.css";

const SidebarComponent = () => {
  return (
    <div className="sidebar-custom">
      <h6 className="mb-4 text-muted">Navigation</h6>
      <Nav className="flex-column">

        <Nav.Link as={NavLink} to="/home" activeClassName="active">
          <span role="img" aria-label="home">🏠</span> Home
        </Nav.Link>

        <Nav.Link as={NavLink} to="/form" activeClassName="active">
          <span role="img" aria-label="new-form">📝</span> New Form
        </Nav.Link>

        <Nav.Link as={NavLink} to="/suppliers" activeClassName="active">
          <span role="img" aria-label="suppliers">📦</span> Suppliers
        </Nav.Link>

        <Nav.Link as={NavLink} to="/print-stickers" activeClassName="active">
          <span role="img" aria-label="print-stickers">🏷️</span> Print Stickers
        </Nav.Link>

      </Nav>
    </div>
  );
};

export default SidebarComponent;
