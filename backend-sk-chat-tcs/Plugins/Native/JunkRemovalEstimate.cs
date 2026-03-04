using backend_sk_chat_tcs.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace backend_sk_chat_tcs.Plugins.Native
{
    public class JunkRemovalEstimate
    {
        [KernelFunction, Description("Estimates the cost of a junk removal job based on volume and heavy items")]
        [return: Description("Estimated price for the junk removal job")]
        public Task<ResponseFormat> EstimateJunkRemoval(
            [Description("Estimated volume of junk in cubic yards")] double volumeCubicYards,
            [Description("Number of large or heavy items such as furniture or appliances")] int heavyItemCount)
        {
            var ratePerCubicYard = 80.0;    // base rate per cubic yard
            var heavyItemSurcharge = 30.0;  // extra per large/heavy item
            var total = (volumeCubicYards * ratePerCubicYard) + (heavyItemCount * heavyItemSurcharge);

            return Task.FromResult(new ResponseFormat
            {
                Message = $"Estimated removal cost: ${total:F2} " +
                          $"({volumeCubicYards} cu yd × ${ratePerCubicYard} + {heavyItemCount} heavy item(s) × ${heavyItemSurcharge})",
                Url = null
            });
        }
    }
}
