import React, { useState } from 'react';
import { useHistory } from 'react-router-dom';
import { Container, Form, Button } from 'react-bootstrap';

// Base URL
const API_BASE_URL = process.env.REACT_APP_API_BASE_URL;

const LoginPage = () => {
  const history = useHistory();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    if (!username || !password) {
      setError('Fields cannot be empty.');
      return;
    }

    try {
      const response = await fetch(`${API_BASE_URL}/api/User/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: username, password }),
      });

      const data = await response.json();

      if (response.ok && data.status === 'Success') {
        localStorage.setItem('userData', JSON.stringify({ username }));
        history.push('/');
      } else {
        setError(data.message || 'Invalid credentials');
      }
    } catch (err) {
      debugger;
      console.log("error: "  + err);
      console.log("api base url " + API_BASE_URL);
      setError('Network error: ' + err.message.toString());
    }
  };

  return (
    <Container className="vh-100 d-flex align-items-center justify-content-center">
      <Form onSubmit={handleSubmit} className="w-50 p-4 shadow bg-light rounded">
        <h2 className="text-center">Login</h2>
        {error && <p className="text-danger text-center">{error}</p>}
        <Form.Group>
          <Form.Label>Email</Form.Label>
          <Form.Control type="text" value={username} onChange={(e) => setUsername(e.target.value)} />
        </Form.Group>
        <Form.Group>
          <Form.Label>Password</Form.Label>
          <Form.Control type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </Form.Group>
        <Button type="submit" className="w-100 mt-3">Login</Button>
      </Form>
    </Container>
  );
};

export default LoginPage;
