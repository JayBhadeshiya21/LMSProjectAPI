---------------------------------------------------- Leaning Management System (LMS) ----------------------------------------------------
use LMS_Project

------------------------ User ------------------------ 
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName VARCHAR(100),
    Email VARCHAR(100) UNIQUE,
    Password VARCHAR(255),
    Role VARCHAR(10) CHECK (Role IN ('Admin', 'Teacher', 'Student')),     
    CreatedAt DATETIME DEFAULT GETDATE(),
    Status BIT DEFAULT 1
);

------------------------ TeacherDetails ------------------------ 

CREATE TABLE TeacherDetails (
    UserId INT PRIMARY KEY,  
    Qualification VARCHAR(100),
    ExperienceYears DECIMAL(2,1),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

------------------------ StudentDetails ------------------------ 

CREATE TABLE StudentDetails (
    UserId INT PRIMARY KEY, 
    EnrollmentNumber VARCHAR(50),
    CourseStream VARCHAR(100),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

------------------------ Courses ------------------------ 
CREATE TABLE Courses (
    CourseId INT PRIMARY KEY IDENTITY,
    Title VARCHAR(200),
    Description TEXT,
    TeacherId INT,
    ImageURL VARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TeacherId) REFERENCES Users(UserId)
);




------------------------ Modules ------------------------ 
CREATE TABLE Modules (
    ModuleId INT PRIMARY KEY IDENTITY,
    CourseId INT,
    Title VARCHAR(200),
    Content TEXT,
    VideoURL VARCHAR(255) NULL,
    OrderIndex INT,
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);


------------------------ Enrollments ------------------------ 
CREATE TABLE Enrollments (
    EnrollmentId INT PRIMARY KEY IDENTITY(1,1),
    StudentId INT,
    CourseId INT,
    EnrolledOn DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (StudentId) REFERENCES Users(UserId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);

------------------------ Feedback ------------------------ 
CREATE TABLE Feedback (
    FeedbackId INT PRIMARY KEY IDENTITY,
    StudentId INT,
    CourseId INT,
    Comment TEXT,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (StudentId) REFERENCES Users(UserId),
    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
);



-- 1 Admin
INSERT INTO Users (FullName, Email, Password, Role) VALUES
('Admin User', 'admin@lms.com', 'admin123', 'Admin');

-- 2 Teachers
INSERT INTO Users (FullName, Email, Password, Role) VALUES
('Dr. John Smith', 'john@lms.com', 'pass123', 'Teacher'),
('Ms. Priya Mehta', 'priya@lms.com', 'pass456', 'Teacher');

-- 2 Students
INSERT INTO Users (FullName, Email, Password, Role) VALUES
('Alice Brown', 'alice@student.com', 'stu123', 'Student'),
('Ravi Kumar', 'ravi@student.com', 'stu456', 'Student');

INSERT INTO TeacherDetails (UserId, Qualification, ExperienceYears) VALUES
(2, 'Ph.D. Computer Science', 7.5),
(3, 'M.Sc. Mathematics', 5.0);

INSERT INTO StudentDetails (UserId, EnrollmentNumber, CourseStream) VALUES
(4, 'STU001', 'Computer Science'),
(5, 'STU002', 'Mathematics');

INSERT INTO Courses (Title, Description, TeacherId, ImageURL) VALUES
('Web Development', 'HTML, CSS, JS basics for beginners.', 2, 'https://cdn.example.com/webdev.jpg'),
('Calculus 101', 'Fundamentals of calculus.', 3, 'https://cdn.example.com/calculus.jpg');

-- For Web Development
INSERT INTO Modules (CourseId, Title, Content, VideoURL, OrderIndex) VALUES
(1, 'HTML Basics', 'Learn HTML tags and structure.', 'https://youtube.com/html', 1),
(1, 'CSS Basics', 'Style your pages with CSS.', 'https://youtube.com/css', 2);

-- For Web Development
INSERT INTO Modules (CourseId, Title, Content, VideoURL, OrderIndex) VALUES
(1, 'HTML Basics', 'Learn HTML tags and structure.', 'https://youtube.com/html', 1),
(1, 'CSS Basics', 'Style your pages with CSS.', 'https://youtube.com/css', 2);

-- For Calculus
INSERT INTO Modules (CourseId, Title, Content, VideoURL, OrderIndex) VALUES
(2, 'Limits', 'Introduction to limits.', 'https://youtube.com/limits', 1),
(2, 'Derivatives', 'Learn how derivatives work.', 'https://youtube.com/derivatives', 2);

INSERT INTO Enrollments (StudentId, CourseId) VALUES
(4, 1),  -- Alice enrolled in Web Dev
(5, 2);  -- Ravi enrolled in Calculus

INSERT INTO Feedback (StudentId, CourseId, Comment, Rating) VALUES
(4, 1, 'Great intro to web!', 5),
(5, 2, 'Very helpful for math revision.', 4)SELECT

select * from Courses





Courses   
