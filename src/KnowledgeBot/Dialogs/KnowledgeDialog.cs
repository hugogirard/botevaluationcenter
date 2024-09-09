using KnowledgeBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class KnowledgeDialog : ComponentDialog
{
    private readonly ILogger<KnowledgeDialog> _logger;
    private readonly IChatService _chatService;
    private readonly IStateService _stateService;

    public KnowledgeDialog(ILogger<KnowledgeDialog> logger, 
                           IChatService chatService, 
                           IStateService stateService) : base(nameof(KnowledgeDialog))
    {
        _logger = logger;
        _chatService = chatService;
        _stateService = stateService;

        InitializeWaterfall();
    }

    private void InitializeWaterfall() 
    {
        var waterfallStreps = new WaterfallStep[]
        {
                SearchInKnowledgeBase,
                ValidateAnswerFound,
                FinalStep
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new WaterfallDialog(nameof(KnowledgeDialog), waterfallStreps));

        InitialDialogId = nameof(KnowledgeDialog);
    }

    private async Task<DialogTurnResult> SearchInKnowledgeBase(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        var kbResponse = await _chatService.GetAnswerFromKnowledgeBaseAsync(message.Prompt);

        if (!string.IsNullOrEmpty(kbResponse.Answer))
        {
            string completion = kbResponse.Answer;

            message.Completion = completion;
            message.FoundInKnowledgeDatabase = true; // Indicate if we have an error
            message.QuestionAnswered = true;
            message.KnowledgeBaseName = kbResponse.KbName;

            var promptMessage = MessageFactory.Text(message.Completion, inputHint: InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
            await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);                        
        }
        else 
        {
            message.FoundInKnowledgeDatabase = false;
            await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);            
        }

        await _stateService.SaveMessageAsync(message);

        return await stepContext.NextAsync(message, cancellationToken);
    }

    private async Task<DialogTurnResult> ValidateAnswerFound(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var message = (Message)stepContext.Result;

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

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var choice = ((FoundChoice)stepContext.Result).Value;
        
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (choice.ToLowerInvariant() == "no")
        {
            message.QuestionAnswered = false;
            message.AgentToComebackToUser = true;
            await _stateService.SaveMessageAsync(message);
            var prompt = MessageFactory.Text("Thank you, an agent will comeback to you soon!", inputHint: InputHints.IgnoringInput);
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
