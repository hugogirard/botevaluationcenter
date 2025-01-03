﻿using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace KnowledgeBot.Models;

public class KnowledgeBaseConfiguration 
{
    public IEnumerable<KnowledgeBase> KnowledgeConfiguration { get; set; }
} 

public record KnowledgeBase(string name, string appRoles, string displayName);
