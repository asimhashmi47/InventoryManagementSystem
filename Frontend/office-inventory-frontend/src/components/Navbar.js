import React from 'react';
import { Navbar, Nav, Button, Container } from 'react-bootstrap';
import { useHistory } from 'react-router-dom';
import '../styles/layout.css'; // ✅ Import the shared layout styles

const NavbarComponent = () => {
  const history = useHistory();

  const handleLogout = () => {
    localStorage.removeItem('userData');
    history.push('/login');
  };

  return (
    <Navbar expand="lg" className="navbar-custom">  {/* ✅ Apply custom class */}
      <Container fluid>
        <Navbar.Brand href="/">Inventory System</Navbar.Brand>
        <Nav className="ms-auto">
          <Button className="btn-logout" variant="outline-primary" onClick={handleLogout}>
            Logout
          </Button>
        </Nav>
      </Container>
    </Navbar>
  );
};

export default NavbarComponent;
