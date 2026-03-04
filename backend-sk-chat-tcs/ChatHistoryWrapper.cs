using Microsoft.SemanticKernel.ChatCompletion;

namespace backend_sk_chat_tcs
{
    public class ChatHistoryWrapper: ChatHistory
    {

        private string id;
        public string ClientId { get; set; } = string.Empty;

        public ChatHistoryWrapper(string id)
        {
            this.id = id;
        }


        public string GetID()
        {
            return id;
        }

        public void SetId(string id)
        {
            this.id = id;
        }
    }
}
