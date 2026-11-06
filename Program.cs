// Лабораторная работа: ООП на C#
// Тема: «Типы сотрудников»
// Используется только то, что есть в курсе: классы, свойства, наследование,
// enum, кортежи, List, StringBuilder, switch/case, TryParse, try/catch.

using System;
using System.Collections.Generic;
using System.Text;

// ============================================================
// Перечисление (enum) — набор именованных констант.
// ============================================================
public enum Grade
{
    Junior,
    Middle,
    Senior
}

// ============================================================
// КЛАСС 1 (АБСТРАКТНЫЙ): Employee — «Сотрудник»
// Абстрактный класс нельзя создать через new — это «шаблон»,
// общая часть для всех типов сотрудников.
// ============================================================
public abstract class Employee
{
    // static — одно поле на ВЕСЬ класс (а не на каждый объект).
    // Нужно, чтобы выдавать каждому сотруднику уникальный Id.
    private static int _nextId = 1;

    // Приватные поля (инкапсуляция): снаружи до них не достучаться,
    // только через свойства, где мы можем проверить значение.
    private string _fullName = "";
    private string _department = "";
    private decimal _salary;

    // Список задач: кортеж (Title, Hours) — название и часы.
    private List<(string Title, int Hours)> _tasks = new List<(string Title, int Hours)>();

    // ---------- СВОЙСТВА ----------

    // Только чтение: задаётся один раз в конструкторе, потом менять нельзя.
    public int Id { get; }

    // Чтение и запись С ПРОВЕРКОЙ (вот зачем свойства, а не публичные поля).
    public string FullName
    {
        get { return _fullName; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("ФИО не может быть пустым.");
            _fullName = value.Trim();
        }
    }

    // Чтение и запись
    public string Department
    {
        get { return _department; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Отдел не может быть пустым.");
            _department = value.Trim();
        }
    }

    // Чтение и запись с проверкой: оклад должен быть положительным.
    // decimal — тип для денег (в отличие от double не копит ошибки округления).
    public decimal Salary
    {
        get { return _salary; }
        set
        {
            if (value <= 0)
                throw new ArgumentException("Оклад должен быть больше нуля.");
            _salary = value;
        }
    }

    // Только чтение и АБСТРАКТНОЕ: каждый наследник обязан сам сказать, кто он.
    public abstract string Position { get; }

    // Только чтение, виртуальное: по умолчанию 40 часов, наследники могут изменить.
    public virtual int WeeklyHours
    {
        get { return 40; }
    }

    // get публичный, set приватный: читать может любой, менять — только сам класс.
    public int CompletedTasks { get; private set; }

    // Вычисляемое свойство (без своего поля, считается на лету) — только чтение.
    public int TotalHours
    {
        get
        {
            int sum = 0;
            foreach (var task in _tasks)
                sum += task.Hours;
            return sum;
        }
    }

    public double WorkloadPercent
    {
        get
        {
            if (WeeklyHours == 0) return 0;
            return (double)TotalHours / WeeklyHours * 100;
        }
    }

    // Отдаём КОПИЮ списка: если вернуть сам _tasks, снаружи его можно было бы
    // менять в обход наших проверок (а это нарушение инкапсуляции).
    public List<(string Title, int Hours)> Tasks
    {
        get { return new List<(string Title, int Hours)>(_tasks); }
    }

    // ---------- КОНСТРУКТОР ----------
    // protected: вызвать его можно только из наследников (через : base(...)).
    protected Employee(string fullName, string department, decimal salary)
    {
        Id = _nextId;
        _nextId++;
        // Присваиваем через СВОЙСТВА, чтобы сработали проверки.
        FullName = fullName;
        Department = department;
        Salary = salary;
    }

    // ---------- МЕТОДЫ ----------

    // 1) АБСТРАКТНЫЙ метод: тела нет. Как считать премию — решает каждый наследник.
    public abstract decimal CalculateBonus();

