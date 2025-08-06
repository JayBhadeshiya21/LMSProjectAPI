import React from 'react';
import { Link } from 'react-router-dom';

const Dashboard = () => {
  return (
    <div className="container py-5">
      <div className="text-center mb-4">
        <h1 className="display-5 fw-bold text-primary">LMS Admin Dashboard</h1>
        <p className="lead">Manage all your learning modules and users in one place</p>
      </div>

      <div className="row g-4">
        <div className="col-md-6">
          <Link to="/courses" className="text-decoration-none">
            <div className="card border-primary h-100">
              <div className="card-body">
                <h5 className="card-title">📘 Manage Courses</h5>
                <p className="card-text">Get, Add, update or delete course content.</p>
              </div>
            </div>
          </Link>
        </div>

        <div className="col-md-6">
          <Link to="/modules" className="text-decoration-none">
            <div className="card border-success h-100">
              <div className="card-body">
                <h5 className="card-title">🎬 Manage Modules</h5>
                <p className="card-text">Organize lessons and upload video content.</p>
              </div>
            </div>
          </Link>
        </div>

        <div className="col-md-6">
          <Link to="/students" className="text-decoration-none">
            <div className="card border-warning h-100">
              <div className="card-body">
                <h5 className="card-title">🎓 Student Records</h5>
                <p className="card-text">View or edit enrolled students' details.</p>
              </div>
            </div>
          </Link>
        </div>

        <div className="col-md-6">
          <Link to="/teachers" className="text-decoration-none">
            <div className="card border-danger h-100">
              <div className="card-body">
                <h5 className="card-title">👨‍🏫 Teacher Profiles</h5>
                <p className="card-text">Manage teacher information and experience.</p>
              </div>
            </div>
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
