import React from 'react';
import { NavLink } from 'react-router-dom';

const Sidebar = () => {
  return (
    <>
   <div className="layout-wrapper layout-content-navbar">
  <div className="layout-container">
   <aside id="layout-menu" className="layout-menu menu-vertical menu bg-menu-theme">
  <div className="app-brand demo">
    <a href="index.html" className="app-brand-link">
      <span className="app-brand-logo demo">
        {/* <!-- SVG Logo here --> */}
      </span>
      <span
        className="app-brand-text demo menu-text fw-bolder ms-2"
        style={{ textTransform: 'uppercase' }}
      >
        LMS
      </span>
    </a>
    <a href="javascript:void(0);" className="layout-menu-toggle menu-link text-large ms-auto d-block d-xl-none">
      <i className="bx bx-chevron-left bx-sm align-middle"></i>
    </a>
  </div>

  <div className="menu-inner-shadow"></div>

  <ul className="menu-inner py-1">
    <li className="menu-item active">
      <a href="/admin/dashboard.html" className="menu-link">
        <i className="menu-icon tf-icons bx bx-home-circle"></i>
        <div>Dashboard</div>
      </a>
    </li>
    <li className="menu-item">
      <a href="/admin/courses.html" className="menu-link">
        <i className="menu-icon tf-icons bx bx-book-bookmark"></i>
        <div>Courses</div>
      </a>
    </li>
    <li className="menu-item">
      <a href="/admin/teachers.html" className="menu-link">
        <i className="menu-icon tf-icons bx bx-chalkboard"></i>
        <div>Teachers</div>
      </a>
    </li>
    <li className="menu-item">
      <a href="/admin/students.html" className="menu-link">
        <i className="menu-icon tf-icons bx bx-user"></i>
        <div>Students</div>
      </a>
    </li>
    <li className="menu-item">
      <a href="/admin/enrollments.html" className="menu-link">
        <i className="menu-icon tf-icons bx bx-check-circle"></i>
        <div>Enrollments</div>
      </a>
    </li>
    <li className="menu-item">
      <a href="/admin/feedback.html" className="menu-link">
        <i className="menu-icon tf-icons bx bx-comment-detail"></i>
        <div>Feedback</div>
      </a>
    </li>
  </ul>
</aside>


    </div>
  </div>

  
</>

  );
};

export default Sidebar;