    // 2) virtual — реализация по умолчанию, которую можно переопределить (override).
    //    Упрощённая формула: средний дневной заработок = оклад / 29,3.
    public virtual decimal CalculateVacationPay(int days)
    {
        if (days <= 0)
            throw new ArgumentException("Количество дней отпуска должно быть больше нуля.");
        return Math.Round(Salary / 29.3m * days, 2);
    }

    // 3) Выдать задачу. Возвращает true/false — удалось или нет.
    public virtual bool AssignTask(string title, int hours)
    {
        if (string.IsNullOrWhiteSpace(title) || hours <= 0)
        {
            Console.WriteLine($"   ✗ {FullName}: некорректная задача.");
            return false;
        }
        if (TotalHours + hours > WeeklyHours)
        {
            Console.WriteLine($"   ✗ {FullName}: не влезает «{title}» ({hours} ч). " +
                              $"Занято {TotalHours} из {WeeklyHours} ч.");
            return false;
        }
        _tasks.Add((title, hours));
        Console.WriteLine($"   ✓ {FullName} получил(а) задачу «{title}» ({hours} ч).");
        return true;
    }

    // 4) Завершить задачу. Не virtual — у всех работает одинаково.
    public bool CompleteTask(string title)
    {
        for (int i = 0; i < _tasks.Count; i++)
        {
            if (_tasks[i].Title == title)
            {
                _tasks.RemoveAt(i);
                CompletedTasks++;   // можно, мы внутри класса (private set)
                return true;
            }
        }
        return false;
    }

    // 5) Показать нагрузку. virtual, чтобы наследники могли дописать своё.
    public virtual void ShowWorkload()
    {
        int filled = (int)Math.Round(WorkloadPercent / 10);
        if (filled > 10) filled = 10;
        string bar = new string('#', filled) + new string('.', 10 - filled);
        Console.WriteLine($"   {FullName} ({Position}): {TotalHours}/{WeeklyHours} ч [{bar}] {WorkloadPercent:F0}%");
        foreach (var task in _tasks)
            Console.WriteLine($"      - {task.Title} ({task.Hours} ч)");
    }

    // 6) Краткая информация о сотруднике.
    public virtual string GetInfo()
    {
        return $"#{Id} {FullName}, {Position}, отдел «{Department}», оклад {Money(Salary)}";
    }

    // 7) Повысить оклад на процент.
    public void RaiseSalary(decimal percent)
    {
        if (percent <= 0)
            throw new ArgumentException("Процент должен быть больше нуля.");
        Salary = Math.Round(Salary * (1 + percent / 100m), 2);
    }

    // 8) Строка для отчёта по отделам. PadRight/PadLeft дополняют строку пробелами до нужной ширины.
    public string GetReportLine()
    {
        return FullName.PadRight(28) + " " + Position.PadRight(12) + " " +
               Money(Salary).PadLeft(12) + " " + WorkloadPercent.ToString("F0").PadLeft(4) + "%";
    }

    // Статический помощник: форматирует деньги. Вызывается как Employee.Money(...)
    public static string Money(decimal value)
    {
        return value.ToString("N0") + " ₽";
    }
}

// ============================================================
// КЛАСС 2: Manager — «Менеджер» (наследник Employee)
// ============================================================
public class Manager : Employee
{
    private List<Employee> _team = new List<Employee>();
    private int _meetingHoursPerWeek;

    public override string Position
    {
        get { return "Менеджер"; }
    }

    // Чтение и запись с проверкой
    public int MeetingHoursPerWeek
    {
        get { return _meetingHoursPerWeek; }
        set
        {
            if (value < 0 || value > 30)
                throw new ArgumentException("Часы встреч: от 0 до 30.");
            _meetingHoursPerWeek = value;
        }
    }

