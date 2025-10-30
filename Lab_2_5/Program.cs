using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lab_2_5
{
    public enum Frequency { Weekly, Monthly, Yearly }

    public interface IRateAndCopy
    {
        double Rating { get; }
        object DeepCopy();
    }

    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }

        public Person(string firstName, string lastName, DateTime birthDate)
        {
            FirstName = firstName;
            LastName = lastName;
            BirthDate = birthDate;
        }

        public Person() : this("Невідомо", "Невідомо", DateTime.Now) { }

        public override string ToString() => $"{FirstName} {LastName}, народився {BirthDate:yyyy-MM-dd}";

        public override bool Equals(object obj)
        {
            if (obj is Person p)
                return FirstName == p.FirstName && LastName == p.LastName && BirthDate == p.BirthDate;
            return false;
        }

        public static bool operator ==(Person left, Person right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Person left, Person right) => !(left == right);

        public override int GetHashCode() => FirstName.GetHashCode() ^ LastName.GetHashCode() ^ BirthDate.GetHashCode();

        public virtual object DeepCopy() => new Person(FirstName, LastName, BirthDate);
    }

    public class Article : IRateAndCopy
    {
        public Person Author { get; set; }
        public string Title { get; set; }
        public double Rating { get; set; }

        public Article(Person author, string title, double rating)
        {
            Author = author;
            Title = title;
            Rating = rating;
        }

        public Article() : this(new Person(), "Без назви", 0.0) { }

        public override string ToString() => $"Стаття: {Title} від {Author}, Рейтинг: {Rating:F2}";

        public virtual object DeepCopy() => new Article((Person)Author.DeepCopy(), Title, Rating);
    }

    public class Edition : IComparable<Edition>, IComparer<Edition>
    {
        protected string name;
        protected DateTime releaseDate;
        protected int circulation;

        public Edition(string name, DateTime releaseDate, int circulation)
        {
            this.name = name;
            this.releaseDate = releaseDate;
            this.Circulation = circulation;
        }

        public Edition() : this("Невідоме видання", DateTime.Now, 1000) { }

        public string Name { get => name; set => name = value; }
        public DateTime ReleaseDate { get => releaseDate; set => releaseDate = value; }
        public int Circulation
        {
            get => circulation;
            set
            {
                if (value < 0) throw new ArgumentException("Тираж не може бути від'ємним.");
                circulation = value;
            }
        }

        public virtual object DeepCopy() => new Edition(name, releaseDate, circulation);

        public override string ToString() => $"Видання: {name}, Дата: {releaseDate:yyyy-MM-dd}, Тираж: {circulation}";

        public override bool Equals(object obj)
        {
            if (obj is Edition e)
                return name == e.name && releaseDate == e.releaseDate && circulation == e.circulation;
            return false;
        }

        public override int GetHashCode() => name.GetHashCode() ^ releaseDate.GetHashCode() ^ circulation.GetHashCode();

        public static bool operator ==(Edition left, Edition right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Edition left, Edition right) => !(left == right);

        public int CompareTo(Edition other)
        {
            if (other == null) return 1;
            return string.Compare(this.name, other.name, StringComparison.Ordinal);
        }

        public int Compare(Edition x, Edition y)
        {
            if (x == null || y == null) return 0;
            return DateTime.Compare(x.ReleaseDate, y.ReleaseDate);
        }
    }

    public class EditionCirculationComparer : IComparer<Edition>
    {
        public int Compare(Edition x, Edition y)
        {
            if (x == null || y == null) return 0;
            return x.Circulation.CompareTo(y.Circulation);
        }
    }

    public class Magazine : Edition, IRateAndCopy
    {
        private Frequency frequency;
        private List<Person> editors;
        private List<Article> articles;

        public Magazine(string name, Frequency frequency, DateTime releaseDate, int circulation)
            : base(name, releaseDate, circulation)
        {
            this.frequency = frequency;
            editors = new List<Person>();
            articles = new List<Article>();
        }

        public Magazine() : this("Невідомо", Frequency.Monthly, DateTime.Now, 1000) { }

        public Frequency Frequency { get => frequency; set => frequency = value; }
        public List<Person> Editors { get => editors; set => editors = value; }
        public List<Article> Articles { get => articles; set => articles = value; }

        public double Rating => articles.Count == 0 ? 0.0 : articles.Average(a => a.Rating);

        public Edition Edition
        {
            get => new Edition(name, releaseDate, circulation);
            set
            {
                name = value.Name;
                releaseDate = value.ReleaseDate;
                circulation = value.Circulation;
            }
        }

        public void AddArticles(params Article[] newArticles) => articles.AddRange(newArticles);

        public void AddEditors(params Person[] newEditors) => editors.AddRange(newEditors);

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Журнал: {name}, Періодичність: {frequency}, Дата: {releaseDate:yyyy-MM-dd}, Тираж: {circulation}");
            sb.AppendLine("Редактори:");
            foreach (Person e in editors) sb.AppendLine($"  - {e}");
            sb.AppendLine("Статті:");
            foreach (Article a in articles) sb.AppendLine($"  - {a}");
            sb.AppendLine($"Середній рейтинг: {Rating:F2}");
            return sb.ToString();
        }

        public string ToShortString() =>
            $"Журнал: {name}, Періодичність: {frequency}, Дата: {releaseDate:yyyy-MM-dd}, Тираж: {circulation}, Сер. рейтинг: {Rating:F2}, Редакторів: {editors.Count}, Статей: {articles.Count}";

        public override object DeepCopy()
        {
            Magazine copy = new Magazine(name, frequency, releaseDate, circulation);
            foreach (var e in editors) copy.editors.Add((Person)e.DeepCopy());
            foreach (var a in articles) copy.articles.Add((Article)a.DeepCopy());
            return copy;
        }
    }

    public class MagazineCollection
    {
        private List<Magazine> magazines = new List<Magazine>();

        public void AddDefaults()
        {
            var mag1 = new Magazine("Наука", Frequency.Monthly, DateTime.Now, 5000);
            mag1.AddArticles(
                new Article(new Person("Автор 1", "Прізвище", DateTime.Now.AddYears(-30)), "Стаття 1", 4.5)
            );

            var mag2 = new Magazine("Техніка", Frequency.Weekly, DateTime.Now.AddDays(-7), 2000);
            mag2.AddArticles(
                new Article(new Person("Автор 2", "Прізвище", DateTime.Now.AddYears(-25)), "Стаття 2", 4.2)
            );

            magazines.Add(mag1);
            magazines.Add(mag2);
        }


        public void AddMagazines(params Magazine[] newMags) => magazines.AddRange(newMags);

        public override string ToString() => string.Join("\n\n", magazines.Select(m => m.ToString()));

        public string ToShortString() => string.Join("\n", magazines.Select(m => m.ToShortString()));

        public void SortByName() => magazines.Sort();
        public void SortByReleaseDate() => magazines.Sort(new Edition());
        public void SortByCirculation() => magazines.Sort(new EditionCirculationComparer());

        public double MaxRating => magazines.Count == 0 ? 0.0 : magazines.Max(m => m.Rating);

        public IEnumerable<Magazine> MonthlyMagazines => magazines.Where(m => m.Frequency == Frequency.Monthly);

        public List<Magazine> RatingGroup(double value) => magazines.Where(m => m.Rating >= value).ToList();
    }

    public class TestCollections
    {
        private List<Edition> editions;
        private List<string> editionStrings;
        private Dictionary<Edition, Magazine> editionMagazine;
        private Dictionary<string, Magazine> stringMagazine;

        public TestCollections(int count)
        {
            editions = new List<Edition>();
            editionStrings = new List<string>();
            editionMagazine = new Dictionary<Edition, Magazine>();
            stringMagazine = new Dictionary<string, Magazine>();

            for (int i = 0; i < count; i++)
            {
                Magazine mag = GenerateMagazine(i);
                Edition ed = mag.Edition;

                editions.Add(ed);
                editionStrings.Add(ed.ToString());
                editionMagazine[ed] = mag;
                stringMagazine[ed.ToString()] = mag;
            }
        }

        public static Magazine GenerateMagazine(int index)
        {
            return new Magazine($"Журнал {index}", Frequency.Monthly, DateTime.Now.AddDays(index), 1000 + index * 100)
            {
                Editors = new List<Person> { new Person($"Редактор {index}", "Прізвище", DateTime.Now.AddYears(-30)) },
                Articles = new List<Article> { new Article(new Person($"Автор {index}", "Прізвище", DateTime.Now.AddYears(-25)), $"Стаття {index}", 3.0 + (index % 3)) }
            };
        }

        public void TestSearch(int index)
        {
            var first = editions.First();
            var middle = editions[editions.Count / 2];
            var last = editions.Last();
            var notFound = new Edition("Немає", DateTime.Now, 0);

            var testItems = new List<Edition> { first, middle, last, notFound };

            foreach (var item in testItems)
            {
                Stopwatch sw = Stopwatch.StartNew();
                editions.Contains(item);
                sw.Stop();
                Console.WriteLine($"List<Edition> contains {item.Name}: {sw.ElapsedTicks} ticks");

                sw.Restart();
                editionMagazine.ContainsKey(item);
                sw.Stop();
                Console.WriteLine($"Dictionary<Edition,Magazine> containsKey {item.Name}: {sw.ElapsedTicks} ticks");

                sw.Restart();
                editionMagazine.ContainsValue(editionMagazine.ContainsKey(item) ? editionMagazine[item] : null);
                sw.Stop();
                Console.WriteLine($"Dictionary<Edition,Magazine> containsValue {item.Name}: {sw.ElapsedTicks} ticks");
                Console.WriteLine();
            }
        }
    }

    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            MagazineCollection coll = new MagazineCollection();
            coll.AddDefaults();
            coll.AddMagazines(
                new Magazine("Світ науки", Frequency.Monthly, new DateTime(2024, 3, 1), 20000),
                new Magazine("Техно", Frequency.Weekly, new DateTime(2024, 2, 15), 15000)
            );

            Console.WriteLine("=== Повна інформація про колекцію ===");
            Console.WriteLine(coll.ToString());

            Console.WriteLine("\n=== Коротка інформація про колекцію ===");
            Console.WriteLine(coll.ToShortString());

            Console.WriteLine("\n=== Сортування за назвою ===");
            coll.SortByName();
            Console.WriteLine(coll.ToShortString());

            Console.WriteLine("\n=== Сортування за датою виходу ===");
            coll.SortByReleaseDate();
            Console.WriteLine(coll.ToShortString());

            Console.WriteLine("\n=== Сортування за тиражем ===");
            coll.SortByCirculation();
            Console.WriteLine(coll.ToShortString());

            Console.WriteLine($"\nМаксимальний рейтинг журналів: {coll.MaxRating:F2}");

            Console.WriteLine("\nЖурнали з періодичністю Monthly:");
            foreach (var m in coll.MonthlyMagazines)
                Console.WriteLine(m.ToShortString());

            Console.WriteLine("\nЖурнали з рейтингом >= 4.0:");
            foreach (var m in coll.RatingGroup(4.0))
                Console.WriteLine(m.ToShortString());

            Console.WriteLine("\n=== Тестування колекцій ===");
            TestCollections testColl = new TestCollections(10);
            testColl.TestSearch(0);
        }
    }
}
