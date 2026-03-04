using backend_sk_chat_tcs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace backend_sk_chat_tcs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ChatManager chatManager;
        private SemanticKernel semanticKernel;
        private Supabase.Client supabase;

        public SessionController(ChatManager chatManager, SemanticKernel semanticKernel, Supabase.Client supabase)
        {
            this.chatManager = chatManager;
            this.semanticKernel = semanticKernel;
            this.supabase = supabase;
        }

        [HttpPost("GetSession")]
        public IActionResult GetSessionIdAPI([FromBody] Session req)
        {
            var config = TenantRegistry.Get(req.ClientId);
            if (config == null)
                return BadRequest($"Unknown clientId: '{req.ClientId}'. Valid values: tcs-paints, tcs-junk-removal, appliance-repair");

            var chat = chatManager.CreateChat(req.Id, req.ClientId);
            chat.AddSystemMessage(config.SystemPrompt);
            chatManager.AddChat(chat);

            return Ok(req.Id);
        }

        [HttpPost("EndChat")]
        public IActionResult EndSessionAPI([FromBody] Session req)
        {
            var chat = chatManager.GetChat(req.Id);

            if (chat == null)
            {
                Console.WriteLine($"Chat not exist");
                return NoContent();
            }

            chatManager.RemoveChat(chat);
            Console.WriteLine($"Chat with ID {req.Id} is removed!");

            return NoContent();
        }

        [HttpPost("WriteToChat")]
        public async Task<string> WriteToChat([FromForm] Message req)
        {
            var chatForMessage = chatManager.GetChat(req.Id);
            var config = TenantRegistry.Get(chatForMessage.ClientId);

            var message = new ChatMessageContentItemCollection
            {
                new TextContent(req.MessageT),
            };

            if (req.Image != null && req.Image.Length > 0)
            {
                // Block image uploads for tenants that don't support them
                if (config?.ImageUploadInstruction == null)
                {
                    return JsonSerializer.Serialize(new ResponseFormat
                    {
                        Message = "Image uploads are not supported for this service.",
                        Url = null
                    });
                }

                using var stream = req.Image.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                var bytes = ms.ToArray();
                var storagePath = $"Temp/{Guid.NewGuid()}_{req.Image.FileName}";

                await supabase.Storage
                    .From("media")
                    .Upload(bytes, storagePath, new Supabase.Storage.FileOptions
                    {
                        CacheControl = "3600",
                        Upsert = false
                    });

                var publicUrl = supabase.Storage.From("media").GetPublicUrl(storagePath);

                // Inject tenant-specific image instruction
                // Tokens: {0} = publicUrl, {1} = storagePath, {2} = userMessage
                chatForMessage.AddSystemMessage(
                    string.Format(config.ImageUploadInstruction, publicUrl, storagePath, req.MessageT)
                );
            }

            chatManager.AddUserMessageToChat(chatForMessage, message);

            var settings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = typeof(ResponseFormat),
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            Console.WriteLine($"Chat with ID: {req.Id} (client: {chatForMessage.ClientId})");

            var chatResponse = "";
            var completion = semanticKernel.ChatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory: chatForMessage,
                executionSettings: settings,
                kernel: semanticKernel.GetKernel()
            );

            await foreach (var content in completion)
            {
                Console.Write(content.Content);
                chatResponse += content.Content;
            }

            chatManager.AddAIMessageToChat(chatForMessage, chatResponse);

            return chatResponse;
        }
    }
}
