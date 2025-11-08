using System;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json;

public class Plant
{
    private static int _count = 0;
    private static double _totalHeight = 0;

    public static int Count => _count;
    public static double AverageHeight => _count > 0 ? _totalHeight / _count : 0;

    private string _name;
    private int _age;
    private double _height;
    private PlantType _type;
    private DateTime _plantingDate;

    [JsonProperty("Назва")]
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Назва не може бути порожньою.");
            if (value.Length < 2 || value.Length > 50)
                throw new ArgumentException("Назва повинна містити від 2 до 50 символів.");
            if (!value.All(c => char.IsLetter(c) || c == ' '))
                throw new ArgumentException("Назва може містити лише літери та пробіли.");
            _name = value;
        }
    }

    [JsonProperty("Тип")]
    public PlantType Type
    {
        get => _type;
        set
        {
            if (!Enum.IsDefined(typeof(PlantType), value))
                throw new ArgumentException("Невірний тип рослини.");
            _type = value;
        }
    }


    [JsonProperty("Вік")]
    public int Age
    {
        get => _age;
        set
        {
            if (value < 0 || value > 5000)
                throw new ArgumentException("Вік повинен бути в діапазоні від 0 до 5000 років.");
            _age = value;
        }
    }

    [JsonProperty("Висота")]
    public double Height
    {
        get => _height;
        set
        {
            if (value <= 0 || value > 115.7)
                throw new ArgumentException("Висота повинна бути в діапазоні від 0 до 115.7 м (рекорд Hyperion).");

            _totalHeight -= _height;
            _height = value;
            _totalHeight += _height;
        }
    }

    [JsonIgnore] // Не серіалізуємо дату посадки у JSON
    public DateTime PlantingDate
    {
        get => _plantingDate;
        set
        {
            ValidatePlantingDate(value);
            _plantingDate = value;
        }
    }

    [JsonProperty("Цвітуча")]
    public bool IsFlowering { get; set; } = true;


    [JsonIgnore] // Не серіалізуємо похідні властивості
    public string AgeCategory
    {
        get
        {
            if (Age < 2) return "Молода";
            if (Age < 10) return "Доросла";
            return "Стара";
        }
    }


    [JsonIgnore] // Не серіалізуємо службові властивості
    public string LastWatered { get; private set; } = "Ніколи";

    public Plant(string name, PlantType type, int age, double height, DateTime plantingDate)
    {
        Console.WriteLine("Викликано основний конструктор з 5 параметрами");
        _name = name;
        _type = type;
        _age = age;
        _height = height;
        _plantingDate = plantingDate;
    }

    public Plant()
    {
        Console.WriteLine("Викликано конструктор без параметрів");
        _name = "Без назви";
        _type = PlantType.Flower;
        _age = 1;
        _height = 0.1;
        _plantingDate = DateTime.Now;
        IsFlowering = true;
    }

    public Plant(string name, PlantType type) : this(name, type, 1, 0.5, DateTime.Now)
    {
        Console.WriteLine("Викликано конструктор з 2 параметрами (ім'я та тип)");
    }

    public Plant(string name, int age) : this(name, PlantType.Tree, age, 1.0, DateTime.Now.AddYears(-age))
    {
        Console.WriteLine("Викликано конструктор з 2 параметрами (ім'я та вік)");
    }

    public static void AddPlantToCollection(Plant plant)
    {
        if (plant != null)
        {
            _count++;
            _totalHeight += plant.Height;
        }
    }

    public static void RemovePlantFromCollection(Plant plant)
    {
        if (plant != null && _count > 0)
        {
            _count--;
            _totalHeight -= plant.Height;
        }
    }

    public static string GetGlobalPlantInfo()
    {
        return $"Всього рослин: {Count}, Середня висота: {AverageHeight:F2} м";
    }

    public static Plant Parse(string s)
    {
        if (string.IsNullOrEmpty(s))
            throw new ArgumentNullException(null, "Рядок не може бути порожнім або null");

        string[] parts = s.Split(',');

        if (parts.Length != 6)
            throw new FormatException("Рядок повинен містити 6 значень розділених комами");

        try
        {
            string name = parts[0];
            PlantType type = (PlantType)Enum.Parse(typeof(PlantType), parts[1]);
            int age = int.Parse(parts[2]);
            double height = double.Parse(parts[3], CultureInfo.InvariantCulture);
            DateTime plantingDate = DateTime.Parse(parts[4]);
            bool isFlowering = bool.Parse(parts[5]);

            return new Plant(name, type, age, height, plantingDate)
            {
                IsFlowering = isFlowering
            };
        }
        catch (Exception ex)
        {
            throw new FormatException($"Помилка парсингу: {ex.Message}");
        }
    }

    public static bool TryParse(string s, out Plant plant)
    {
        plant = null;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        string[] parts = s.Split(',');

        if (parts.Length != 6)
            return false;

        try
        {
            string name = parts[0].Trim();
            // Перевірка на пусту назву
            if (string.IsNullOrWhiteSpace(name))
                return false;

            PlantType type;
            if (!Enum.TryParse(parts[1].Trim(), out type))
                return false;

            int age;
            if (!int.TryParse(parts[2].Trim(), out age))
                return false;

            double height;
            if (!double.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height))
                return false;

            DateTime plantingDate;
            if (!DateTime.TryParse(parts[4].Trim(), out plantingDate))
                return false;

            bool isFlowering;
            if (!bool.TryParse(parts[5].Trim(), out isFlowering))
                return false;

            plant = new Plant(name, type, age, height, plantingDate)
            {
                IsFlowering = isFlowering
            };
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public override string ToString()
    {
        return $"{Name},{Type},{Age},{Height.ToString(CultureInfo.InvariantCulture)},{PlantingDate:yyyy-MM-dd},{IsFlowering}";
    }

    private void ValidatePlantingDate(DateTime date)
    {
        if (date.Year < 1900)
            throw new ArgumentException("Дата посадки не може бути раніше 1900 року.");

        if (date.Date > DateTime.Today)
            throw new ArgumentException("Дата посадки не може бути у майбутньому.");
    }

    private string FormatWateringTime(DateTime time)
    {
        return time.ToString("dd.MM.yyyy HH:mm");
    }

    public void WaterPlant()
    {
        DateTime wateringTime = DateTime.Now;
        LastWatered = FormatWateringTime(wateringTime);
        Console.WriteLine($"{Name} було полито. Час останнього поливу: {LastWatered}");
    }

    public void WaterPlant(string waterType)
    {
        WaterPlant();
        Console.WriteLine($"Використано тип води: {waterType}");
    }

    public void WaterPlant(int milliliters)
    {
        WaterPlant();
        Console.WriteLine($"Використано {milliliters} мл води");
    }

    public void WaterPlant(string waterType, int milliliters)
    {
        WaterPlant();
        Console.WriteLine($"Використано {milliliters} мл води типу: {waterType}");
    }

    private string GetFormattedDescription()
    {
        return $"{Name} ({Type}) - {Age} років, {Height} м";
    }

    public string GetDescription()
    {
        return GetFormattedDescription();
    }

    public void Grow(double growth)
    {
        if (growth <= 0)
            throw new ArgumentException("Ріст повинен бути більше 0.");

        _totalHeight -= _height;
        _height += growth;
        _totalHeight += _height;

        Console.WriteLine($"{Name} виріс на {growth}м. Нова висота: {Height}м");
    }

    public void Grow()
    {
        double defaultGrowth = 0.1;
        Grow(defaultGrowth);
    }

    public void Grow(int years)
    {
        if (years <= 0)
            throw new ArgumentException("Кількість років повинна бути більше 0.");

        double growthPerYear = 0.2;
        double totalGrowth = years * growthPerYear;

        _totalHeight -= _height;
        _height += totalGrowth;
        _age += years;
        _totalHeight += _height;

        Console.WriteLine($"{Name} виріс на {totalGrowth}м за {years} років. Нова висота: {Height}м, Новий вік: {Age} років");
    }

    public string GetPlantingInfo()
    {
        return $"{Name} було висаджено {PlantingDate:dd.MM.yyyy}.";
    }

    public bool IsMature()
    {
        return Age > 5;
    }

    public static void Reset()
    {
        try
        {
            _count = 0;
            _totalHeight = 0;
        }
        catch
        {
            // Ігноруємо помилки - в тесті будемо використовувати локальні об'єкти
        }
    }
}
