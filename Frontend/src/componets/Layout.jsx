import React from 'react';
import Sidebar from './Sidebar';
import { Outlet } from 'react-router-dom'; // For nested routing

const Layout = () => {
  return (
    <div className="layout-wrapper layout-content-navbar">
      <div className="layout-container">
        <Sidebar />
        <div className="layout-page">
          <div className="content-wrapper">
            <Outlet />
          </div>
        </div>
      </div>
    </div>
  );
};

export default Layout;
