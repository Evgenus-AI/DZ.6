using System;

class Building
{
    // Статическое поле: хранит номер, который выдадим следующему зданию.
    // Одно на весь класс, а не на каждый объект
    private static int nextNumber = 1;

    // Закрытые поля
    private int buildingNumber;   // уникальный номер (генерируется сам)
    private double height;        // высота здания в метрах (может быть дробной)
    private int floors;           // этажность
    private int apartments;       // всего квартир
    private int entrances;        // подъездов

    // Статический метод: выдаёт новый номер и увеличивает счётчик.
    // private, чтобы снаружи нельзя было "сжечь" номера
    private static int GenerateNumber()
    {
        return nextNumber++;      // сначала вернём текущее, потом +1
    }

    // Метод заполнения. Номер сюда не передаём: он генерируется программно
    public void Fill(double height, int floors, int apartments, int entrances)
    {
        this.buildingNumber = GenerateNumber();
        this.height = height;
        this.floors = floors;
        this.apartments = apartments;
        this.entrances = entrances;
    }

    // Методы получения значений полей (геттеры)
    public int GetBuildingNumber() { return buildingNumber; }
    public double GetHeight() { return height; }
    public int GetFloors() { return floors; }
    public int GetApartments() { return apartments; }
    public int GetEntrances() { return entrances; }

    // Высота одного этажа = высота здания / этажность.
    // Высота double, поэтому деление дробное, ничего не теряется
    public double GetFloorHeight()
    {
        if (floors == 0)
            return 0;             // защита от деления на ноль
        return height / floors;
    }

    // Квартир в одном подъезде = всего квартир / подъездов.
    // (double) - явное приведение: без него int / int даст целое
    // и отбросит дробную часть (например, 7 / 2 = 3, а не 3,5)
    public double GetApartmentsPerEntrance()
    {
        if (entrances == 0)
            return 0;
        return (double)apartments / entrances;
    }

    // Квартир на одном этаже = всего квартир / (подъезды * этажи).
    // Предполагаем, что каждый подъезд идёт по всем этажам
    public double GetApartmentsPerFloor()
    {
        if (entrances == 0 || floors == 0)
            return 0;
        return (double)apartments / (entrances * floors);
    }

    // Печать всей информации о здании
    public void Print()
    {
        Console.WriteLine("Здание №{0}", buildingNumber);
        Console.WriteLine("Высота: {0} м, этажей: {1}", height, floors);
        Console.WriteLine("Квартир: {0}, подъездов: {1}", apartments, entrances);
        // {0:F2} - вывести число с двумя знаками после запятой
        Console.WriteLine("Высота этажа: {0:F2} м", GetFloorHeight());
        Console.WriteLine("Квартир в подъезде: {0:F2}", GetApartmentsPerEntrance());
        Console.WriteLine("Квартир на этаже: {0:F2}", GetApartmentsPerFloor());
        Console.WriteLine();
    }
}

class Program
{
    static void Main()
    {
        Building b1 = new Building();
        b1.Fill(45.5, 15, 180, 3);

        // Второе здание автоматически получит номер 2
        Building b2 = new Building();
        b2.Fill(27, 9, 72, 2);

        b1.Print();
        b2.Print();
    }
}