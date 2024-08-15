import azure.functions as func
import logging
from food_repository import FoodRepository

food_recipe = FoodRepository()

app = func.FunctionApp(http_auth_level=func.AuthLevel.FUNCTION)

@app.route(route="get_food_recipe")
async def get_food_recipe(req: func.HttpRequest) -> func.HttpResponse:

    logging.info('Function get_food_recipe.')

    try:
        req_body = req.get_json()
        
        question = req_body.get('question')
        
        if not question:
            return func.HttpResponse(
                "Please pass a question on the request body",
                status_code=400
            )

        answers = await food_recipe.get_recipe(question)

        if isinstance(answers, list):
          answers = '\n'.join(answers)

        return func.HttpResponse(answers,status_code=200)
    
    except Exception as ex:
        logging.error(ex)
        return func.HttpResponse(
            "Somthing happen",
            status_code=500
        )