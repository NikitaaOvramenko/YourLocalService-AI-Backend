namespace backend_sk_chat_tcs
{

    using Microsoft.SemanticKernel.ChatCompletion;
    using System.Collections.Concurrent;

   
    public class ChatManager
    {
        private ConcurrentDictionary<string,ChatHistoryWrapper> chats1;


        public ChatManager() {

            chats1 = new ConcurrentDictionary<string,ChatHistoryWrapper>();
        }

        public List<ChatHistoryWrapper> GetChats()
        {
            return chats1.Values.ToList();
        }

        public ChatHistoryWrapper CreateChat(string id, string clientId)
        {
            ChatHistoryWrapper chatHistory = new ChatHistoryWrapper(id)
            {
                ClientId = clientId
            };

            return chatHistory;
        }

        public void AddChat(ChatHistoryWrapper chat)
        {

            //chats.Add(chat);
            chats1.TryAdd(chat.GetID(),chat);
            System.Diagnostics.Debug.WriteLine($"Chat Added !");
            Console.WriteLine($"Chat Added! with id {chat.GetID}");
            GetChats().ForEach(chat => System.Diagnostics.Debug.WriteLine(chat.GetID()));
        }


        public bool RemoveChat(ChatHistoryWrapper chat)
        {
            return chats1.TryRemove(chat.GetID(), out _);
        }

        public bool RemoveChat(string id)
        {
            return chats1.TryRemove(id, out _);
        }


        public ChatHistoryWrapper GetChat(string id)
        {

            if (chats1.TryGetValue(id, out var chat1))
            {
                return chat1;
            }
            return null;


        }

        public void AddUserMessageToChat(ChatHistoryWrapper chat,string message)
        {
            chat.AddUserMessage(message);
        }

        public void AddUserMessageToChat(ChatHistoryWrapper chat, ChatMessageContentItemCollection message)
        {
            chat.AddUserMessage(message);
        }

        public void AddAIMessageToChat(ChatHistoryWrapper chat, string message)
        {
            chat.AddAssistantMessage(message);
        }


        


    }
}
