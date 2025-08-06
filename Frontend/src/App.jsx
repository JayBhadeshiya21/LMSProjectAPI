// import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
// import CourseForm from './pages/CourseForm';
// import CourseList from './pages/CourseList';
// import Dashboard from './pages/Dashboard';
// import 'bootstrap/dist/css/bootstrap.min.css';


// function App() {
//   return (
//     <Router>
//       <Routes>
//         <Route path="/" element={<Dashboard />} />
//         <Route path="/courses" element={<CourseList />} />
//         <Route path="/courses/new" element={<CourseForm />} />
//         <Route path="/courses/edit/:id" element={<CourseForm />} />
//       </Routes>
//     </Router>
//   );
// }

// export default App;

import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Dashboard from './pages/Dashboard';
import CourseList from './pages/CourseList';
import CourseForm from './pages/CourseForm';
import 'bootstrap/dist/css/bootstrap.min.css';
import Layout from './componets/Layout';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/admin" element={<Layout />}>
          {/* Admin Dashboard */}
          <Route path="dashboard" element={<Dashboard />} />

          {/* Course Management */}
          <Route path="courses" element={<CourseList />} />
          <Route path="courses/new" element={<CourseForm />} />
          <Route path="courses/edit/:id" element={<CourseForm />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;
