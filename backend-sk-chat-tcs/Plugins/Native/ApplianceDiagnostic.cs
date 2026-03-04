using backend_sk_chat_tcs.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace backend_sk_chat_tcs.Plugins.Native
{
    public class ApplianceDiagnostic
    {
        [KernelFunction, Description("Suggests the appropriate repair service based on appliance type and symptoms")]
        [return: Description("Recommended repair service and next steps for the user")]
        public Task<ResponseFormat> DiagnoseAppliance(
            [Description("Type of appliance, e.g. washing machine, refrigerator, oven, dishwasher, dryer")] string applianceType,
            [Description("Symptoms or issues described by the user")] string symptoms)
        {
            var type = applianceType.ToLower();

            var service = type switch
            {
                var t when t.Contains("washer") || t.Contains("washing") => "Laundry Appliance Repair",
                var t when t.Contains("dryer")                           => "Laundry Appliance Repair",
                var t when t.Contains("fridge") || t.Contains("refrigerator") => "Refrigeration Repair",
                var t when t.Contains("oven") || t.Contains("stove") || t.Contains("range") => "Cooking Appliance Repair",
                var t when t.Contains("dishwasher")                     => "Dishwasher Repair",
                var t when t.Contains("install")                        => "Appliance Installation",
                _                                                        => "General Appliance Repair"
            };

            return Task.FromResult(new ResponseFormat
            {
                Message = $"Based on your description, we recommend booking a **{service}** appointment. " +
                          $"Noted symptoms: {symptoms}. " +
                          $"Please fill out the service request form or call us to schedule a visit.",
                Url = null
            });
        }
    }
}