    // Совещания «съедают» рабочее время менеджера -> меньше часов на задачи.
    public override int WeeklyHours
    {
        get { return base.WeeklyHours - MeetingHoursPerWeek; }
    }

    // Только чтение
    public int TeamSize
    {
        get { return _team.Count; }
    }

    // Копия списка — по той же причине, что и у Tasks
    public List<Employee> Team
    {
        get { return new List<Employee>(_team); }
    }

    // КОНСТРУКТОР 1: базовый (встречи по умолчанию 10 ч)
    // : base(...) — вызываем конструктор родителя, он заполнит ФИО, отдел, оклад.
    public Manager(string fullName, string department, decimal salary)
        : base(fullName, department, salary)
    {
        MeetingHoursPerWeek = 10;
    }

    // КОНСТРУКТОР 2: с часами встреч. : this(...) — вызываем конструктор 1,
    // чтобы не дублировать код.
    public Manager(string fullName, string department, decimal salary, int meetingHours)
        : this(fullName, department, salary)
    {
        MeetingHoursPerWeek = meetingHours;
    }

    // Премия: 20% + 2% за каждого подчинённого, но не больше 40%.
    public override decimal CalculateBonus()
    {
        decimal rate = 0.20m + 0.02m * TeamSize;
        if (rate > 0.40m) rate = 0.40m;
        return Math.Round(Salary * rate, 2);
    }

    // Отпускные: считаем как у всех (base), плюс 10% надбавка (упрощённая учебная модель).
    public override decimal CalculateVacationPay(int days)
    {
        return Math.Round(base.CalculateVacationPay(days) * 1.10m, 2);
    }

    public override string GetInfo()
    {
        return base.GetInfo() + $", подчинённых: {TeamSize}";
    }

    public override void ShowWorkload()
    {
        base.ShowWorkload();   // сначала общая часть из родителя
        Console.WriteLine($"      (совещания: {MeetingHoursPerWeek} ч/нед, подчинённых: {TeamSize})");
    }

    public void AddSubordinate(Employee employee)
    {
        if (employee == this) return;            // сам себе не подчиняется
        if (!_team.Contains(employee))
            _team.Add(employee);
    }

    // ПОЛИМОРФИЗМ: параметр типа Employee принимает хоть Developer, хоть Intern.
    // Какая именно версия AssignTask сработает — решится во время работы программы.
    public bool DelegateTask(Employee worker, string title, int hours)
    {
        Console.WriteLine($"   {FullName} поручает задачу сотруднику {worker.FullName}:");
        return worker.AssignTask(title, hours);
    }

    public void ShowTeam()
    {
        Console.WriteLine($"   Команда менеджера {FullName}:");
        foreach (var member in _team)
            Console.WriteLine($"      - {member.FullName} ({member.Position})");
    }
}

// ============================================================
// КЛАСС 3: Developer — «Разработчик» (наследник Employee)
// ============================================================
public class Developer : Employee
{
    public override string Position
    {
        get { return "Разработчик"; }
    }

    // Чтение и запись
    public string Language { get; set; }

    // Чтение и запись (enum)
    public Grade Grade { get; set; }

    // КОНСТРУКТОР 1: по умолчанию Junior
    public Developer(string fullName, string department, decimal salary, string language)
        : base(fullName, department, salary)
    {
        Language = language;
        Grade = Grade.Junior;
    }

    // КОНСТРУКТОР 2: с уровнем
    public Developer(string fullName, string department, decimal salary, string language, Grade grade)
        : this(fullName, department, salary, language)
    {
        Grade = grade;
    }

    // Процент премии зависит от уровня — выбираем через switch.
    public override decimal CalculateBonus()
    {
        decimal rate = 0m;
        switch (Grade)
        {
            case Grade.Junior:
                rate = 0.10m;
                break;
            case Grade.Middle:
                rate = 0.15m;
                break;
            case Grade.Senior:
                rate = 0.25m;
                break;
            default:
                rate = 0m;
                break;
        }
        // Плюс 1000 ₽ за каждую закрытую задачу.
        return Math.Round(Salary * rate + 1000m * CompletedTasks, 2);
    }

