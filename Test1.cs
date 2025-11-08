using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class PlantTests
{
    private Plant testPlant;

    [TestInitialize]
    public void TestInitialize()
    {
        // Викликається перед кожним тестом - ініціалізація тестового об'єкта
        Console.WriteLine($"=== Початок тесту {TestContext.TestName} ===");

        // Спочатку скидаємо статичний стан
        Plant.Reset();

        // Переконуємося, що стан скинуто
        int retryCount = 0;
        while (Plant.Count != 0 && retryCount < 3)
        {
            Console.WriteLine($"Спроба {retryCount + 1}: Count все ще {Plant.Count}, повторне скидання...");
            Plant.Reset();
            retryCount++;
            System.Threading.Thread.Sleep(10); // Невелика затримка
        }

        if (Plant.Count != 0)
        {
            Assert.Inconclusive($"Не вдалося скинути статичний стан. Count = {Plant.Count}");
        }

        // Створюємо тестовий об'єкт
        testPlant = new Plant("Тестова рослина", PlantType.Flower, 2, 0.5, new DateTime(2023, 5, 15));
        Console.WriteLine($"Створено тестову рослину: {testPlant.Name}");
    }

    [TestCleanup]
    public void TestCleanup()
    {
        // Викликається після кожного тесту - очищення ресурсів
        Console.WriteLine($"=== Завершення тесту {TestContext.TestName} ===");

        // Скидаємо статичний стан після тесту
        Plant.Reset();
        testPlant = null;
    }

    // Додайте цю властивість для доступу до TestContext
    public TestContext TestContext { get; set; }

    // Тестування валідації властивості Name з використанням DataRow
    [TestMethod]
    [DataRow(null, false)]
    [DataRow("", false)]
    [DataRow("А", false)]
    [DataRow("Ро", true)]
    [DataRow("Дуже довга назва рослини яка перевищує допустиму довжину", false)]
    [DataRow("Рослина1", false)]
    public void Name_ValidationTest(string name, bool expectedIsValid)
    {
        // Arrange
        bool actualIsValid = true;

        // Act
        try
        {
            testPlant.Name = name;
        }
        catch (ArgumentException)
        {
            actualIsValid = false;
        }

        // Assert
        Assert.AreEqual(expectedIsValid, actualIsValid);
    }

    // Тестування валідації властивості Age з використанням DataRow
    [TestMethod]
    [DataRow(-1, false)]
    [DataRow(0, true)]
    [DataRow(100, true)]
    [DataRow(5000, true)]
    [DataRow(5001, false)]
    public void Age_ValidationTest(int age, bool expectedIsValid)
    {
        // Arrange
        bool actualIsValid = true;

        // Act
        try
        {
            testPlant.Age = age;
        }
        catch (ArgumentException)
        {
            actualIsValid = false;
        }

        // Assert
        Assert.AreEqual(expectedIsValid, actualIsValid);
    }

    // Тестування валідації властивості Height з використанням DataRow
    [TestMethod]
    [DataRow(-1.0, false)]
    [DataRow(0.0, false)]
    [DataRow(0.01, true)]
    [DataRow(50.5, true)]
    [DataRow(115.7, true)]
    [DataRow(115.8, false)]
    public void Height_ValidationTest(double height, bool expectedIsValid)
    {
        // Arrange
        bool actualIsValid = true;

        // Act
        try
        {
            testPlant.Height = height;
        }
        catch (ArgumentException)
        {
            actualIsValid = false;
        }

        // Assert
        Assert.AreEqual(expectedIsValid, actualIsValid);
    }

    // Тестування методу Grow з від'ємним значенням 
    [TestMethod]
    public void Grow_NegativeValue_ThrowsException()
    {
        // Arrange
        double negativeGrowth = -0.5;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() => testPlant.Grow(negativeGrowth));
    }

    // Тестування методу Grow з додатнім значенням
    [TestMethod]
    public void Grow_PositiveValue_UpdatesHeight()
    {
        // Arrange
        double initialHeight = testPlant.Height;
        double growth = 0.3;
        double expectedHeight = initialHeight + growth;

        // Act
        testPlant.Grow(growth);

        // Assert
        Assert.AreEqual(expectedHeight, testPlant.Height);
    }

    // Тестування перевантаженого методу Grow без параметрів
    [TestMethod]
    public void Grow_WithoutParameters_UpdatesHeight()
    {
        // Arrange
        double initialHeight = testPlant.Height;
        double expectedHeight = initialHeight + 0.1;

        // Act
        testPlant.Grow();

        // Assert
        Assert.AreEqual(expectedHeight, testPlant.Height);
    }

    // Тестування статичного методу Parse з коректними даними
    [TestMethod]
    public void Parse_ValidData_ReturnsPlantObject()
    {
        // Arrange
        string validData = "Троянда,Flower,2,0.5,2023-05-15,True";

        // Act
        Plant result = Plant.Parse(validData);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Троянда", result.Name);
        Assert.AreEqual(PlantType.Flower, result.Type);
        Assert.AreEqual(2, result.Age);
    }

    // Тестування статичного методу Parse з некоректними даними 
    [TestMethod]
    [DataRow("Неповні,дані")]
    [DataRow("Невірний,тип,2,0.5,2023-05-15,True")]
    [DataRow("Троянда,Flower,нечисло,0.5,2023-05-15,True")]
    public void Parse_InvalidData_ThrowsException(string invalidData)
    {
        // Act & Assert
        Assert.ThrowsExactly<FormatException>(() => Plant.Parse(invalidData));
    }

    // Тестування статичного методу TryParse з використанням DataRow
    [TestMethod]
    [DataRow("Троянда,Flower,2,0.5,2023-05-15,True", true)]
    [DataRow("Неповні,дані", false)]
    [DataRow("", false)]
    public void TryParse_VariousInputs_ReturnsExpectedResult(string input, bool expectedSuccess)
    {
        // Arrange & Act
        bool result = Plant.TryParse(input, out Plant plant);

        // Assert
        Assert.AreEqual(expectedSuccess, result);
        if (expectedSuccess)
        {
            Assert.IsNotNull(plant);
        }
        else
        {
            Assert.IsNull(plant);
        }
    }

    // Тестування статичних властивостей Count та AverageHeight
    [TestMethod]
    public void StaticProperties_AfterAddingPlants_CalculateCorrectly()
    {
        // Arrange
        Plant.Reset();
        var plant1 = new Plant("Рослина1", PlantType.Tree, 5, 2.0, DateTime.Now);
        var plant2 = new Plant("Рослина2", PlantType.Shrub, 3, 1.5, DateTime.Now);

        Assert.AreEqual(0, Plant.Count, "Count повинен бути 0 перед додаванням рослин");
        Assert.AreEqual(0, Plant.AverageHeight, "AverageHeight повинен бути 0 перед додаванням рослин");

        // Act
        Plant.AddPlantToCollection(plant1);
        Plant.AddPlantToCollection(plant2);

        // Assert
        Assert.AreEqual(2, Plant.Count, "Count повинен бути 2 після додавання двох рослин");
        Assert.AreEqual(1.75, Plant.AverageHeight, 0.001, "AverageHeight повинен бути 1.75 (2.0 + 1.5) / 2");
    }

    // Тестування методу ToString
    [TestMethod]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var plant = new Plant("Тест", PlantType.Tree, 1, 1.5, new DateTime(2023, 1, 1));
        string expected = "Тест,Tree,1,1.5,2023-01-01,True";

        // Act
        string result = plant.ToString();

        // Assert
        Assert.AreEqual(expected, result);
    }

    // Тестування властивості AgeCategory з використанням DataRow
    [TestMethod]
    [DataRow(1, "Молода")]
    [DataRow(5, "Доросла")]
    [DataRow(15, "Стара")]
    public void AgeCategory_VariousAges_ReturnsCorrectCategory(int age, string expectedCategory)
    {
        // Arrange
        testPlant.Age = age;

        // Act
        string result = testPlant.AgeCategory;

        // Assert
        Assert.AreEqual(expectedCategory, result);
    }

    // Тестування методу IsMature з використанням DataRow
    [TestMethod]
    [DataRow(4, false)]
    [DataRow(5, false)]
    [DataRow(6, true)]
    public void IsMature_VariousAges_ReturnsCorrectResult(int age, bool expectedMature)
    {
        // Arrange
        testPlant.Age = age;

        // Act
        bool result = testPlant.IsMature();

        // Assert
        Assert.AreEqual(expectedMature, result);
    }

    // Тестування валідації дати посадки з використанням DataRow
    [TestMethod]
    [DataRow(1899, 1, 1, false)]
    [DataRow(1900, 1, 1, true)]
    [DataRow(2023, 5, 15, true)] // минула дата
    public void PlantingDate_ValidationTest_StaticDates(int year, int month, int day, bool expectedIsValid)
    {
        // Arrange
        bool actualIsValid = true;
        var date = new DateTime(year, month, day);

        // Act
        try
        {
            testPlant.PlantingDate = date;
            Console.WriteLine($"Дата {date:yyyy-MM-dd} була прийнята. Поточна дата: {DateTime.Now:yyyy-MM-dd}");
        }
        catch (ArgumentException ex)
        {
            actualIsValid = false;
            Console.WriteLine($"Дата {date:yyyy-MM-dd} була відхилена: {ex.Message}. Поточна дата: {DateTime.Now:yyyy-MM-dd}");
        }

        // Assert
        Assert.AreEqual(expectedIsValid, actualIsValid,
            $"Для дати {date:yyyy-MM-dd} очікувалась валідність: {expectedIsValid}, але отримано: {actualIsValid}");
    }

    // Тестування майбутньої дати 
    [TestMethod]
    public void PlantingDate_FutureDate_ThrowsException()
    {
        // Arrange
        var futureDate = DateTime.Today.AddDays(1);
        bool wasExceptionThrown = false;

        // Act
        try
        {
            testPlant.PlantingDate = futureDate;
            Console.WriteLine($"МАЙБУТНЯ дата {futureDate:yyyy-MM-dd} була ПРИЙНЯТА (це неправильно)! Поточна дата: {DateTime.Today:yyyy-MM-dd}");
        }
        catch (ArgumentException)
        {
            wasExceptionThrown = true;
            Console.WriteLine($"МАЙБУТНЯ дата {futureDate:yyyy-MM-dd} була ВІДХИЛЕНА (це правильно). Поточна дата: {DateTime.Today:yyyy-MM-dd}");
        }

        // Assert
        Assert.IsTrue(wasExceptionThrown,
            $"Майбутня дата {futureDate:yyyy-MM-dd} повинна викликати виняток. Поточна дата: {DateTime.Today:yyyy-MM-dd}");
    }

    // Тестування вчорашньої дати 
    [TestMethod]
    public void PlantingDate_PastDate_IsValid()
    {
        // Arrange
        var pastDate = DateTime.Today.AddDays(-1);
        bool isValid = true;

        // Act
        try
        {
            testPlant.PlantingDate = pastDate;
            Console.WriteLine($"МИНУЛА дата {pastDate:yyyy-MM-dd} була ПРИЙНЯТА (це правильно). Поточна дата: {DateTime.Today:yyyy-MM-dd}");
        }
        catch (ArgumentException)
        {
            isValid = false;
            Console.WriteLine($"МИНУЛА дата {pastDate:yyyy-MM-dd} була ВІДХИЛЕНА (це неправильно)! Поточна дата: {DateTime.Today:yyyy-MM-dd}");
        }

        // Assert
        Assert.IsTrue(isValid,
            $"Минула дата {pastDate:yyyy-MM-dd} повинна бути валідною. Поточна дата: {DateTime.Today:yyyy-MM-dd}");
    }

    // Тестування статичного методу GetGlobalPlantInfo
    [TestMethod]
    public void GetGlobalPlantInfo_ReturnsCorrectString()
    {
        // Arrange
        // Використовуємо явне скидання через створення нових об'єктів
        var localTestPlant = new Plant("Локальна тестова рослина", PlantType.Flower, 2, 0.5, new DateTime(2023, 5, 15));

        // Act
        // Додаємо рослину і одразу тестуємо
        Plant.AddPlantToCollection(localTestPlant);
        string result = Plant.GetGlobalPlantInfo();

        // Assert
        string expected = $"Всього рослин: 1, Середня висота: {localTestPlant.Height:F2} м";
        Assert.AreEqual(expected, result);
    }

    // Тестування перевантаженого конструктора з двома параметрами (ім'я та тип)
    [TestMethod]
    public void Constructor_WithNameAndType_CreatesValidPlant()
    {
        // Arrange & Act
        var plant = new Plant("Дуб", PlantType.Tree);

        // Assert
        Assert.AreEqual("Дуб", plant.Name);
        Assert.AreEqual(PlantType.Tree, plant.Type);
        Assert.AreEqual(1, plant.Age); // default age
    }

    // Тестування перевантаженого конструктора з двома параметрами (ім'я та вік)
    [TestMethod]
    public void Constructor_WithNameAndAge_CreatesValidPlant()
    {
        // Arrange & Act
        var plant = new Plant("Ялина", 10);

        // Assert
        Assert.AreEqual("Ялина", plant.Name);
        Assert.AreEqual(10, plant.Age);
        Assert.AreEqual(PlantType.Tree, plant.Type);
    }
}