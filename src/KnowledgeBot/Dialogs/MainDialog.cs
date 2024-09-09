using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly ILogger<MainDialog> _logger;
    private readonly IStateService _stateService;

    private const string AGENT_FEEDBACK = "Cannot found the answer from your question, an agent will comeback to you soon";

    public MainDialog(ILogger<MainDialog> logger, 
                      GreetingDialog greetingDialog,
                      KnowledgeDialog knowledgeDialog,
                      ExtendedSearchDialog extendedSearchDialog,
                      ICosmosDbRepository cosmosDbRepository,
                      IStateService stateService) : base(nameof(MainDialog))
    {
        _logger = logger;        
        _stateService = stateService;

        var waterfallSteps = new WaterfallStep[]
        {
            InitialStepAsync,
            FindAnswerKnowledgeBase,
            EvaluateAnswerKnowledgeBase,
            EvaluateAnswerRetrieval,
            EvaluateAnswer,
            AcknowledgeQuestionAnswered,
            FinalStepAsync
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        AddDialog(greetingDialog);
        AddDialog(knowledgeDialog);
        AddDialog(extendedSearchDialog);
        AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

        InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
    }

    private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {         
        return await stepContext.BeginDialogAsync(nameof(GreetingDialog), null, cancellationToken);
    }

    private async Task<DialogTurnResult> FindAnswerKnowledgeBase(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        return await stepContext.BeginDialogAsync(nameof(KnowledgeDialog), null, cancellationToken);        
    }

    private async Task<DialogTurnResult> EvaluateAnswerKnowledgeBase(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (message.FoundInKnowledgeDatabase)
        {
            var promptMessage = MessageFactory.Text(message.Completion, inputHint: InputHints.IgnoringInput);
            message.QuestionAnswered = true;

            // Update the message in the database
            await _stateService.SaveMessageAsync(message);

            await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);            
        }
        else
        {            
            return await stepContext.BeginDialogAsync(nameof(ExtendedSearchDialog), null, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> EvaluateAnswerRetrieval(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (message.FoundInKnowledgeDatabase)
            return await stepContext.NextAsync(null, cancellationToken);

        Activity promptMessage;

        if (message.FoundInRetrieval) 
        {
            promptMessage = MessageFactory.Text(message.Completion);
            message.QuestionAnswered = true;

            // Update the message in the database
            await _stateService.SaveMessageAsync(message);

            await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);

            // Ask if the user want an agent to comeback to him
            string msg = "The answer provided was not found from one of our knowledge database, do you want an agent to validate and comeback to you?";
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                                                 new PromptOptions
                                                 {
                                                     Prompt = MessageFactory.Text(msg),
                                                     Choices = ChoiceFactory.ToChoices(new List<string>
                                                     {
                                                        "Yes",
                                                        "No"
                                                     }),
                                                     Style = ListStyle.HeroCard
                                                 }, cancellationToken);
        }
        else 
        {
            promptMessage = MessageFactory.Text(AGENT_FEEDBACK, inputHint: InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
        }

        await _stateService.SaveMessageAsync(message);

        return await EndConversation(stepContext, cancellationToken);

    }

    private async Task<DialogTurnResult> EvaluateAnswer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (message.FoundInKnowledgeDatabase)
        {
            var promptMessage = MessageFactory.Text("Did the answer provided answered your question?");
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                                                 new PromptOptions
                                                 {
                                                     Prompt = promptMessage,
                                                     Choices = ChoiceFactory.ToChoices(new List<string>
                                                     {
                                                        "Yes",
                                                        "No"
                                                     }),
                                                     Style = ListStyle.HeroCard
                                                 }, cancellationToken);
        }

        // Validate here if the user want an agent to comeback to him
        var choice = ((FoundChoice)stepContext.Result).Value;

        if (choice.ToLowerInvariant() == "yes")
        {
            message.QuestionAnswered = false;
            message.AgentToComebackToUser = true;
            await _stateService.SaveMessageAsync(message);
            var prompt = MessageFactory.Text("Thank you, an agent will comeback to you soon!");
            await stepContext.Context.SendActivityAsync(prompt, cancellationToken);
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
                            
    }
    
    private async Task<DialogTurnResult> AcknowledgeQuestionAnswered(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (!message.QuestionAnswered)
            return await stepContext.NextAsync(null, cancellationToken);

        if (message.FoundInRetrieval)
        {
            var choice = ((FoundChoice)stepContext.Result).Value;

            if (choice.ToLowerInvariant() == "no")
            {
                message.QuestionAnswered = false;
                message.AgentToComebackToUser = true;
                var prompt = MessageFactory.Text("Thank you for your feedback, an agent will comeback to you soon!");
                await stepContext.Context.SendActivityAsync(prompt, cancellationToken);
            }
            else
            {
                message.QuestionAnswered = true;
            }

            await _stateService.SaveMessageAsync(message);

            // Here we want to end the dialog here
            if (!message.QuestionAnswered)
                return await stepContext.EndDialogAsync(null, cancellationToken);

            return await stepContext.NextAsync(null, cancellationToken);
        }
        else 
        {
            var prompt = MessageFactory.Text("Cannot find the answer to your question; an agent will get back to you soon.");
            await stepContext.Context.SendActivityAsync(prompt, cancellationToken);

            message.AgentToComebackToUser = true;
            await _stateService.SaveMessageAsync(message);

            return await EndConversation(stepContext, cancellationToken);
        }

    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {        
        var prompt = MessageFactory.Text("Thank you for using our bot knowledge assistant, have a great day!");
        await stepContext.Context.SendActivityAsync(prompt, cancellationToken);
        return await EndConversation(stepContext, cancellationToken);
    }

    private async Task<DialogTurnResult> EndConversation(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var endOfConversation = Activity.CreateEndOfConversationActivity();
        endOfConversation.Code = EndOfConversationCodes.CompletedSuccessfully;
        await stepContext.Context.SendActivityAsync(endOfConversation, cancellationToken);

        return await stepContext.EndDialogAsync(null, cancellationToken);        
    }

}
