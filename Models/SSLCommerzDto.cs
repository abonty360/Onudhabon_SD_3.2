using System.Text.Json.Serialization;

namespace Onudhabon.Models
{
    public class SSLCommerzInitResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("failedreason")]
        public string? FailedReason { get; set; }

        [JsonPropertyName("sessionkey")]
        public string? SessionKey { get; set; }

        [JsonPropertyName("GatewayPageURL")]
        public string? GatewayPageURL { get; set; }

        [JsonPropertyName("tran_id")]
        public string? TranId { get; set; }
    }

    public class SSLCommerzValidationResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("tran_date")]
        public string? TranDate { get; set; }

        [JsonPropertyName("tran_id")]
        public string? TranId { get; set; }

        [JsonPropertyName("val_id")]
        public string? ValId { get; set; }

        [JsonPropertyName("amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("store_amount")]
        public string? StoreAmount { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("bank_tran_id")]
        public string? BankTranId { get; set; }

        [JsonPropertyName("card_type")]
        public string? CardType { get; set; }

        [JsonPropertyName("card_no")]
        public string? CardNo { get; set; }

        [JsonPropertyName("card_issuer")]
        public string? CardIssuer { get; set; }

        [JsonPropertyName("card_brand")]
        public string? CardBrand { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
