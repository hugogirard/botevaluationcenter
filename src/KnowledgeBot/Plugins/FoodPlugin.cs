using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace KnowledgeBot.Plugins;

public class Recipe 
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("province")]
    public string Province { get; set; }
}

public class FoodPlugin
{
    List<Recipe> _recipes;
    private readonly ILogger<FoodPlugin> _logger;

    List<string> synonyms = new List<string>()
    {  
        // Recipe synonyms  
        "Formula",
        "Method",
        "Procedure",
        "Instructions",
        "Cookery",
        "Dish",
        "Guide",
        "Creation",
        "Menu",
        "Instruction",
        "Plan",
        "Technique",
        "Prescription",
        "Secret",  
      
        // Food synonyms  
        "Cuisine",
        "Dish",
        "Meal",
        "Fare",
        "Cooking",
        "Edibles",
        "Nourishment",
        "Provisions",
        "Sustenance",
        "Nutrition",
        "Eatables",
        "Comestibles",
        "Gastronomy",
        "Culinary",
        "Catering",
        "Grub",
        "Repast",
        "Chow",
        "Fodder",
        "Provender"
    };


    public FoodPlugin(ILogger<FoodPlugin> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _recipes = new List<Recipe>()
        {
            new Recipe { Name = "Poutine", Description = "A popular Canadian dish made of french fries, cheese curds, and gravy.", Province = "Quebec" },
            new Recipe { Name = "Butter Tart", Description = "A sweet pastry filled with a gooey buttery filling, often made with maple syrup.", Province = "Ontario" },
            new Recipe { Name = "Nanaimo Bar", Description = "A no-bake dessert bar made of a chocolate coconut crumb base, custard filling, and chocolate ganache topping.", Province = "British Columbia" },
            new Recipe { Name = "BeaverTails", Description = "A fried dough pastry stretched to resemble a beaver's tail, often topped with various sweet toppings.", Province = "Ontario" },
            new Recipe { Name = "Tourtière", Description = "A traditional meat pie originating from Quebec, typically filled with ground pork, beef, or veal.", Province = "Quebec" },
            // Those 3 recipes below are from Too Many bones boardgame, we needed to create fake recipe
            // to be sure the LLM take the answer from the plugin
            new Recipe { Name = "Zelfer Root Hot Pot", Description = "Zelfey trees are some of the most common in Quebec, but most people don't know that their roots can be turned into a tasty hot pot", Province = "Quebec" },
            new Recipe { Name = "Deepwood Campfire Bread", Description = "A perfect snack for when you're roughing it.  No oven required!!!", Province = "Ontario" },
            new Recipe { Name = "Troll Brew Pot Roast", Description = "Though know for it's revivifying qualities when imbibed by it's creator this food taste really bad", Province = "British Columbia" }
        };

        _logger = logger;

    }

    [KernelFunction("get_popular_recipes")]
    [Description("Get popular dished from Canada")]
    [return: Description("Popular recipes from Canada based by province")]
    public async Task<List<Recipe>> GetDishes(string question)
    {
        _logger.LogInformation($"Calling FoodPlugin with question: {question}");

        if (!IsAboutFood(question))
            return new List<Recipe>();

        string province = ExtractProvince(question);
        var filteredRecipes = _recipes.Where(d => d.Province.Equals(province, StringComparison.OrdinalIgnoreCase)).ToList();

        if (filteredRecipes.Any())
        {
            _logger.LogInformation($"Found recipes");
            foreach (var filteredRecipe in filteredRecipes) 
            { 
                _logger.LogInformation($"{filteredRecipe.Name}");
            }
        }
        else 
        {
            _logger.LogInformation($"No recipes found");
        }

        return await Task.FromResult(filteredRecipes);
    }

    private bool IsAboutFood(string question) 
    {
        if (question.ToLower().Contains(question.ToLower()))
            return true;

        return false;
    }

    private string ExtractProvince(string question)
    {
        foreach (var province in _recipes.Select(r => r.Province))
        {
            if (question.ToLower().Contains(province.ToLower()))
                return province;
        }

        return string.Empty;
    }
}
