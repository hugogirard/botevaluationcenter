using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class AuthenticationDialog : ComponentDialog
{
    private readonly IUserRepository _userRepository;
    private readonly IStateService _stateService;
    private readonly ILogger<AuthenticationDialog> _logger;

    public AuthenticationDialog(IStateService stateService,                
                                IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _stateService = stateService;

        var waterfallSteps = new WaterfallStep[]
        {
            AskCredential,
            LoginStep,
        };

        AddDialog(new TextPrompt("AskCredential", EmailValidator));
        AddDialog(new WaterfallDialog(nameof(AuthenticationDialog), waterfallSteps));

        InitialDialogId = $"{nameof(AuthenticationDialog)}";
    }

    private async Task<DialogTurnResult> AskCredential(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        string msg = "Hi, enter you email to login to the assistant system";
        var promptMessage = MessageFactory.Text(msg, msg, InputHints.ExpectingInput);
        return await stepContext.PromptAsync("AskCredential",
                                             new PromptOptions
                                             {
                                                 Prompt = promptMessage,
                                                 RetryPrompt = MessageFactory.Text("Please enter a valid email address")
                                             },
                                             cancellationToken);
    }

    /// <summary>
    /// This is not a real login but mostly mock-up
    /// Since the emulator doesn't support a lot of flow we did this mocking.
    /// DO NOT DO THIS IN PRODUCTION, to see how to implement a real login
    /// check this sample
    /// https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/18.bot-authentication
    /// </summary>    
    private async Task<DialogTurnResult> LoginStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {        
        var email = stepContext.Result?.ToString();


        var user = _userRepository.GetUserByEmail(email);

        // Add info to the session
        var session = await _stateService.SessionAccessor.GetAsync(stepContext.Context);
        session.MemberId = user.Id;
        session.Name = user.Name;

        await _stateService.SessionAccessor.SetAsync(stepContext.Context, session);

        await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Welcome {user.Name}"), cancellationToken);
        await _stateService.UserInfoAccessor.SetAsync(stepContext.Context, user);

        return await stepContext.EndDialogAsync(null, cancellationToken);        

    }

    private Task<bool> EmailValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
    {
        if (promptContext.Recognized.Succeeded)
        {
            string email = promptContext.Recognized.Value;

            if (Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
            {
                // Validate if the user is found
                if (_userRepository.GetUserByEmail(email) is not null)
                {
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }
        return Task.FromResult(false);
    }


}