    public override string GetInfo()
    {
        return base.GetInfo() + $", язык: {Language}, уровень: {Grade}";
    }

    public override void ShowWorkload()
    {
        base.ShowWorkload();
        Console.WriteLine($"      (стек: {Language}, уровень {Grade})");
    }

    public void WriteCode(string feature)
    {
        Console.WriteLine($"   {FullName} пишет «{feature}» на {Language}...");
    }

    // Принимаем другого Developer: ревью кода — типичная задача разработчика.
    public void DoCodeReview(Developer author)
    {
        if (author == this)
            Console.WriteLine($"   {FullName}: сам себе ревью не делают 🙂");
        else
            Console.WriteLine($"   {FullName} делает code review для {author.FullName}.");
    }

    // Повышение уровня: Junior -> Middle -> Senior (+15% к окладу).
    public bool Promote()
    {
        if (Grade == Grade.Senior) return false;

        if (Grade == Grade.Junior)
            Grade = Grade.Middle;
        else
            Grade = Grade.Senior;

        RaiseSalary(15);
        return true;
    }
}

// ============================================================
// КЛАСС 4: Intern — «Стажёр» (наследник Employee)
// ============================================================
public class Intern : Employee
{
    private int _internshipMonths;

    public override string Position
    {
        get { return "Стажёр"; }
    }

    // Наставник — любой Employee (опять полиморфизм). Может быть null — тогда наставника нет.
    public Employee Mentor { get; set; }

    // Чтение и запись с проверкой
    public int InternshipMonths
    {
        get { return _internshipMonths; }
        set
        {
            if (value < 1 || value > 12)
                throw new ArgumentException("Срок стажировки: от 1 до 12 месяцев.");
            _internshipMonths = value;
        }
    }

    // У стажёра неполный день.
    public override int WeeklyHours
    {
        get { return 20; }
    }

    // КОНСТРУКТОР 1: срок по умолчанию — 3 месяца
    public Intern(string fullName, string department, decimal salary, Employee mentor)
        : base(fullName, department, salary)
    {
        Mentor = mentor;
        InternshipMonths = 3;
    }

    // КОНСТРУКТОР 2: со сроком
    public Intern(string fullName, string department, decimal salary, Employee mentor, int months)
        : this(fullName, department, salary, mentor)
    {
        InternshipMonths = months;
    }

    // Премия небольшая: 1000 ₽ + 1000 ₽ за каждую закрытую задачу, но не больше 5000 ₽.
    public override decimal CalculateBonus()
    {
        decimal bonus = 1000m + 1000m * CompletedTasks;
        if (bonus > 5000m) bonus = 5000m;
        return bonus;
    }

    // Оплачиваем максимум 7 дней отпуска (учебное правило для стажёров).
    public override decimal CalculateVacationPay(int days)
    {
        if (days > 7) days = 7;
        return base.CalculateVacationPay(days);
    }

    // Стажёру нельзя давать крупные задачи (больше 8 часов).
    public override bool AssignTask(string title, int hours)
    {
        if (hours > 8)
        {
            Console.WriteLine($"   ✗ {FullName}: стажёру нельзя задачу «{title}» на {hours} ч (максимум 8 ч).");
            return false;
        }
        return base.AssignTask(title, hours);   // остальные проверки — как у всех
    }

    public override string GetInfo()
    {
        string mentorName = "не назначен";
        if (Mentor != null)
            mentorName = Mentor.FullName;
        return base.GetInfo() + $", наставник: {mentorName}, срок: {InternshipMonths} мес.";
    }

    public void AskMentor(string question)
    {
        if (Mentor == null)
            Console.WriteLine($"   {FullName}: наставника нет, спросить некого.");
        else
            Console.WriteLine($"   {FullName} спрашивает {Mentor.FullName}: «{question}»");
    }

