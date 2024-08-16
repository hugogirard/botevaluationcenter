using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text;
using System.Threading;

namespace KnowledgeBot.Bots
{
    public class KnowledgeBot : ActivityHandler
    {        
        private readonly Kernel _kernel;
        private readonly ILogger<KnowledgeBot> _logger;
        private Dictionary<string,ChatHistory> _chatHistories;
        private readonly IChatCompletionService _chat;
        private readonly OpenAIPromptExecutionSettings _openAIPromptExecutionSettings;

        public KnowledgeBot(Kernel kernel,
                            IChatCompletionService chatCompletionService,
                            ILogger<KnowledgeBot> logger)
        {
            _kernel = kernel;
            _logger = logger;
            _chatHistories = new Dictionary<string,ChatHistory>();
            _chat = chatCompletionService;

            string systemPrompt = @"You are a chat company assistant, you answer question from your 
                                    native function reply I don't have the information, sorry if you don't know.";

            _openAIPromptExecutionSettings = new()
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                ChatSystemPrompt = systemPrompt,
                MaxTokens = 2000,
                Temperature = 0.7,                
            };
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var question = turnContext.Activity.Text;

            string memberId = turnContext.Activity.Recipient.Id;

            // This should happen but in case we need to create the system prompt
            if (!_chatHistories.ContainsKey(memberId))
            {
                _logger.LogError($"Cannot find member in the history chat: {memberId}");
                _chatHistories.Add(memberId, new ChatHistory());
            }
            
            _chatHistories[memberId].AddUserMessage(question);
            
            var response = await _chat.GetChatMessageContentAsync(_chatHistories[memberId],
                                                                  _openAIPromptExecutionSettings,
                                                                  _kernel);

            string answer = response.Items[0].ToString();

            _chatHistories[memberId].AddAssistantMessage(answer);

            await turnContext.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello, please ask your question!";
            
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    _logger.LogInformation($"Member joined: {member.Id}-{member.Name}");

                    _chatHistories.Add(member.Id, new ChatHistory());
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersRemoved)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    try
                    {
                        // We don't want to crash if we cannot remove the history
                        _chatHistories.Remove(member.Id);
                    }
                    catch
                    {                        
                    }                    
                }
            }

            await Task.FromResult(0);
        }
    }
}
