using System;
using System.Collections.Generic;
using System.Linq;

// Клас для роботи з колекцією рослин 
public class PlantCollection
{
    // використання колекції List<T> для зберігання об'єктів
    private List<Plant> plants = new List<Plant>();
    private int maxPlants = 10;

    public int MaxPlants
    {
        get { return maxPlants; }
        set { maxPlants = value; }
    }

    //  очищення колекції через метод List<T>.Clear()
    // Додано метод очищення колекції - вимога №3
    public void ClearPlants()
    {
        plants.Clear();
    }

    //додавання об'єкта через метод List<T>.Add()
    public bool TryAddPlant(Plant plant)
    {
        if (plants.Count >= maxPlants)
            return false;

        plants.Add(plant);
        return true;
    }

    public int GetPlantsCount()
    {
        return plants.Count;
    }

    //пошук об'єктів через методи List<T> та LINQ
    public List<Plant> FindPlantsByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return new List<Plant>();

        return plants.Where(p => p != null && p.Name != null &&
            p.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }

    //видалення об'єкта через метод List<T>.RemoveAt()
    public bool TryRemovePlant(int index)
    {
        if (index < 0 || index >= plants.Count)
            return false;

        plants.RemoveAt(index);
        return true;
    }

    public List<Plant> GetAllPlants()
    {
        return plants.ToList();
    }

    // видалення об'єктів за умовою через метод List<T>.RemoveAll()
    public void RemoveAllPlantsByName(string name)
    {
        plants.RemoveAll(p => p != null && p.Name != null &&
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // пошук об'єктів за типом через методи List<T>
    public List<Plant> FindPlantsByType(PlantType type)
    {
        return plants.Where(p => p != null && p.Type == type).ToList();
    }

    //пошук об'єктів за категорією віку через методи List<T>
    public List<Plant> FindPlantsByAgeCategory(string ageCategory)
    {
        return plants.Where(p => p != null && p.AgeCategory == ageCategory).ToList();
    }
}