import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';

const CourseForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const [course, setCourse] = useState({
    title: '',
    description: '',
    teacherId: '',
    imageUrl: '',
    createdAt: new Date().toISOString()
  });

  useEffect(() => {
    if (isEdit) {
      axios.get(`http://localhost:5281/api/CourseAPI/${id}`)
        .then(res => setCourse(res.data))
        .catch(err => alert("Error fetching course data"));
    }
  }, [id]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setCourse(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      if (isEdit) {
        await axios.put(`http://localhost:5281/api/CourseAPI/${id}`, course);
      } else {
        await axios.post(`http://localhost:5281/api/CourseAPI`, course);
      }
      navigate('/courses');
    } catch (err) {
      console.error(err);
      alert("Failed to save course. Please check the input.");
    }
  };

  return (
    <div className="container py-4">
      <div className="card shadow p-4">
        <h3 className="mb-4 text-primary">{isEdit ? "Edit Course" : "Create New Course"}</h3>
        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label">Course Title</label>
            <input
              type="text"
              name="title"
              value={course.title}
              onChange={handleChange}
              className="form-control"
              required
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Description</label>
            <textarea
              name="description"
              value={course.description}
              onChange={handleChange}
              className="form-control"
              rows="3"
              required
            ></textarea>
          </div>

          <div className="mb-3">
            <label className="form-label">Teacher ID</label>
            <input
              type="number"
              name="teacherId"
              value={course.teacherId}
              onChange={handleChange}
              className="form-control"
              required
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Image URL</label>
            <input
              type="url"
              name="imageUrl"
              value={course.imageUrl}
              onChange={handleChange}
              className="form-control"
              required
            />
          </div>

          <button type="submit" className="btn btn-primary me-2">💾 Save</button>
          <button type="button" onClick={() => navigate('/courses')} className="btn btn-secondary">Cancel</button>
        </form>
      </div>
    </div>
  );
};

export default CourseForm;