    public void ExtendInternship(int months)
    {
        int total = InternshipMonths + months;
        if (total > 12) total = 12;
        InternshipMonths = total;
    }

    // Готов к найму, если закрыл хотя бы 2 задачи.
    public bool IsReadyForOffer()
    {
        return CompletedTasks >= 2;
    }
}

// ============================================================
// КЛАСС 5: Company — «Компания» (хранит сотрудников, делает отчёт по отделам)
// Это композиция: компания ВКЛЮЧАЕТ в себя сотрудников.
// ============================================================
public class Company
{
    private List<Employee> _employees = new List<Employee>();
    private string _name;

    // Чтение и запись
    public string Name
    {
        get { return _name; }
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
                _name = value.Trim();
        }
    }

    // Только чтение
    public int Count
    {
        get { return _employees.Count; }
    }

    public List<Employee> Employees
    {
        get { return new List<Employee>(_employees); }
    }

    public decimal TotalPayroll
    {
        get
        {
            decimal sum = 0;
            foreach (var e in _employees)
                sum += e.Salary;
            return sum;
        }
    }

    public Company(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            _name = "Компания";
        else
            _name = name.Trim();
    }

    public void Hire(Employee employee)
    {
        _employees.Add(employee);
    }

    public bool Fire(int id)
    {
        Employee found = FindById(id);
        if (found == null) return false;
        return _employees.Remove(found);
    }

    public Employee FindById(int id)
    {
        foreach (var e in _employees)
            if (e.Id == id) return e;
        return null;
    }

    public List<string> GetDepartments()
    {
        var result = new List<string>();
        foreach (var e in _employees)
            if (!result.Contains(e.Department))
                result.Add(e.Department);
        return result;
    }

    public decimal TotalBonuses()
    {
        decimal sum = 0;
        foreach (var e in _employees)
            sum += e.CalculateBonus();   // полиморфизм: у каждого своя формула
        return sum;
    }

    // Отчёт по отделам
    public string BuildDepartmentReport()
    {
        var sb = new StringBuilder();   // StringBuilder — эффективная сборка длинной строки
        sb.AppendLine("ОТЧЁТ ПО ОТДЕЛАМ — " + Name);
        sb.AppendLine(new string('=', 62));

        foreach (string dept in GetDepartments())
        {
            sb.AppendLine("Отдел «" + dept + "»");
            sb.AppendLine("ФИО".PadRight(28) + " " + "Должность".PadRight(12) + " " +
                          "Оклад".PadLeft(12) + " " + "Загр.".PadLeft(5));

            decimal payroll = 0;
            decimal bonuses = 0;
            double loadSum = 0;
            int count = 0;

            foreach (var e in _employees)
            {
                if (e.Department != dept) continue;
                sb.AppendLine(e.GetReportLine());
                payroll += e.Salary;
                bonuses += e.CalculateBonus();
                loadSum += e.WorkloadPercent;
                count++;
            }

            sb.AppendLine($"   Сотрудников: {count}, фонд оклада: {Employee.Money(payroll)}, " +
                          $"премии: {Employee.Money(bonuses)}, средняя загрузка: {loadSum / count:F0}%");
            sb.AppendLine(new string('-', 62));
        }

        sb.AppendLine($"ИТОГО: {Count} чел., фонд оклада {Employee.Money(TotalPayroll)}");
        return sb.ToString();
    }
}

// ============================================================
// ТОЧКА ВХОДА
// ============================================================
public class Program
{
    // Номер задачи — поменяй на свой, как в твоём задании.
    private const string TaskNumber = "1";

