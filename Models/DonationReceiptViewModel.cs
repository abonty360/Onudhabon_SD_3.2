namespace Onudhabon_ISD.Models
{
    public class DonationReceiptViewModel
    {
        public Donation Donation { get; set; } = new();
        public string OrganizationName { get; set; } = "Onudhabon Educational Foundation";
        public string AccreditationRef { get; set; } = "REG-ONUD-2026-BD";
        public string Helpline { get; set; } = "16247";
        public string ContactEmail { get; set; } = "support@onudhabon.com";
    }
}
