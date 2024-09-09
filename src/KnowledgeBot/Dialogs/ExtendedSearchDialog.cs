using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class ExtendedSearchDialog : ComponentDialog
{
    private readonly IStateService _stateService;
    private readonly ILogger<ExtendedSearchDialog> _logger;
    private readonly IChatService _chatService;

    public ExtendedSearchDialog(IStateService stateService, 
                                ILogger<ExtendedSearchDialog> logger,
                                IChatService chatService) : base(nameof(ExtendedSearchDialog))
    {
        _stateService = stateService;
        _logger = logger;
        _chatService = chatService;

        InitializeWaterfall();
    }

    private void InitializeWaterfall()
    {
        var waterfallStreps = new WaterfallStep[]
        {
                SearchExtendedSource,
                ValidateAnswerFound,
                FinalStep
        };

        AddDialog(new WaterfallDialog(nameof(ExtendedSearchDialog), waterfallStreps));

        InitialDialogId = nameof(ExtendedSearchDialog);
    }

    private async Task<DialogTurnResult> SearchExtendedSource(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        var response = await _chatService.GetAnswerFromExtendedSourceAsync(message.Prompt);

        if (!string.IsNullOrEmpty(response.Answer))
        {
            message.Completion = response.Answer;
            message.FoundInRetrieval = true;
            message.RetrievalPluginName = response.RetrievalPluginName;

            var promptMessage = MessageFactory.Text(response.Answer, inputHint: InputHints.IgnoringInput);

            await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);

            await _stateService.SaveMessageAsync(message);

            await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);

            return await stepContext.NextAsync(null, cancellationToken);
        }

        return await stepContext.NextAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> ValidateAnswerFound(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (message.FoundInRetrieval)
        {
            var promptMessage = MessageFactory.Text("The answer provided was not found from one of our knowledge database, do you want an agent to validate and comeback to you?");
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

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var choice = ((FoundChoice)stepContext.Result).Value;

        if (choice.ToLowerInvariant() == "yes")
        {
            var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

            message.AgentToComebackToUser = true;
            message.QuestionAnswered = false;

            await _stateService.SaveMessageAsync(message);

            var prompt = MessageFactory.Text("Thank you, an agent will comeback to you soon!");
            await stepContext.Context.SendActivityAsync(prompt, cancellationToken);
        }
        else 
        {
            var prompt = MessageFactory.Text("Thank you for using our bot knowledge assistant, have a great day!", inputHint: InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(prompt, cancellationToken);
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