    public static void Main()
    {
        // Чтобы русские буквы и символы ✓ ✗ ₽ нормально отображались в консоли
        Console.OutputEncoding = Encoding.UTF8;

        PrintHeader();

        // ---------- ШАГ 1: создаём объекты (данные прописаны в коде) ----------
        PrintStep("ШАГ 1. Создаём компанию и сотрудников",
            "Данные о людях прописаны прямо в коде. Каждый тип сотрудника создаётся своим конструктором.");

        var company = new Company("ООО «ТехноСофт»");

        // Конструктор Manager №1 (3 параметра) и №2 (с часами встреч)
        var ivanov = new Manager("Иванов Пётр Сергеевич", "Разработка", 200000m);
        var smirnova = new Manager("Смирнова Ольга Андреевна", "Мобильная разработка", 190000m, 6);

        // Конструктор Developer №1 (Junior по умолчанию) и №2 (с уровнем)
        var petrova = new Developer("Петрова Анна Игоревна", "Разработка", 180000m, "C#", Grade.Senior);
        var sidorov = new Developer("Сидоров Максим Олегович", "Разработка", 120000m, "Python");
        var orlov = new Developer("Орлов Дмитрий Павлович", "Мобильная разработка", 150000m, "Kotlin", Grade.Middle);

        // Конструктор Intern №1 (3 месяца) и №2 (со сроком)
        var kozlov = new Intern("Козлов Артём Викторович", "Разработка", 40000m, petrova);
        var belova = new Intern("Белова Мария Романовна", "Мобильная разработка", 35000m, orlov, 6);

        // Все — в один массив. Тип массива Employee, но лежат разные потомки.
        Employee[] all = { ivanov, smirnova, petrova, sidorov, orlov, kozlov, belova };
        foreach (var e in all)
            company.Hire(e);

        // Подчинённые менеджеров
        ivanov.AddSubordinate(petrova);
        ivanov.AddSubordinate(sidorov);
        ivanov.AddSubordinate(kozlov);
        smirnova.AddSubordinate(orlov);
        smirnova.AddSubordinate(belova);

        foreach (var e in company.Employees)
            Console.WriteLine("   " + e.GetInfo());
        Pause();

        // ---------- ШАГ 2: инкапсуляция ----------
        PrintStep("ШАГ 2. ИНКАПСУЛЯЦИЯ",
            "Данные спрятаны в приватных полях. Менять их можно только через свойства, а там стоят проверки.");

        Console.WriteLine($"   Id только для чтения: у {petrova.FullName} Id = {petrova.Id} (изменить нельзя, код не скомпилируется).");
        Console.WriteLine($"   Оклад {sidorov.FullName} сейчас: {Employee.Money(sidorov.Salary)}");
        try
        {
            Console.WriteLine("   Пробуем поставить оклад = -5000...");
            sidorov.Salary = -5000m;   // сработает проверка в set
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"   ✗ Отказ: {ex.Message}");
        }
        sidorov.Salary = 130000m;       // нормальное значение пройдёт
        Console.WriteLine($"   Корректное значение принято, оклад: {Employee.Money(sidorov.Salary)}");
        Pause();

        // ---------- ШАГ 3: абстракция + наследование ----------
        PrintStep("ШАГ 3. АБСТРАКЦИЯ и НАСЛЕДОВАНИЕ",
            "Employee — абстрактный: «просто сотрудника» не бывает, бывает менеджер, разработчик или стажёр.\n" +
            "   Все трое наследуют ФИО, отдел, оклад и общие методы от Employee.");

        // var x = new Employee(...);  // ОШИБКА КОМПИЛЯЦИИ: нельзя создать экземпляр абстрактного класса
        Console.WriteLine("   Employee нельзя создать через new — компилятор не даст (строка закомментирована в коде).");
        Console.WriteLine($"   {ivanov.FullName} -> {ivanov.Position}");
        Console.WriteLine($"   {petrova.FullName} -> {petrova.Position}");
        Console.WriteLine($"   {kozlov.FullName} -> {kozlov.Position}");
        Console.WriteLine("   Общее (ФИО, отдел, оклад, задачи) — в родителе, особенное (команда, язык, наставник) — в наследниках.");
        Pause();

