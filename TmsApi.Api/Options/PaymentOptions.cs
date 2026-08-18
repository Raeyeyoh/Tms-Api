
using System.ComponentModel.DataAnnotations;

namespace TmsApi.Api.Options;

public class PaymentOptions
{
   required public string GetwayUrl { get; init; }
   [Range(100, 10000)] public decimal MaxDipositBirr { get; init; }
}