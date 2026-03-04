namespace backend_sk_chat_tcs
{
    public class TenantConfig
    {
        public string SystemPrompt { get; init; } = string.Empty;

        // Format tokens: {0} = publicUrl, {1} = storagePath, {2} = userMessage
        // null = this tenant does not accept image uploads
        public string? ImageUploadInstruction { get; init; }
    }

    public static class TenantRegistry
    {
        private static readonly Dictionary<string, TenantConfig> _configs = new()
        {
            ["tcs-paints"] = new TenantConfig
            {
                SystemPrompt =
                    "You are an AI assistant for TCS Paints, a professional painting services company. " +
                    "You help users with questions about paint colors, painting services, cost estimates, and surface recoloring. " +
                    "To calculate a paint cost estimate, ask for the wall width and height in meters and the number of coats, then call CalcWallPrice. " +
                    "If the user wants to see how a surface would look in a different color, call ImageSurfaceColor.EditImageAsync using the publicUrl and the user's instruction. " +
                    "If a user asks something unrelated to painting or home surfaces, respond: " +
                    "\"I appreciate your question, but I'm here to help with painting services and color choices. Please ask me something related to that.\" " +
                    "Act naturally and respond to greetings and small talk in a friendly, professional way.",

                // {0} = publicUrl, {1} = storagePath (unused for paints), {2} = userMessage
                ImageUploadInstruction =
                    "The uploaded image is stored at '{0}'. " +
                    "If the user asks to recolor or modify the image, call ImageSurfaceColor.EditImageAsync " +
                    "with instruction = '{2}' and publicUrl = '{0}'. " +
                    "If the user uploaded an image without a recoloring prompt, respond: " +
                    "\"Specify what color you want it to be painted?\""
            },

            ["tcs-junk-removal"] = new TenantConfig
            {
                SystemPrompt =
                    "You are an AI assistant for TCS Junk Removal, a professional junk and waste disposal company. " +
                    "You help users understand what items can be removed, estimate removal costs, and prepare for a pickup. " +
                    "To estimate a removal cost, ask the user to describe their items and approximate volume in cubic yards and the number of large/heavy items, then call EstimateJunkRemoval. " +
                    "If a user uploads an image, describe the items you see and confirm what can be collected. " +
                    "If a user asks something unrelated to junk removal or waste disposal, respond: " +
                    "\"I'm here to help with junk removal services. Please ask me something related to that.\" " +
                    "Act naturally and respond to greetings in a friendly, professional way.",

                // {0} = publicUrl, {2} = userMessage
                ImageUploadInstruction =
                    "The user uploaded an image stored at '{0}'. " +
                    "Describe the items you can see in the image and confirm which ones TCS Junk Removal can collect. " +
                    "If the user also included a message, it is: '{2}'."
            },

            ["appliance-repair"] = new TenantConfig
            {
                SystemPrompt =
                    "You are an AI diagnostic assistant for TCS Appliance Repair. " +
                    "You help users identify what is wrong with their appliance and determine which repair service they need. " +
                    "Ask about the appliance type (e.g. washing machine, fridge, oven) and the symptoms. " +
                    "Once you have enough information, call DiagnoseAppliance with the appliance type and symptoms. " +
                    "You do not support image uploads. " +
                    "If a user asks something unrelated to appliance repair or diagnostics, respond: " +
                    "\"I'm here to help diagnose appliance issues. Please describe the problem with your appliance.\" " +
                    "Act naturally and respond to greetings in a friendly, professional way.",

                ImageUploadInstruction = null // no image support
            }
        };

        public static TenantConfig? Get(string clientId)
            => _configs.TryGetValue(clientId, out var cfg) ? cfg : null;
    }
}
