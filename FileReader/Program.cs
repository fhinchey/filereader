using System;
using System.IO;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace FileReader
{

    class Person
    {
        public string LastName
        {
            get;
            set;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string Sex
        {
            get;
            set;
        }

        public string FavoriteColor
        {
            get;
            set;
        }

        public DateTime BirthDate
        {
            get;
            set;
        }
    }

    class Program
    {

        enum Format { Comma, Pipe, Space };

        static List<Person> people = new List<Person>();

        static void Main(string[] args)
        {

            String command = Console.ReadLine();

            switch (command)
            {

                case "/p":

                    Console.WriteLine();

                    string path = ConfigurationManager.AppSettings["InputDirectory"];

                    if (Directory.Exists(path))
                    {

                        ProcessDirectory(path);
                    }
                    else
                    {

                        Console.WriteLine("Can't find input path");
                    }

                    break;
                default:

                    Console.WriteLine("Unknown Command " + command);

                    break;
            }

            Console.ReadKey();
        }

        public static void ProcessDirectory(string targetDirectory)
        {

            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.txt");

            if (fileEntries.Length == 0)
                Console.WriteLine("Input directory is empty");

            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            if (people.Count > 0)
            {

                WritePeopleToFile(people);

                var sorted = people.OrderBy(d => d.Sex).ThenByDescending(d => d.LastName).ToList();

                Console.WriteLine("Sorted by gender (asc), last name (asc):");
                Console.WriteLine();

                PrintPeople(sorted);

                sorted = people.OrderBy(d => d.BirthDate).ToList();

                Console.WriteLine("Sorted by birth date (asc):");
                Console.WriteLine();

                PrintPeople(sorted);

                sorted = people.OrderByDescending(d => d.LastName).ToList();

                Console.WriteLine("Sorted by last name (desc):");
                Console.WriteLine();

                PrintPeople(sorted);
            }
        }

        public static void ProcessFile(string path)
        {


            Console.WriteLine("Processing file '{0}'.", path);

            var fileName = Path.GetFileNameWithoutExtension(path);

            switch (fileName)
            {

                case "comma":

                    Console.WriteLine("Comma delimited format detected");

                    ReadFile(path, Format.Comma);

                    break;

                case "pipe":

                    Console.WriteLine("Pipe delimited format detected");

                    ReadFile(path, Format.Pipe);

                    break;

                case "space":

                    Console.WriteLine("Space delimited format detected");

                    ReadFile(path, Format.Space);

                    break;

                default:

                    Console.WriteLine("Unknown file format detected");

                    break;
            }
        }

        private static void ReadFile(string path, Format format)
        {

            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {

                Console.WriteLine(line);

                string[] cells = new String[4];

                switch (format)
                {

                    case Format.Comma:

                        cells = line.Split(',');

                        break;
                    case Format.Pipe:

                        cells = line.Split('|');

                        break;
                    case Format.Space:

                        cells = line.Split(' ');

                        break;
                }

                if (cells.Length == 0)
                {
                    Console.WriteLine("No fields");
                }
                else if (cells.Length != 5)
                {
                    Console.WriteLine(@"Wrong number of fields {0}", cells.Length);
                }
                else
                {
                    OutputLine(cells);
                }

                Console.WriteLine();
                Console.WriteLine("**********");
                Console.WriteLine();
            }
        }

        private static void OutputLine(string[] cells)
        {

            Person MyPerson = new Person()
            {
                LastName = cells[0].ToString(),
                FirstName = cells[1].ToString(),
                Sex = cells[2].ToString(),
                FavoriteColor = cells[3].ToString(),
                BirthDate = DateTime.Parse(cells[4].ToString())
            };

            people.Add(MyPerson);
        }

        private static void PrintPeople(List<Person> people)
        {

            foreach (Person person in people)
            {
                PrintPerson(person);
            }

            Console.WriteLine();
        }

        private static void PrintPerson(Person person)
        {

            PropertyInfo[] properties = person.GetType().GetProperties();

            string line = string.Empty;

            foreach (PropertyInfo pi in properties)
            {

                var name = pi.Name;
                var value = pi.GetValue(person, null);

                if (pi.PropertyType == typeof(DateTime))
                    value = ((DateTime)value).ToString("M/d/yyyy");

                line += string.Format("{0}: {1}{2} ", name, value, ",");            
            }

            var myLine = line.Substring(0, (line.Length - 2));

            Console.WriteLine(myLine);
        }

        private static void WritePeopleToFile(List<Person> people)
        {

            string outputFile = Path.Combine(ConfigurationManager.AppSettings["OutputDirectory"], "people.json");

            string peopleString = JsonConvert.SerializeObject(people);

            File.WriteAllText(outputFile, peopleString);
        }
    }
}
