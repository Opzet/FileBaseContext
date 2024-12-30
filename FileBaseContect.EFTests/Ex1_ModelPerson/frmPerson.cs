using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using Bogus;
using System.Text; // for generating fake data

namespace Ex1_ModelPerson
{
    public partial class FrmPerson : Form
    {
        public FrmPerson()
        {
            InitializeComponent();
        }

        private void btnTestPerson_Click(object sender, EventArgs e)
        {
            TestPeople();
        }

        private void TestPeople()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            txtDebug.Text = "TestPerson()\r\n";

            using (PersonModel context = new PersonModel())
            {
                // Ask for confirmation before deleting the database
                var result = MessageBox.Show("Do you really want to delete the existing database?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    txtDebug.Text += "Deleting DB...\r\n";
                    context.Database.EnsureDeleted();
                    context.DeleteOldStore(); // Custom implementation to delete old store

                    txtDebug.Text += "Deleted Ok\r\n";
                }

                txtDebug.Text += "Creating DB...\r\n";
                context.Database.EnsureCreated();
                
                //context.HasSchemaChanged()

                txtDebug.Text += "Created DB\r\n";

                List<Person> people = GeneratePeople(500);

                context.People.AddRange(people);

                try
                {
                    txtDebug.Text += "Saving data to the database...\r\n";
                    context.SaveChanges();
                    txtDebug.Text += "Data saved successfully\r\n";
                }
                catch (DbUpdateException dbEx)
                {
                    // This Exception handler helps to describe what went wrong with the EF database save inner exception detail.
                    // It decodes why the data did not comply with defined database field structure. e.g too long or wrong type.
                    Exception raise = dbEx;
                    foreach (var entry in dbEx.Entries)
                    {
                        string message = $"Entity of type {entry.Entity.GetType().Name} in state {entry.State} could not be updated";
                        raise = new InvalidOperationException(message, raise);
                    }
                    throw raise;
                }

                // Read it back
                List<Person> dbpeople = context.People.ToList();

                stopwatch.Stop();
                txtDebug.Text += $"Database operations completed in {stopwatch.ElapsedMilliseconds} ms\r\n";

                StringBuilder peopleList = new StringBuilder();
                foreach (Person p in dbpeople)
                {
                    peopleList.AppendFormat("{0} {1} {2} {3} {4}\r\n", p.Id, p.FirstName, p.MiddleName, p.LastName, p.Phone);
                }

                txtDebug.Text += peopleList.ToString();

                result = MessageBox.Show("Do you want to view the database filesystem?", "Open Folder", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Open the database folder
                    string dbFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PersonModel");
                    Process.Start("explorer.exe", dbFolderPath);
                }
            }

            txtDebug.Text += $"Total operation time: {stopwatch.ElapsedMilliseconds} ms\r\n";
        }


        public static List<Person> GeneratePeople(int count)
        {
            var personFaker = new Faker<Person>()
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                // Make Mobile phone number format (eg. 04## ### ###)
                .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber("04## ### ###"))
            .RuleFor(p => p.Location, f => new NetTopologySuite.Geometries.Point(f.Address.Latitude(), f.Address.Longitude()) { SRID = 4326 });
            return personFaker.Generate(count);
        }


        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFilter.Text = "";

        }
        private void txtFilter_KeyPress(object sender, KeyPressEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            txtDebug.Text = "Starting search...\r\n";

            using (PersonModel context = new PersonModel())
            {
                try
                {
                    // Search for people within the first and last name containing the filter text
                    List<Person> people = context.People
                        .Where(p => p.FirstName.Contains(txtFilter.Text) || p.LastName.Contains(txtFilter.Text))
                        .ToList();

                    stopwatch.Stop();
                    txtDebug.Text += $"Search completed in {stopwatch.ElapsedMilliseconds} ms\r\n";
                    txtDebug.Text += $"Found {people.Count} people matching the filter\r\n";

                    txtResults.Text = "";


                    StringBuilder resultsBuilder = new StringBuilder();
                    foreach (Person p in people)
                    {
                        // Pause updating the textbox to make it faster
                        resultsBuilder.AppendFormat("{0} {1} {2} {3} {4}\r\n", p.Id, p.FirstName, p.MiddleName, p.LastName, p.Phone);
                    }
                    txtResults.Text = resultsBuilder.ToString();

                    txtDebug.Text += "Results displayed successfully\r\n";
                }
                catch (Exception ex)
                {
                    txtDebug.Text += $"An error occurred: {ex.Message}\r\n";
                }
            }

            stopwatch.Stop();
            txtDebug.Text += $"Total operation time: {stopwatch.ElapsedMilliseconds} ms\r\n";
        }

        private void btnLoadPeople_Click(object sender, EventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            txtDebug.Text = "Loading people search...\r\n";

            using (PersonModel context = new PersonModel())
            {
                List<Person> people = context.People.ToList();

                stopwatch.Stop();
                txtDebug.Text += $"Search completed in {stopwatch.ElapsedMilliseconds} ms\r\n";
                txtDebug.Text += $"Found {people.Count} people \r\n";

                txtResults.Text = "";

                StringBuilder resultsBuilder = new StringBuilder();
                foreach (Person p in people)
                {
                    // Pause updating the textbox to make it faster
                    resultsBuilder.AppendFormat("{0} {1} {2} {3} {4}\r\n", p.Id, p.FirstName, p.MiddleName, p.LastName, p.Phone);
                }
                txtResults.Text = resultsBuilder.ToString();
            }
        }
    }
}