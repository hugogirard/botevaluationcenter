﻿using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly ILogger<MainDialog> _logger;
    private readonly IStateService _stateService;
    private readonly GreetingDialog _greetingDialog;
    private readonly KnowledgeDialog _knowledgeDialog;

    public MainDialog(ILogger<MainDialog> logger, 
                      GreetingDialog greetingDialog,
                      KnowledgeDialog knowledgeDialog,
                      IStateService stateService) : base(nameof(MainDialog))
    {
        _logger = logger;        
        _stateService = stateService;
        _greetingDialog = greetingDialog;
        _knowledgeDialog = knowledgeDialog;

        InitializeWaterfallDialog();
    }

    private void InitializeWaterfallDialog() 
    {
        var waterfallSteps = new WaterfallStep[]
        {
            InitialStepAsync,
            FindAnswerKnowledgeBase,
            EvaluateAnswerKnowledgeBase,
            FinalStepAsync
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(_greetingDialog);
        AddDialog(_knowledgeDialog);
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
        var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context);

        if (data.FoundInKnowledgeBase)
        {
            var promptMessage = MessageFactory.Text(data.Answer);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
        else
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

}