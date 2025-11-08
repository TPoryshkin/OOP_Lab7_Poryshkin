using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

[TestClass]
public class ProgramTests
{
    private PlantCollection plantCollection;

    [TestInitialize]
    public void TestInitialize()
    {
        plantCollection = new PlantCollection();
        plantCollection.MaxPlants = 10;
        Plant.Reset();
    }

    //тестування методу додавання рослини
    [TestMethod]
    public void TryAddPlant_WhenWithinLimit_ReturnsTrue()
    {
        // Arrange
        var plant = new Plant("Тестова рослина", PlantType.Flower, 2, 0.5, new DateTime(2023, 5, 15));

        // Act
        bool result = plantCollection.TryAddPlant(plant);
        int count = plantCollection.GetPlantsCount();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, count);
    }

    //тестування методу додавання з перевищенням ліміту
    [TestMethod]
    public void TryAddPlant_WhenExceedsLimit_ReturnsFalse()
    {
        // Arrange
        plantCollection.MaxPlants = 2;
        plantCollection.TryAddPlant(new Plant("Рослина1", PlantType.Tree, 1, 1.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Рослина2", PlantType.Shrub, 1, 1.0, DateTime.Now));

        // Act
        bool result = plantCollection.TryAddPlant(new Plant("Рослина3", PlantType.Flower, 1, 1.0, DateTime.Now));
        int count = plantCollection.GetPlantsCount();

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(2, count);
    }

    //тестування методу видалення рослини
    [TestMethod]
    public void TryRemovePlant_WhenValidIndex_ReturnsTrue()
    {
        // Arrange
        var plant = new Plant("Тестова рослина", PlantType.Flower, 2, 0.5, new DateTime(2023, 5, 15));
        plantCollection.TryAddPlant(plant);

        // Act
        bool result = plantCollection.TryRemovePlant(0);
        int count = plantCollection.GetPlantsCount();

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(0, count);
    }

    //тестування некоректного введення (невалідний індекс)
    [TestMethod]
    public void TryRemovePlant_WhenInvalidIndex_ReturnsFalse()
    {
        // Act
        bool result = plantCollection.TryRemovePlant(-1);
        int count = plantCollection.GetPlantsCount();

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(0, count);
    }

    //тестування пошуку рослин за назвою
    [TestMethod]
    public void FindPlantsByName_WhenExists_ReturnsPlants()
    {
        // Arrange
        plantCollection.TryAddPlant(new Plant("Дуб", PlantType.Tree, 5, 2.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Береза", PlantType.Tree, 3, 1.5, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Дуб червоний", PlantType.Tree, 2, 1.0, DateTime.Now));

        // Act
        var results = plantCollection.FindPlantsByName("Дуб");

        // Assert
        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.Any(p => p.Name == "Дуб"));
        Assert.IsTrue(results.Any(p => p.Name == "Дуб червоний"));
    }

    //тестування пошуку при відсутності результатів
    [TestMethod]
    public void FindPlantsByName_WhenNotExists_ReturnsEmptyList()
    {
        // Arrange
        plantCollection.TryAddPlant(new Plant("Дуб", PlantType.Tree, 5, 2.0, DateTime.Now));

        // Act
        var results = plantCollection.FindPlantsByName("Ялина");

        // Assert
        Assert.AreEqual(0, results.Count);
    }

    //тестування отримання кількості рослин
    [TestMethod]
    public void GetPlantsCount_WhenEmpty_ReturnsZero()
    {
        // Act & Assert
        Assert.AreEqual(0, plantCollection.GetPlantsCount());
    }

    //тестування підрахунку кількості рослин
    [TestMethod]
    public void GetPlantsCount_AfterAdding_ReturnsCorrectCount()
    {
        // Arrange
        plantCollection.TryAddPlant(new Plant("Рослина1", PlantType.Tree, 1, 1.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Рослина2", PlantType.Shrub, 1, 1.0, DateTime.Now));

        // Act & Assert
        Assert.AreEqual(2, plantCollection.GetPlantsCount());
    }

    //тестування очищення колекції
    [TestMethod]
    public void ClearPlants_WhenCalled_ClearsCollection()
    {
        // Arrange
        plantCollection.TryAddPlant(new Plant("Рослина1", PlantType.Tree, 1, 1.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Рослина2", PlantType.Shrub, 1, 1.0, DateTime.Now));

        // Act
        plantCollection.ClearPlants();

        // Assert
        Assert.AreEqual(0, plantCollection.GetPlantsCount());
    }

    //тестування видалення всіх рослин за назвою
    [TestMethod]
    public void RemoveAllPlantsByName_WhenExists_RemovesAllWithName()
    {
        // Arrange
        plantCollection.TryAddPlant(new Plant("Дуб", PlantType.Tree, 5, 2.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Дуб", PlantType.Tree, 3, 1.5, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Береза", PlantType.Tree, 2, 1.0, DateTime.Now));

        // Act
        plantCollection.RemoveAllPlantsByName("Дуб");
        var results = plantCollection.FindPlantsByName("Дуб");

        // Assert
        Assert.AreEqual(0, results.Count);
        Assert.AreEqual(1, plantCollection.GetPlantsCount());
    }

    //тестування пошуку рослин за типом
    [TestMethod]
    public void FindPlantsByType_WhenExists_ReturnsPlants()
    {
        // Arrange
        plantCollection.TryAddPlant(new Plant("Дуб", PlantType.Tree, 5, 2.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Троянда", PlantType.Flower, 1, 0.5, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Ялина", PlantType.Tree, 3, 1.5, DateTime.Now));

        // Act
        var results = plantCollection.FindPlantsByType(PlantType.Tree);

        // Assert
        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.All(p => p.Type == PlantType.Tree));
    }

    // тестування пошуку рослин за категорією віку
    [TestMethod]
    public void FindPlantsByAgeCategory_WhenExists_ReturnsPlants()
    {
        // Arrange
        plantCollection.TryAddPlant(new Plant("Рослина1", PlantType.Tree, 1, 1.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Рослина2", PlantType.Shrub, 5, 1.0, DateTime.Now));
        plantCollection.TryAddPlant(new Plant("Рослина3", PlantType.Flower, 15, 1.0, DateTime.Now));

        // Act
        var results = plantCollection.FindPlantsByAgeCategory("Доросла");

        // Assert
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Доросла", results[0].AgeCategory);
    }

    //тест на ізоляцію стану між тестами
    [TestMethod]
    public void TestIsolation_BetweenTests()
    {
        int count = plantCollection.GetPlantsCount();
        Assert.AreEqual(0, count);
    }
}