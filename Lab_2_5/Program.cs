using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Lab_2_4
{
    public enum Frequency { Щотижневий, Щомісячний, Щорічний }

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

        public override int GetHashCode()
        {
            return FirstName.GetHashCode() ^ LastName.GetHashCode() ^ BirthDate.GetHashCode();
        }

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

    public class Edition
    {
        protected string name;
        protected DateTime releaseDate;
        protected int circulation;

        public Edition(string name, DateTime releaseDate, int circulation)
        {
            this.name = name;
            this.releaseDate = releaseDate;
            this.circulation = circulation;
        }

        public Edition() : this("Невідоме видання", DateTime.Now, 1000) { }

        public string Name { get => name; set => name = value; }
        public DateTime ReleaseDate { get => releaseDate; set => releaseDate = value; }
        public int Circulation
        {
            get => circulation;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Тираж не може бути від'ємним. Допустимі значення >= 0.");
                circulation = value;
            }
        }

        public virtual object DeepCopy() => new Edition(name, releaseDate, circulation);

        public override bool Equals(object obj)
        {
            if (obj is Edition e)
                return name == e.name && releaseDate == e.releaseDate && circulation == e.circulation;
            return false;
        }

        public static bool operator ==(Edition left, Edition right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Edition left, Edition right) => !(left == right);

        public override int GetHashCode()
        {
            return name.GetHashCode() ^ releaseDate.GetHashCode() ^ circulation.GetHashCode();
        }

        public override string ToString() =>
            $"Видання: {name}, Дата випуску: {releaseDate:yyyy-MM-dd}, Тираж: {circulation}";
    }

    public class Magazine : Edition, IRateAndCopy, IEnumerable
    {
        private Frequency frequency;
        private ArrayList editors;
        private ArrayList articles;

        public Magazine(string name, Frequency frequency, DateTime releaseDate, int circulation)
            : base(name, releaseDate, circulation)
        {
            this.frequency = frequency;
            editors = new ArrayList();
            articles = new ArrayList();
        }

        public Magazine() : this("Невідомо", Frequency.Щомісячний, DateTime.Now, 1000) { }

        public Frequency Frequency { get => frequency; set => frequency = value; }
        public ArrayList Editors { get => editors; set => editors = value; }
        public ArrayList Articles { get => articles; set => articles = value; }

        public double Rating
        {
            get
            {
                if (articles.Count == 0) return 0.0;
                double sum = 0;
                foreach (Article a in articles) sum += a.Rating;
                return sum / articles.Count;
            }
        }
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

        public void AddArticles(params Article[] newArticles)
        {
            foreach (var a in newArticles) articles.Add(a);
        }

        public void AddEditors(params Person[] newEditors)
        {
            foreach (var e in newEditors) editors.Add(e);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Журнал: {name}, Періодичність: {frequency}, Дата випуску: {releaseDate:yyyy-MM-dd}, Тираж: {circulation}");
            sb.AppendLine("Редактори:");
            foreach (Person e in editors) sb.AppendLine($"  - {e}");
            sb.AppendLine("Статті:");
            foreach (Article a in articles) sb.AppendLine($"  - {a}");
            sb.AppendLine($"Середній рейтинг: {Rating:F2}");
            return sb.ToString();
        }

        public virtual string ToShortString() =>
            $"Журнал: {name}, Періодичність: {frequency}, Дата випуску: {releaseDate:yyyy-MM-dd}, Тираж: {circulation}, Сер. рейтинг: {Rating:F2}";

        public override object DeepCopy()
        {
            Magazine copy = new Magazine(name, frequency, releaseDate, circulation);
            foreach (Person e in editors) copy.editors.Add(e.DeepCopy());
            foreach (Article a in articles) copy.articles.Add(a.DeepCopy());
            return copy;
        }

        public IEnumerable<Article> GetArticlesWithRatingGreaterThan(double minRating)
        {
            foreach (Article a in articles)
                if (a.Rating > minRating) yield return a;
        }

        public IEnumerable<Article> GetArticlesWithTitleContaining(string str)
        {
            foreach (Article a in articles)
                if (a.Title.Contains(str)) yield return a;
        }

        public IEnumerable<Article> GetArticlesByEditors()
        {
            foreach (Article a in articles)
                foreach (Person e in editors)
                    if (a.Author == e)
                    {
                        yield return a;
                        break;
                    }
        }

        public IEnumerable<Person> GetEditorsWithoutArticles()
        {
            foreach (Person e in editors)
            {
                bool hasArticle = false;
                foreach (Article a in articles)
                    if (a.Author == e) { hasArticle = true; break; }
                if (!hasArticle) yield return e;
            }
        }

        public IEnumerator GetEnumerator() => new MagazineEnumerator(articles, editors);

        public class MagazineEnumerator : IEnumerator
        {
            private ArrayList articles;
            private ArrayList editors;
            private int currentIndex = -1;

            public MagazineEnumerator(ArrayList articles, ArrayList editors)
            {
                this.articles = articles;
                this.editors = editors;
            }

            public object Current => articles[currentIndex];

            public bool MoveNext()
            {
                currentIndex++;
                while (currentIndex < articles.Count)
                {
                    Article a = (Article)articles[currentIndex];
                    bool isEditor = false;
                    foreach (Person e in editors)
                        if (a.Author == e) { isEditor = true; break; }
                    if (!isEditor) return true;
                    currentIndex++;
                }
                return false;
            }

            public void Reset() => currentIndex = -1;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("=== Варіант 2 ===\n");

            Edition e1 = new Edition("Тест", new DateTime(2024, 1, 1), 1000);
            Edition e2 = new Edition("Тест", new DateTime(2024, 1, 1), 1000);
            Console.WriteLine($"Посилання однакові: {ReferenceEquals(e1, e2)}");
            Console.WriteLine($"Об'єкти рівні: {e1 == e2}");
            Console.WriteLine($"Хеш-коди: {e1.GetHashCode()} | {e2.GetHashCode()}\n");

            try
            {
                e1.Circulation = -10;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Виняток: {ex.Message}\n");
            }

            Magazine mag = new Magazine("Світ науки", Frequency.Щомісячний, new DateTime(2024, 3, 1), 20000);
            Person ed1 = new Person("Аліса", "Джонсон", new DateTime(1980, 1, 1));
            Person ed2 = new Person("Боб", "Сміт", new DateTime(1975, 6, 10));
            Person a1 = new Person("Чарлі", "Браун", new DateTime(1990, 5, 5));
            mag.AddEditors(ed1, ed2);
            mag.AddArticles(
                new Article(ed1, "Редакція про ШІ", 4.3),
                new Article(a1, "Квантова механіка", 4.8)
            );
            Console.WriteLine(mag);
            Console.WriteLine();

            Console.WriteLine("Властивість Edition:");
            Console.WriteLine(mag.Edition);
            Console.WriteLine();

            Magazine copy = (Magazine)mag.DeepCopy();
            mag.Name = "Змінено";
            Console.WriteLine("Оригінал змінено:");
            Console.WriteLine(mag.ToShortString());
            Console.WriteLine("Копія (не змінена):");
            Console.WriteLine(copy.ToShortString());
            Console.WriteLine();

            Console.WriteLine("Статті з рейтингом > 4.0:");
            foreach (var art in mag.GetArticlesWithRatingGreaterThan(4.0))
                Console.WriteLine($"  - {art}");
            Console.WriteLine();

            Console.WriteLine("Статті, що містять 'ШІ':");
            foreach (var art in mag.GetArticlesWithTitleContaining("ШІ"))
                Console.WriteLine($"  - {art}");
            Console.WriteLine();

            Console.WriteLine("Статті не редакторів:");
            foreach (Article a in mag)
                Console.WriteLine($"  - {a}");
            Console.WriteLine();

            Console.WriteLine("Статті редакторів:");
            foreach (var art in mag.GetArticlesByEditors())
                Console.WriteLine($"  - {art}");
            Console.WriteLine();

            Console.WriteLine("Редактори без статей:");
            foreach (var ed in mag.GetEditorsWithoutArticles())
                Console.WriteLine($"  - {ed}");
            Console.WriteLine();

            Console.WriteLine("=== Кінець тестів Варіанту 2 ===");
        }
    }
}