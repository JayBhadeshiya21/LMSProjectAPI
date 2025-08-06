import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';

const CourseList = () => {
  const [courses, setCourses] = useState([]);

  useEffect(() => {
    fetchCourses();
  }, []);

  const fetchCourses = () => {
    axios.get('http://localhost:5281/api/CourseAPI')
      .then(res => setCourses(res.data))
      .catch(err => console.error(err));
  };

  const handleDelete = async (id) => {
    const confirmDelete = window.confirm('Are you sure you want to delete this course?');
    if (!confirmDelete) return;

    try {
      await axios.delete(`http://localhost:5281/api/CourseAPI/${id}`);
      setCourses(prev => prev.filter(course => course.courseId !== id));
    } catch (err) {
      console.error('Delete failed', err);
      alert('Something went wrong while deleting the course.');
    }
  };

  return (
    <div className="container py-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 className="text-primary">📚 All Courses</h2>
        <Link to="/courses/new" className="btn btn-success">
          + Add New Course
        </Link>
      </div>

      {courses.length === 0 ? (
        <div className="alert alert-warning">No courses found.</div>
      ) : (
        <div className="table-responsive">
          <table className="table table-bordered table-hover shadow-sm">
            <thead className="table-light">
              <tr>
                <th>#</th>
                <th>Course Title</th>
                <th>Description</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {courses.map((course, index) => (
                <tr key={course.courseId}>
                  <td>{index + 1}</td>
                  <td>{course.title}</td>
                  <td>{course.description}</td>
                  <td>
                    <div className="btn-group " role="group">
                      <Link to={`/courses/edit/${course.courseId}`} className="btn btn-sm btn-primary">
                        Edit
                      </Link>
                      <button
                        onClick={() => handleDelete(course.courseId)}
                        className="btn btn-sm btn-danger "
                      >
                        Delete
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default CourseList;
