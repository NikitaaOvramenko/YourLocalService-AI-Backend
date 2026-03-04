using backend_sk_chat_tcs;
using dotenv.net;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Supabase;

DotEnv.Load();

AppContext.SetSwitch("System.Diagnostics.Tracing.EventSource.IsSupported", false);

Console.WriteLine("🔎 Environment check:");
Console.WriteLine("SUPABASE_URL=" + Environment.GetEnvironmentVariable("SUPBASE_URL"));
Console.WriteLine("SUPABASE_KEY=" + Environment.GetEnvironmentVariable("SUPBASE_KEY"));
Console.WriteLine("OPENAI_APIKEY=" + Environment.GetEnvironmentVariable("OPENAI_APIKEY"));

var builder = WebApplication.CreateBuilder(args);
var page_urls_raw = Environment.GetEnvironmentVariable("PAGE_URLS") ?? string.Empty;
var page_urls = page_urls_raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
Console.WriteLine($"PAGE_URLS: {page_urls_raw}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
    {
        policy.WithOrigins(page_urls)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SemanticKernel init with guards
try
{
    Console.WriteLine("Step 1: Initializing SemanticKernel...");

    var modelName = Environment.GetEnvironmentVariable("MODEL_NAME")
        ?? throw new InvalidOperationException("MODEL_NAME not set");
    var openAiKey = Environment.GetEnvironmentVariable("OPENAI_APIKEY")
        ?? throw new InvalidOperationException("OPENAI_APIKEY not set");

    builder.Services.AddSingleton(new SemanticKernel(modelName, openAiKey));

    Console.WriteLine("✅ SemanticKernel registered.");
}
catch (Exception ex)
{
    Console.WriteLine("❌ SemanticKernel init failed: " + ex.GetType().FullName + " — " + ex.Message);
    throw;
}

builder.Services.AddSingleton<ChatManager>();

// Supabase init with guards
builder.Services.AddSingleton(provider =>
{
    try
    {
        Console.WriteLine("Step 2: Initializing Supabase client...");

        var url = Environment.GetEnvironmentVariable("SUPBASE_URL")
            ?? throw new InvalidOperationException("SUPBASE_URL not set");
        var key = Environment.GetEnvironmentVariable("SUPBASE_KEY")
            ?? throw new InvalidOperationException("SUPABASE_KEY not set");

        var supabase = new Supabase.Client(url, key, new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true
        });

        // Ensure client is initialized
        supabase.InitializeAsync().Wait();

        Console.WriteLine("✅ Supabase client initialized.");
        return supabase;
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Supabase init failed: " + ex.GetType().FullName + " — " + ex.Message);
        throw;
    }
});

var app = builder.Build();

app.UseCors("AllowReactDev");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Application starting...");
app.Run();
