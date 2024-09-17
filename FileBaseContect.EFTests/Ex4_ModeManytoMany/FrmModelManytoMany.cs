using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Ex4_ModelManytoMany
{
    public partial class FrmModelManytoMany : Form
    {
        public FrmModelManytoMany()
        {
            InitializeComponent();
        }

        private void btnMany2Many_Click(object sender, EventArgs e)
        {
            // Trigger the many-to-many relationship test
            TestMany2Many();
        }

        void TestMany2Many()
        {
            txtDebug.Text = "TestMany2Many()\r\n";

            using (EFModelManytoMany context = new EFModelManytoMany())
            {
                // Confirm deletion of the existing database
                var result = MessageBox.Show("Do you really want to delete the existing database?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    txtDebug.Text += "Deleting DB...\r\n";
                    context.Database.EnsureDeleted(); // Ensure the database is deleted
                    context.DeleteOldStore(); // Custom implementation to delete old store

                    txtDebug.Text += "Deleted Ok\r\n";
                }

                // Generate sample data
                List<Student> students = GenerateStudents(10);
                List<Course> courses = GenerateCourse(10);


                foreach (var s in students)
                {
                    context.Students.Add(s);
                }


                foreach (var c in courses)
                {
                    context.Courses.Add(c);
                }
                // This didnt work? Add generated data to the context
               // context.Students.AddRange(students);
               // context.Courses.AddRange(courses);

                context.SaveChanges();
            }

            using (EFModelManytoMany context = new EFModelManytoMany())
            {
                List<Student> students = context.Students.ToList();
                List<Course> courses = context.Courses.ToList();
                txtDebug.Text += $"Loaded Students [{students.Count}] & Courses [{courses.Count}] ..Ok\r\n";
                // Associate random many-to-many
                Random random = new Random();
                for (int i = 0; i < 30; i++)
                {
                    var student = students[random.Next(students.Count)];
                    var course = courses[random.Next(courses.Count)];

                    // Check if the enrollment already exists for student and course
                    if (!context.Enrollments.Any(e => e.Student.Id == student.Id && e.Course.Id == course.Id))
                    {
                        // Association / Joining table for many-to-many
                        Enrollment enrollment = new Enrollment();
                        enrollment.Student = student;   // Can be duplicates IRL
                        enrollment.Course = course;     // Can be duplicates IRL
                        context.Enrollments.Add(enrollment);
                        context.SaveChanges();
                        txtDebug.Text += $"Enrolled Student : {student.FirstName} {student.LastName} to  Course [{course.Id}] {course.Title}..Ok\r\n";

                    }
                }

                context.SaveChanges();

                txtDebug.Text += "Added students, courses, and enrollments to the context and saved changes.\r\n";
            }

            using (EFModelManytoMany context = new EFModelManytoMany())
            {
                try
                {
                    // Save changes to persist the relationships
                    context.SaveChanges();
                    txtDebug.Text += "Saved changes to persist the many-to-many relationships.\r\n";
                }
                catch (DbUpdateException dbEx)
                {
                    // Handle exceptions and provide detailed error messages
                    Exception raise = dbEx;
                    foreach (var entry in dbEx.Entries)
                    {
                        string message = $"Entity of type {entry.Entity.GetType().Name} in state {entry.State} could not be updated";
                        raise = new InvalidOperationException(message, raise);
                    }
                    txtDebug.Text += $"Error: {raise.Message}\r\n";
                    throw raise;
                }
            }
        }

        void ReadDbEnrolments()
        {
            // WIP : Fails with duplicate key detected.. 
            txtDebug.Text += "\r\n--------- READ ENROLMENTS -------\r\n";
            using (EFModelManytoMany readback = new EFModelManytoMany())
            {
                // Read Joining Table enrollments
                var enrollments = readback.Enrollments
                    .Select(e => new
                    {
                        StudentId = e.Student.Id,
                        CourseId = e.Course.Id
                    })
                    .ToList();

                // Display enrollments by showing students and their courses
                foreach (var enrol in enrollments)
                {
                    txtDebug.Text += $"Enrollment: {enrol.StudentId} {enrol.CourseId} \r\n";
                }
                    // Display enrollments by showing students and their courses

                    //// Read Joining Table enrollments
                    //var enrollments = readback.Enrollments
                    //    .Include(e => e.Student)
                    //    .Include(e => e.Course)
                    //    .ToList();

                    //// Display enrollments by showing students and their courses
                    //foreach (var enrol in enrollments)
                    //{
                    //    txtDebug.Text += $"Enrollment: {enrol.Id} {enrol.Course.Id} {enrol.Student.Id} \r\n";

                    //    //txtDebug.Text += $"Enrollment: {enrol.Id} {enrol.Course.Title} {enrol.Student.FirstName} {enrol.Student.LastName}\r\n";
                    //}
                }

            // Prompt to open the database folder
            var openresult = MessageBox.Show("Do you want to view the database filesystem?", "Open Folder", MessageBoxButtons.YesNo);
            if (openresult == DialogResult.Yes)
            {
                // Open the database folder
                string dbFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, EFModelManytoMany.DatabaseName);
                Process.Start("explorer.exe", dbFolderPath);
            }
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            // Trigger the database read operation
            txtDebug.Text = "Reading Database...\r\n";
            ReadDbEnrolments();
        }

        public static List<Student> GenerateStudents(int count)
        {
            // Generate a list of students using Bogus
            var studentFaker = new Faker<Student>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName());

            return studentFaker.Generate(count);
        }

        public static List<Course> GenerateCourse(int count)
        {
            // List of sample educational course titles
            var courseTitles = new List<string>
            {
                "Introduction to Computer Science",
                "Data Structures and Algorithms",
                "Database Management Systems",
                "Operating Systems",
                "Software Engineering",
                "Artificial Intelligence",
                "Machine Learning",
                "Web Development",
                "Mobile App Development",
                "Cybersecurity"
            };

            // Generate a list of educational courses using Bogus
            var courseFaker = new Faker<Course>()
                .RuleFor(p => p.Title, f => f.PickRandom(courseTitles));

            return courseFaker.Generate(count);
        }
    }
}
