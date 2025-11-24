using System;

namespace CodenamesGame.Domain.POCO
{
    /// <summary>
    /// Represents a message in the chat, distinguishing between the local user and the remote user.
    /// </summary>
    public class ChatMessageDM
    {
        public string Message { get; set; }
        public string Author { get; set; }
        public bool IsMine { get; set; }
        public DateTime Timestamp { get; set; }
    }
}