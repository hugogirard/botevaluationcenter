using System;

namespace KnowledgeBot.Models
{
    public class Message
    {
        public string Id { get; }

        public string Type { get; } = "Message";

        public DateTime TimeStamp { get; set; }

        public string Prompt { get; set; }

        public string Completion { get; set; }

        public Message()
        {
            Id = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }
    }
}
