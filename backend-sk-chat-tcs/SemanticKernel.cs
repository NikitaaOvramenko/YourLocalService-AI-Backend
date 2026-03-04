namespace backend_sk_chat_tcs
{
    using backend_sk_chat_tcs.Plugins.Native;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Supabase;
    using System.Reflection;

    public class SemanticKernel
    {
        private readonly string modelName;
        private readonly string apiKey;
        private readonly Kernel kernel;
        private readonly IChatCompletionService chatService;

        public SemanticKernel(string modelName, string apiKey)
        {
            this.modelName = modelName;
            this.apiKey = apiKey;

            var url = Environment.GetEnvironmentVariable("SUPBASE_URL");
            var key = Environment.GetEnvironmentVariable("SUPBASE_KEY");

            var supabase = new Supabase.Client(url, key, new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            });

            // 🔑 Ensure client is initialized before use
            supabase.InitializeAsync().Wait();



            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(modelName, apiKey);


            builder.Plugins.AddFromType<Estimate>();
            builder.Plugins.AddFromType<JunkRemovalEstimate>();
            builder.Plugins.AddFromType<ApplianceDiagnostic>();
            builder.Plugins.AddFromObject(new ImageSurfaceColor(supabase));
           

            //var pathToImageToImagePlugin = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "Semantic","ImageToImagePlugin");
            //builder.Plugins.AddFromPromptDirectory(pathToImageToImagePlugin,"ImageToImagePlugin");

            this.kernel = builder.Build(); 
            this.chatService = kernel.GetRequiredService<IChatCompletionService>();

            
        }
        public Kernel GetKernel() => this.kernel;
        public IChatCompletionService ChatCompletionService => this.chatService;
    }

}

