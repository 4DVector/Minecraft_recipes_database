namespace backend.Models;

public class Item
{
    public string Name { get; set; } = string.Empty;
    public string LocalizationName { get; set; } = string.Empty;
}

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ItemResult { get; set; } = string.Empty;
    public int Count { get; set; } = 1;
    public bool IsShapeless { get; set; } = false;
    public Dictionary<string, string> Ingredients { get; set; } = new();
}

