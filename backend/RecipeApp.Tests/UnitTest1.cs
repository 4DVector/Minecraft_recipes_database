using Xunit;
using backend.Models;
using System.Collections.Generic;

namespace RecipeApp.Tests
{
    public class RecipeTests
    {
        [Fact]
        public void CreateRecipe_WithValidData_SetsPropertiesCorrectly()
        {
            var recipe = new Recipe
            {
                Id = 1,
                Name = "stone_sword_1",
                ItemResult = "stone_sword",
                Count = 1,
                IsShapeless = false,
                Ingredients = new Dictionary<string, string>
                {
                    { "2", "cobblestone" },
                    { "5", "cobblestone" },
                    { "8", "stick" }
                }
            };
            
            Assert.Equal(1, recipe.Id);
            Assert.Equal("stone_sword_1", recipe.Name);
            Assert.Equal("stone_sword", recipe.ItemResult);
            Assert.False(recipe.IsShapeless);
            Assert.Equal("stick", recipe.Ingredients["8"]);
        }
    }
}