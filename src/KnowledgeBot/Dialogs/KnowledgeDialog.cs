﻿using Microsoft.Bot.Builder;
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
                SearchInKnowledgeBase                
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
            string completion = $"{kbResponse.KbName}: {kbResponse.Answer}";

            message.Completion = completion;
            message.FoundInKnowledgeDatabase = !kbResponse.Error; // Indicate if we have an error
            message.KnowledgeBaseName = kbResponse.Error ? string.Empty : kbResponse.KbName;
                        
            await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);                        
        }
        else 
        {
            message.FoundInKnowledgeDatabase = false;
            await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);            
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
