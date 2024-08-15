using Azure.AI.Language.QuestionAnswering;
using Azure;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Generic;

namespace KnowledgeBot.Services;

public class KnowledgeBaseService : IKnowledgeBaseService
{
    QuestionAnsweringClient _client;
    QuestionAnsweringProject _project;
    private readonly ILogger<KnowledgeBaseService> _logger;

    public KnowledgeBaseService(IConfiguration configuration, ILogger<KnowledgeBaseService> logger)
    {
        Uri endpoint = new Uri(configuration["LANGUAGESRV:ENDPOINT"]);
        AzureKeyCredential credential = new AzureKeyCredential(configuration["LANGUAGESRV:KEY"]);
        string projectName = configuration["LANGUAGESRV:PROJECT_NAME"];
        string deploymentName = "production";

        _client = new(endpoint, credential);
        _project = new(projectName, deploymentName);
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetAnswersAsync(string question)
    {
        List<string> answers = new();
        try
        {
            Response<AnswersResult> response = await _client.GetAnswersAsync(question, _project);

            foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
            {
                answers.Add(answer.Answer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return answers;
    }
}
