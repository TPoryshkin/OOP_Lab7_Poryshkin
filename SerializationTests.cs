using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

[TestClass]
[DoNotParallelize]
public class SerializationTests
{
    private PlantCollection plantCollection;
    private string testCsvFile;
    private string testJsonFile;

    [TestInitialize]
    public void TestInitialize()
    {
        string timestamp = DateTime.Now.Ticks.ToString();
        testCsvFile = $"test_plants_{timestamp}.csv";
        testJsonFile = $"test_plants_{timestamp}.json";

        plantCollection = new PlantCollection();
        plantCollection.MaxPlants = 10;
        Plant.Reset();

        var plants = new List<Plant>
        {
            new Plant("Троянда", PlantType.Flower, 2, 0.5, new DateTime(2023, 5, 15)),
            new Plant("Дуб", PlantType.Tree, 10, 5.0, new DateTime(2015, 3, 20)),
            new Plant("Кактус", PlantType.Succulent, 1, 0.3, new DateTime(2024, 1, 10))
        };

        foreach (var plant in plants)
        {
            plantCollection.TryAddPlant(plant);
            Plant.AddPlantToCollection(plant);
        }
    }

    [TestCleanup]
    public void TestCleanup()
    {
        TryDeleteFileWithRetry(testCsvFile);
        TryDeleteFileWithRetry(testJsonFile);

        Plant.Reset();
    }

    private void TryDeleteFileWithRetry(string filePath)
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    break;
                }
            }
            catch (IOException)
            {
                if (i == 2) throw; 
                Thread.Sleep(100); 
            }
        }
    }

    [TestMethod]
    public void SaveToCSV_ValidData_CreatesFileWithCorrectContent()
    {
        // Arrange
        var plants = plantCollection.GetAllPlants();

        // Act
        SaveToCSV(plants, testCsvFile);

        // Assert
        Assert.IsTrue(File.Exists(testCsvFile));

        // Чекаємо, поки файл стане доступним
        WaitForFileAccess(testCsvFile);

        var lines = File.ReadAllLines(testCsvFile);
        Assert.AreEqual(4, lines.Length); 
        Assert.AreEqual("Name,Type,Age,Height,PlantingDate,IsFlowering", lines[0]);
        Assert.IsTrue(lines[1].Contains("Троянда"));
    }

    [TestMethod]
    public void LoadFromCSV_ValidFile_LoadsCorrectNumberOfPlants()
    {
        // Arrange
        var plants = plantCollection.GetAllPlants();
        SaveToCSV(plants, testCsvFile);
        WaitForFileAccess(testCsvFile);

        // Act
        var loadedPlants = LoadFromCSV(testCsvFile);

        // Assert
        Assert.AreEqual(3, loadedPlants.Count);
    }

    [TestMethod]
    public void SaveToJSON_ValidData_CreatesFileWithCorrectContent()
    {
        // Arrange
        var plants = plantCollection.GetAllPlants();

        // Act
        SaveToJSON(plants, testJsonFile);

        // Assert
        Assert.IsTrue(File.Exists(testJsonFile));

        WaitForFileAccess(testJsonFile);

        var jsonContent = File.ReadAllText(testJsonFile);

        Assert.IsTrue(jsonContent.Contains("Назва"), "JSON повинен містити українські назви властивостей");
        Assert.IsTrue(jsonContent.Contains("Троянда"), "JSON повинен містити назви рослин");
        Assert.IsTrue(jsonContent.Contains("Вік"), "JSON повинен містити властивість Вік");
    }

    [TestMethod]
    public void LoadFromJSON_ValidFile_LoadsCorrectNumberOfPlants()
    {
        // Arrange
        var plants = plantCollection.GetAllPlants();
        SaveToJSON(plants, testJsonFile);
        WaitForFileAccess(testJsonFile);

        // Act
        var loadedPlants = LoadFromJSON(testJsonFile);

        // Assert
        Assert.AreEqual(3, loadedPlants.Count);
    }

    [TestMethod]
    public void ClearCollection_WhenCalled_RemovesAllPlants()
    {
        // Arrange
        int initialCount = plantCollection.GetPlantsCount();
        Assert.AreEqual(3, initialCount);

        // Act
        plantCollection.ClearPlants();
        Plant.Reset();

        // Assert
        Assert.AreEqual(0, plantCollection.GetPlantsCount());
        Assert.AreEqual(0, Plant.Count);
    }

    [TestMethod]
    public void LoadFromCSV_WithInvalidData_SkipsInvalidRows()
    {
        // Arrange 
        var lines = new List<string>
        {
            "Name,Type,Age,Height,PlantingDate,IsFlowering",
            "Троянда,Flower,2,0.5,2023-05-15,True", // коректний рядок
            "Невірна,InvalidType,abc,0.5,2023-05-15,True", // нечисловий вік та невірний тип
            ",Tree,10,5.0,2015-03-20,True", // пуста назва
            "Дуб,Tree,10,5.0,2015-03-20,True" // коректний рядок
        };

        File.WriteAllLines(testCsvFile, lines);
        WaitForFileAccess(testCsvFile);

        // Act
        var loadedPlants = LoadFromCSV(testCsvFile);

        // Assert 
        Assert.AreEqual(2, loadedPlants.Count);

        Assert.IsTrue(loadedPlants.Exists(p => p.Name == "Троянда"));
        Assert.IsTrue(loadedPlants.Exists(p => p.Name == "Дуб"));
    }

    private void WaitForFileAccess(string filePath, int maxRetries = 5)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return; 
                }
            }
            catch (IOException)
            {
                if (i == maxRetries - 1) throw;
                Thread.Sleep(100);
            }
        }
    }

    // Допоміжні методи для серіалізації
    private void SaveToCSV(List<Plant> plants, string fileName)
    {
        var lines = new List<string>();

        lines.Add("Name,Type,Age,Height,PlantingDate,IsFlowering");

        foreach (var plant in plants)
        {
            lines.Add(plant.ToString());
        }

        File.WriteAllLines(fileName, lines);
    }

    private List<Plant> LoadFromCSV(string fileName)
    {
        WaitForFileAccess(fileName);
        var lines = File.ReadAllLines(fileName);
        var loadedPlants = new List<Plant>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (Plant.TryParse(lines[i], out Plant plant) && plant != null)
            {
                loadedPlants.Add(plant);
            }
        }

        return loadedPlants;
    }

    private void SaveToJSON(List<Plant> plants, string fileName)
    {
        var jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        string json = JsonConvert.SerializeObject(plants, jsonSettings);
        File.WriteAllText(fileName, json);
    }

    private List<Plant> LoadFromJSON(string fileName)
    {
        WaitForFileAccess(fileName);
        string json = File.ReadAllText(fileName);
        var plants = JsonConvert.DeserializeObject<List<Plant>>(json);
        return plants ?? new List<Plant>();
    }
}