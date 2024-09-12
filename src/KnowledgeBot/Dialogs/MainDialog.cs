using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
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
                      IStateService stateService,
                      IConfiguration configuration) : base(nameof(MainDialog))
    {
        _logger = logger;        
        _stateService = stateService;

        var waterfallSteps = new WaterfallStep[]
        {
            //PromptStepAsync,
            //LoginStepAsync,
            InitialStep,
            SearchKB,
            SearchRetrievalPlugin,
            FinalStep
        };

        // Add the login dialog
        AddDialog(new OAuthPrompt(nameof(OAuthPrompt),
            new OAuthPromptSettings
            {
                ConnectionName = configuration["ConnectionName"],
                Text = "Please Sign In",
                Title = "Login",
                Timeout = 300000, // User has 5 minutes to login
            }));

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        AddDialog(greetingDialog);
        AddDialog(knowledgeDialog);
        AddDialog(extendedSearchDialog);
        AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

        InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
    }

    private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
    }

    private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        // Get the token from the previous step. Note that we could also have gotten the
        // token directly from the prompt itself. There is an example of this in the next method.
        var tokenResponse = (TokenResponse)stepContext.Result;
        if (tokenResponse != null)
        {
            return await stepContext.NextAsync(null, cancellationToken);
        }

        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
        return await stepContext.EndDialogAsync();
    }


    private async Task<DialogTurnResult> InitialStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {         
        return await stepContext.BeginDialogAsync(nameof(GreetingDialog), null, cancellationToken);
    }

    private async Task<DialogTurnResult> SearchKB(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        return await stepContext.BeginDialogAsync(nameof(KnowledgeDialog), null, cancellationToken);        
    }
    private async Task<DialogTurnResult> SearchRetrievalPlugin(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        // Already found the answer in the KB no need to validate
        // from the retrieval
        if (message.FoundInKnowledgeDatabase)
            return await stepContext.EndDialogAsync(null, cancellationToken);

        return await stepContext.BeginDialogAsync(nameof(ExtendedSearchDialog), null, cancellationToken);
    }    
   
    private async Task<DialogTurnResult> FinalStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (!message.FoundInKnowledgeDatabase && !message.FoundInRetrieval)
        {
            var prompt = MessageFactory.Text("Cannot find the answer to your question; an agent will get back to you soon.", inputHint: InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(prompt, cancellationToken);

        }
        return await stepContext.EndDialogAsync(null, cancellationToken);        
    }
}