        // ---------- ШАГ 4: полиморфизм ----------
        PrintStep("ШАГ 4. ПОЛИМОРФИЗМ: «начислить премию»",
            "Для каждого сотрудника вызываем ОДИН И ТОТ ЖЕ метод CalculateBonus(),\n" +
            "   а считается он по-разному — зависит от реального типа объекта.");

        foreach (var e in all)
            Console.WriteLine($"   {e.FullName.PadRight(28)} {e.Position.PadRight(12)} премия: {Employee.Money(e.CalculateBonus())}");
        Console.WriteLine($"   Всего на премии: {Employee.Money(company.TotalBonuses())}");
        Pause();

        // ---------- ШАГ 5: выдача задач и нагрузка ----------
        PrintStep("ШАГ 5. Выдаём задачи и смотрим нагрузку",
            "Менеджер делегирует задачи. У стажёра ограничение по размеру задачи, у всех — по часам в неделю.");

        ivanov.DelegateTask(petrova, "Спроектировать API", 16);
        ivanov.DelegateTask(petrova, "Миграция БД", 14);
        ivanov.DelegateTask(petrova, "Рефакторинг модуля оплаты", 20);   // не влезет (16+14+20 > 40)
        ivanov.DelegateTask(sidorov, "Скрипт выгрузки отчётов", 12);
        ivanov.DelegateTask(kozlov, "Написать unit-тесты", 6);
        ivanov.DelegateTask(kozlov, "Переписать сервис авторизации", 30); // стажёру нельзя
        smirnova.DelegateTask(orlov, "Экран профиля", 18);
        smirnova.DelegateTask(belova, "Вёрстка списка заказов", 8);
        ivanov.AssignTask("Планирование спринта", 12);                    // задача самому менеджеру

        Console.WriteLine();
        petrova.WriteCode("API заказов");
        sidorov.DoCodeReview(petrova);
        kozlov.AskMentor("Как правильно называть тесты?");
        kozlov.CompleteTask("Написать unit-тесты");
        Console.WriteLine($"   {kozlov.FullName} закрыл задачу. Закрыто задач: {kozlov.CompletedTasks}");

        Console.WriteLine();
        Console.WriteLine("   Нагрузка (ShowWorkload — у каждого типа свой вывод):");
        foreach (var e in all)
            e.ShowWorkload();
        Console.WriteLine();
        ivanov.ShowTeam();
        Pause();

        // ---------- ШАГ 6: отпускные (ввод с клавиатуры) ----------
        PrintStep("ШАГ 6. Рассчитать отпускные",
            "ТВОЁ ДЕЙСТВИЕ: введи число дней отпуска и нажми Enter (если ввести ерунду или ничего — возьмём 14).");

        Console.Write("   Дней отпуска: ");
        string input = Console.ReadLine();
        // TryParse не падает на плохом вводе, а возвращает false.
        int days;
        if (!int.TryParse(input, out days) || days <= 0)
        {
            days = 14;
            Console.WriteLine("   Не разобрал число, берём 14 дней.");
        }

        Console.WriteLine($"   Отпускные за {days} дн.:");
        foreach (var e in all)
            Console.WriteLine($"   {e.FullName.PadRight(28)} {e.Position.PadRight(12)} {Employee.Money(e.CalculateVacationPay(days))}");
        Console.WriteLine("   (у менеджера есть надбавка +10%, у стажёра оплачивается максимум 7 дней)");
        Pause();

        // ---------- ШАГ 7: изменения состояния ----------
        PrintStep("ШАГ 7. Свойства на чтение и запись в действии",
            "Меняем свойства и смотрим, как это влияет на методы.");

