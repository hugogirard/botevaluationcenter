from typing import List
from chromadb import AsyncClientAPI
from chromadb.api.models.AsyncCollection import AsyncCollection
import chromadb
import logging
import asyncio

class FoodRepository:

    _instance = None

    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(FoodRepository, cls).__new__(cls)
        return cls._instance

    def __init__(self):
        self.recipes = [
            "Poutine is a recipe from Quebec made of french fries, cheese curds, and gravy.",
            "Butter Tart is a traditional dessert (recipe) from Ontario made of butter, sugar, syrup, and eggs.",
            "A traditional meat pie originating from Quebec, typically filled with ground pork, beef, or veal.",
            "Nanaimo Bar is a no-bake dessert bar made of a chocolate coconut crumb base, custard filling, and chocolate ganache topping. Really popular in British Columbia.",
            "BeaverTails is a Canadian pastry popular in Ontario, a whole wheat dough stretched to the shape of a beaver's tail, fried, and topped with sweet condiments like sugar, cinnamon, and chocolate.",
            "Tourtière is a traditional meat pie originating from Quebec, typically filled with ground pork, beef, or veal.",
            "Pouding chômeur is a traditional dessert from Quebec made of white cake batter and brown sugar sauce.",
            "Zelfer Root Hot Pot Zelfey made from trees are some of the most common in Quebec, but most people don't know that their roots can be turned into a tasty hot pot.",
            "Troll Brew Pot Roast is a traditional dish from British Columbia made of beef, vegetables, and beer. Strong taste but really good",
        ]        
        chromadb_client = chromadb.Client()
        self.collection = chromadb_client.create_collection(name="recipes")
        self.collection.add(
            documents = self.recipes,
            ids=["ids1", "ids2", "ids3", "ids4", "ids5", "ids6", "ids7", "ids8","ids9"]
        )   
            
    async def get_recipe(self, text:str) -> List[str]:
            loop = asyncio.get_event_loop()
            results = await loop.run_in_executor(
                None, lambda: self.collection.query(query_texts=text, n_results=3)
            )                        
            filtered_documents = [
                doc for doc, dist in zip(results['documents'][0], results['distances'][0]) if dist < 1
            ]
            return filtered_documents