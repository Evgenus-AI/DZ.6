using System;

// --- 3.1: перечисление видов счёта ---
enum AccountType
{
    Current,   // текущий (= 0)
    Savings    // сберегательный (= 1)
}

class BankAccount
{
    // --- 7.2: статическое поле, одно на весь класс (общий счётчик номеров) ---
    private static int nextNumber = 1;

    // --- 7.1: закрытые поля (camelCase, без подчёркивания, как в курсе) ---
    private int accountNumber;
    private decimal balance;      // decimal для денег: точнее, чем double
    private AccountType type;

    // --- 7.2: статический метод, увеличивает счётчик ---
    // Постфиксный ++: сначала вернётся текущее значение, потом счётчик вырастет
    private static int GenerateNumber()
    {
        return nextNumber++;
    }

    // --- 7.1: метод заполнения ---
    // Номер не передаём: он генерируется сам (7.2).
    // this.balance - поле объекта, balance - параметр метода. this их различает
    public void Fill(decimal balance, AccountType type)
    {
        this.accountNumber = GenerateNumber();
        this.balance = balance;
        this.type = type;
    }

    // --- 7.1: методы чтения (геттеры) ---
    public int GetAccountNumber() { return accountNumber; }
    public decimal GetBalance() { return balance; }
    public AccountType GetAccountType() { return type; }

    // --- 7.3: положить на счёт ---
    // Возвращает true, если операция прошла, и false, если сумма некорректна
    public bool Deposit(decimal amount)
    {
        if (amount <= 0)
            return false;

        balance += amount;
        return true;
    }

    // --- 7.3: снять со счёта ---
    // Сначала проверяем, можно ли снять, и только потом меняем баланс
    public bool Withdraw(decimal amount)
    {
        if (amount <= 0 || amount > balance)
            return false;         // денег не хватает - баланс не трогаем

        balance -= amount;
        return true;
    }

    // --- 7.1: вывод информации об объекте ---
    public void Print()
    {
        Console.WriteLine("Счёт №{0}, тип: {1}, баланс: {2}", accountNumber, type, balance);
    }
}

class Program
{
    static void Main()
    {
        // 7.1: создаём объект, заполняем, печатаем
        BankAccount acc1 = new BankAccount();
        acc1.Fill(1000m, AccountType.Current);

        // 7.2: второй счёт получит следующий номер автоматически
        BankAccount acc2 = new BankAccount();
        acc2.Fill(500m, AccountType.Savings);

        acc1.Print();   // Счёт №1, тип: Current, баланс: 1000
        acc2.Print();   // Счёт №2, тип: Savings, баланс: 500

        // 7.3: проверка снятия и пополнения
        Console.WriteLine(acc1.Withdraw(300m));    // True
        Console.WriteLine(acc1.Withdraw(5000m));   // False (не хватает денег)
        Console.WriteLine(acc1.Deposit(200m));     // True
        Console.WriteLine(acc1.Deposit(-50m));     // False (отрицательная сумма)

        acc1.Print();   // Счёт №1, тип: Current, баланс: 900
    }
}