        Console.WriteLine($"   Было: {sidorov.FullName}, уровень {sidorov.Grade}, оклад {Employee.Money(sidorov.Salary)}, премия {Employee.Money(sidorov.CalculateBonus())}");
        sidorov.Promote();   // Junior -> Middle, +15% к окладу
        Console.WriteLine($"   Стало (Promote): уровень {sidorov.Grade}, оклад {Employee.Money(sidorov.Salary)}, премия {Employee.Money(sidorov.CalculateBonus())}");

        Console.WriteLine($"   Совещания {ivanov.FullName}: {ivanov.MeetingHoursPerWeek} ч, доступно часов: {ivanov.WeeklyHours}");
        ivanov.MeetingHoursPerWeek = 4;
        Console.WriteLine($"   После сокращения совещаний до 4 ч доступно часов: {ivanov.WeeklyHours}");

        kozlov.CompleteTask("Написать unit-тесты");   // уже закрыта -> false
        string offerText = "нет, закрыто мало задач";
        if (kozlov.IsReadyForOffer())
            offerText = "да";
        Console.WriteLine($"   Стажёр {kozlov.FullName} готов к офферу? {offerText}");
        kozlov.ExtendInternship(2);
        Console.WriteLine($"   Стажировку продлили, срок: {kozlov.InternshipMonths} мес.");
        Pause();

        // ---------- ШАГ 8: отчёт по отделам ----------
        PrintStep("ШАГ 8. Отчёт по отделам",
            "Компания сама собирает всё воедино: оклады, премии и среднюю загрузку по каждому отделу.");

        Console.WriteLine(company.BuildDepartmentReport());

        // ---------- ИТОГ ----------
        PrintPrinciplesSummary();
    }

    // ---------- Вспомогательные методы вывода ----------

    private static void PrintHeader()
    {
        Console.WriteLine("==============================================================");
        Console.WriteLine($" Задача № {TaskNumber}");
        Console.WriteLine(" Тема: «Типы сотрудников»");
        Console.WriteLine("==============================================================");
        Console.WriteLine(" Что делает программа:");
        Console.WriteLine("   Показывает все 4 принципа ООП на примере иерархии сотрудников:");
        Console.WriteLine("   абстрактный Employee и наследники Manager, Developer, Intern,");
        Console.WriteLine("   плюс класс Company, который строит отчёт по отделам.");
        Console.WriteLine(" Что нужно от пользователя:");
        Console.WriteLine("   - нажимать Enter, чтобы переходить к следующему шагу;");
        Console.WriteLine("   - на шаге 6 ввести число дней отпуска.");
        Console.WriteLine("==============================================================");
        Console.WriteLine();
    }

    private static void PrintStep(string title, string description)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {title} ---");
        Console.WriteLine($"   {description}");
        Console.WriteLine();
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("   [Enter — дальше] ");
        Console.ReadLine();
    }

    private static void PrintPrinciplesSummary()
    {
        Console.WriteLine("==============================================================");
        Console.WriteLine(" ИТОГ: где в программе какой принцип ООП");
        Console.WriteLine("==============================================================");
        Console.WriteLine(" 1. Инкапсуляция — приватные поля + свойства с проверками");
        Console.WriteLine("    (Salary, FullName, MeetingHoursPerWeek, private set у CompletedTasks).");
        Console.WriteLine(" 2. Наследование — Manager, Developer, Intern берут всё общее от Employee");
        Console.WriteLine("    и вызывают его конструктор через : base(...).");
        Console.WriteLine(" 3. Полиморфизм — один вызов CalculateBonus() / AssignTask() / ShowWorkload()");
        Console.WriteLine("    работает по-разному для разных типов (virtual + override).");
        Console.WriteLine(" 4. Абстракция — Employee нельзя создать; он задаёт «контракт»:");
        Console.WriteLine("    abstract Position и abstract CalculateBonus() обязан реализовать каждый наследник.");
        Console.WriteLine("==============================================================");
    }
}