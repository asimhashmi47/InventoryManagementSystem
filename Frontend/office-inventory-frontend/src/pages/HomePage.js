// src/pages/HomePage.js
/**
 * What: This HomePage component serves as the dashboard and uses the centralized Layout
 *       (which includes the Navbar and Sidebar) to maintain a consistent look across pages.
 *
 * Why: The dashboard displays key metrics (total products, low stock, out of stock, and most stock product)
 *      using colorful cards. This template design gives users an at-a-glance overview of critical data.
 *
 * How: The component imports a custom CSS file (DashboardCards.css) for all dashboard card styles,
 *      then uses Bootstrap’s grid system to layout the cards responsively.
 *      It wraps all content inside the Layout component, ensuring that the navbar and sidebar appear on every page.
 */

import React from 'react';
import Layout from '../components/Layout';
import '../styles/DashboardCards.css'; 

const HomePage = () => {
  return (
    <Layout>
      <div className="container my-5">
        <h2 className="mb-4">Dashboard</h2>
        <div className="row g-3">
          {/* Card 1: Total Products */}
          <div className="col-sm-6 col-md-3">
            <div className="dashboard-card gradient1">
              <div className="card-content">
                <h5>Total Products</h5>
                <h2 className="count">4555</h2>
                {/* <p>Jan - March 2019</p> */}
              </div>
              <div className="card-icon">
                <i className="fas fa-shopping-cart"></i>
              </div>
            </div>
          </div>

          {/* Card 2: Low Stock Products */}
          <div className="col-sm-6 col-md-3">
            <div className="dashboard-card gradient2">
              <div className="card-content">
                <h5>Low Stock Products</h5>
                <h2 className="count">851</h2>
                {/* <p>Jan - March 2019</p> */}
              </div>
              <div className="card-icon">
                <i className="fas fa-exclamation-triangle"></i>
              </div>
            </div>
          </div>

          {/* Card 3: Out of Stock Products */}
          <div className="col-sm-6 col-md-3">
            <div className="dashboard-card gradient3">
              <div className="card-content">
                <h5>Out of Stock Products</h5>
                <h2 className="count">120</h2>
                {/* <p>Jan - March 2019</p> */}
              </div>
              <div className="card-icon">
                <i className="fas fa-times-circle"></i>
              </div>
            </div>
          </div>

          {/* Card 4: Most Stock Product */}
          <div className="col-sm-6 col-md-3">
            <div className="dashboard-card gradient4">
              <div className="card-content">
                <h5>Most Stock Product</h5>
                <h2 className="count">99%</h2>
                {/* <p>Jan - March 2019</p> */}
              </div>
              <div className="card-icon">
                <i className="fas fa-box-open"></i>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
};

export default HomePage;
