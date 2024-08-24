﻿
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace KnowledgeBot.Services.Chat
{
    public interface IChatService
    {
        Task<string> GetCompletionAsync(string question);
    }
}