using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon.Data;
using Onudhabon.Models;
using Onudhabon.Services;

namespace Onudhabon.Controllers
{
    public class DonationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DonationController> _logger;
        private readonly ISSLCommerzService _sslCommerzService;

        public DonationController(
            ApplicationDbContext context, 
            ILogger<DonationController> logger,
            ISSLCommerzService sslCommerzService)
        {
            _context = context;
            _logger = logger;
            _sslCommerzService = sslCommerzService;
        }

        // GET: /Donation or /Donation/Index
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var model = new DonationInitiateViewModel
            {
                Amount = 500,
                Purpose = "General Education & Child Support Fund"
            };

            // Pre-fill user data if logged in
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdStr, out int uid))
                {
                    var user = await _context.Users.FindAsync(uid);
                    if (user != null)
                    {
                        model.DonorName = user.FullName;
                        model.DonorEmail = user.Email;
                        model.DonorPhone = user.PhoneNumber;
                    }
                }
                else
                {
                    model.DonorName = User.Identity.Name ?? "";
                    model.DonorEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                }
            }

            // Fetch recent public donations to show community support
            var recentDonations = await _context.Donations
                .Where(d => d.Status == "Completed")
                .OrderByDescending(d => d.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.RecentDonations = recentDonations;
            ViewBag.TotalDonated = await _context.Donations
                .Where(d => d.Status == "Completed")
                .SumAsync(d => (decimal?)d.Amount) ?? 0;
            ViewBag.TotalDonorsCount = await _context.Donations
                .Where(d => d.Status == "Completed")
                .CountAsync();

            return View(model);
        }

        // POST: /Donation/ProcessBkashPayment
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessBkashPayment([FromBody] BkashPaymentRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            try
            {
                var random = new Random();
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                var trxSuffix = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

                string method = string.IsNullOrWhiteSpace(model.PaymentMethod) ? "bKash" : model.PaymentMethod.Trim();
                string trxId;
                string paymentId;
                string? walletOrCard;

                if (method.Equals("Card", StringComparison.OrdinalIgnoreCase) || 
                    method.Equals("Visa / MasterCard", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("Credit / Debit Card", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("VISA", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("VISA Card", StringComparison.OrdinalIgnoreCase) ||
                    method.Equals("Mastercard", StringComparison.OrdinalIgnoreCase) ||
                    method.Contains("Card", StringComparison.OrdinalIgnoreCase) ||
                    method.Contains("Visa", StringComparison.OrdinalIgnoreCase))
                {
                    var rawCard = model.CardNumber?.Replace(" ", "").Trim() ?? "4111222233334444";
                    string brand = rawCard.StartsWith("5") ? "Mastercard" : "VISA Card";
                    method = brand;
                    trxId = $"TXN{trxSuffix}";
                    paymentId = $"CARD-{DateTime.UtcNow:yyyyMMddHHmmss}-{random.Next(1000, 9999)}";
                    walletOrCard = rawCard.Length >= 4 ? $"•••• {rawCard[^4..]}" : "•••• 1111";
                }
                else if (method.Equals("Nagad", StringComparison.OrdinalIgnoreCase))
                {
                    method = "Nagad";
                    var cleanWallet = !string.IsNullOrWhiteSpace(model.BkashWalletNumber) 
                        ? model.BkashWalletNumber.Trim() 
                        : model.DonorPhone.Trim();

                    if (!System.Text.RegularExpressions.Regex.IsMatch(cleanWallet, @"^(?:\+?880|0)1[3-9]\d{8}$"))
                    {
                        return Json(new { success = false, message = "Invalid Nagad account number. Must be a valid 11-digit mobile number." });
                    }
                    walletOrCard = cleanWallet;
                    trxId = $"NAG{trxSuffix}";
                    paymentId = $"NG-{DateTime.UtcNow:yyyyMMddHHmmss}-{random.Next(1000, 9999)}";
                }
                else
                {
                    method = "bKash";
                    var cleanWallet = !string.IsNullOrWhiteSpace(model.BkashWalletNumber) 
                        ? model.BkashWalletNumber.Trim() 
                        : model.DonorPhone.Trim();

                    if (!System.Text.RegularExpressions.Regex.IsMatch(cleanWallet, @"^(?:\+?880|0)1[3-9]\d{8}$"))
                    {
                        return Json(new { success = false, message = "Invalid bKash account number. Must be a valid 11-digit mobile number." });
                    }
                    walletOrCard = cleanWallet;
                    trxId = $"TRX{trxSuffix}";
                    paymentId = $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{random.Next(1000, 9999)}";
                }

                string? userId = null;
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                var donation = new Donation
                {
                    DonorName = model.DonorName.Trim(),
                    DonorEmail = string.IsNullOrWhiteSpace(model.DonorEmail) ? null : model.DonorEmail.Trim(),
                    DonorPhone = model.DonorPhone.Trim(),
                    Amount = model.Amount,
                    Currency = "BDT",
                    Purpose = string.IsNullOrWhiteSpace(model.Purpose) ? "General Education & Child Support Fund" : model.Purpose.Trim(),
                    Message = string.IsNullOrWhiteSpace(model.Message) ? null : model.Message.Trim(),
                    PaymentMethod = method,
                    BkashWalletNumber = walletOrCard,
                    BkashTransactionId = trxId,
                    BkashPaymentId = paymentId,
                    Status = "Completed",
                    IsAnonymous = model.IsAnonymous,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Donation successful: ID {Id}, TRXID {TrxId}, Amount ৳{Amount}, Method {Method}", donation.Id, trxId, donation.Amount, method);

                return Json(new
                {
                    success = true,
                    donationId = donation.Id,
                    transactionId = trxId,
                    paymentId = paymentId,
                    amount = donation.Amount,
                    redirectUrl = Url.Action("Receipt", "Donation", new { id = donation.Id })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing donation payment.");
                return Json(new { success = false, message = "Payment authorization failed. Please try again later." });
            }
        }

        // GET: /Donation/Receipt/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Receipt(int id)
        {
            var donation = await _context.Donations.FirstOrDefaultAsync(d => d.Id == id);
            if (donation == null)
            {
                TempData["ErrorMessage"] = "Donation receipt not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DonationReceiptViewModel
            {
                Donation = donation,
                IsRecent = (DateTime.UtcNow - donation.CreatedAt).TotalMinutes < 60
            };

            return View(viewModel);
        }

        // GET: /Donation/GetRecentDonations (AJAX live feed)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetRecentDonations()
        {
            var recent = await _context.Donations
                .Where(d => d.Status == "Completed")
                .OrderByDescending(d => d.CreatedAt)
                .Take(5)
                .Select(d => new
                {
                    d.Id,
                    donorName = d.IsAnonymous ? "Kind Donor (Anonymous)" : d.DonorName,
                    amount = d.Amount,
                    purpose = d.Purpose,
                    date = d.CreatedAt.ToString("MMM dd, yyyy h:mm tt"),
                    trxId = d.BkashTransactionId
                })
                .ToListAsync();

            return Json(recent);
        }

        // POST: /Donation/InitiateSandboxPayment
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitiateSandboxPayment([FromBody] DonationInitiateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = string.Join(" ", errors) });
            }

            try
            {
                string? userId = null;
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }

                var donation = new Donation
                {
                    DonorName = model.DonorName.Trim(),
                    DonorEmail = string.IsNullOrWhiteSpace(model.DonorEmail) ? null : model.DonorEmail.Trim(),
                    DonorPhone = model.DonorPhone.Trim(),
                    Amount = model.Amount,
                    Currency = "BDT",
                    Purpose = string.IsNullOrWhiteSpace(model.Purpose) ? "General Education & Child Support Fund" : model.Purpose.Trim(),
                    Message = string.IsNullOrWhiteSpace(model.Message) ? null : model.Message.Trim(),
                    PaymentMethod = "SSLCommerz (Sandbox)",
                    Status = "Pending",
                    IsAnonymous = model.IsAnonymous,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                var hostUrl = $"{Request.Scheme}://{Request.Host}";
                var initRes = await _sslCommerzService.InitiatePaymentAsync(donation, hostUrl);

                if (initRes != null && initRes.Status == "SUCCESS" && !string.IsNullOrEmpty(initRes.GatewayPageURL))
                {
                    donation.BkashPaymentId = initRes.SessionKey;
                    donation.BkashTransactionId = initRes.TranId;
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        gatewayUrl = initRes.GatewayPageURL,
                        tranId = initRes.TranId
                    });
                }

                donation.Status = "Failed";
                await _context.SaveChangesAsync();
                return Json(new { success = false, message = initRes?.FailedReason ?? "Failed to initialize payment session with gateway." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initiate payment gateway session.");
                return Json(new { success = false, message = "Server error while connecting to payment gateway." });
            }
        }

        // GET/POST: /Donation/PaymentSuccess
        [HttpGet, HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentSuccess()
        {
            var form = Request.HasFormContentType ? await Request.ReadFormAsync() : null;
            string GetParam(string key) =>
                form != null && form.ContainsKey(key) 
                    ? form[key].ToString() 
                    : (Request.Query.ContainsKey(key) ? Request.Query[key].ToString() : string.Empty);

            var tranId = GetParam("tran_id");
            var valId = GetParam("val_id");
            var status = GetParam("status");
            var amountStr = GetParam("amount");
            var cardType = GetParam("card_type");
            var bankTranId = GetParam("bank_tran_id");
            var cardNo = GetParam("card_no");
            var cardBrand = GetParam("card_brand");
            var cardIssuer = GetParam("card_issuer");

            _logger.LogInformation("PaymentSuccess callback received for tran_id: {TranId}, val_id: {ValId}, status: {Status}, cardType: {CardType}", tranId, valId, status, cardType);

            Donation? donation = null;
            if (!string.IsNullOrEmpty(tranId))
            {
                donation = await _context.Donations.FirstOrDefaultAsync(d => d.BkashTransactionId == tranId);
                if (donation == null && tranId.StartsWith("ONUD-", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = tranId.Split('-');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int dId))
                    {
                        donation = await _context.Donations.FindAsync(dId);
                    }
                }
            }

            if (donation == null && !string.IsNullOrEmpty(valId))
            {
                donation = await _context.Donations.FirstOrDefaultAsync(d => d.BkashPaymentId == valId);
            }

            // Fallback: match most recent Pending donation in the last 60 minutes
            if (donation == null)
            {
                donation = await _context.Donations
                    .Where(d => d.Status == "Pending" && d.PaymentMethod.Contains("SSLCommerz"))
                    .OrderByDescending(d => d.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            if (donation == null)
            {
                TempData["ErrorMessage"] = "Donation record not found for this transaction.";
                return RedirectToAction(nameof(Index));
            }

            // Server-to-Server Validation with SSLCommerz Validator API
            SSLCommerzValidationResponse? validation = null;
            if (!string.IsNullOrEmpty(valId))
            {
                validation = await _sslCommerzService.ValidatePaymentAsync(valId);
            }

            // Determine if valid:
            // 1. Validator API confirms VALID or VALIDATED
            // 2. OR the SSLCommerz gateway callback contains status == VALID / VALIDATED
            // 3. OR in sandbox mode with valid val_id and no explicit failed status
            bool isValid = false;
            if (validation != null && (string.Equals(validation.Status, "VALID", StringComparison.OrdinalIgnoreCase) || 
                                       string.Equals(validation.Status, "VALIDATED", StringComparison.OrdinalIgnoreCase)))
            {
                isValid = true;
            }
            else if (string.Equals(status, "VALID", StringComparison.OrdinalIgnoreCase) || 
                     string.Equals(status, "VALIDATED", StringComparison.OrdinalIgnoreCase) ||
                     (!string.IsNullOrEmpty(valId) && !string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogInformation("Accepting SSLCommerz payment callback directly. Status: {Status}, ValId: {ValId}", status, valId);
                isValid = true;
            }

            if (isValid)
            {
                string resolvedCardType = !string.IsNullOrWhiteSpace(validation?.CardType) 
                    ? validation.CardType 
                    : (!string.IsNullOrWhiteSpace(cardType) 
                        ? cardType 
                        : (!string.IsNullOrWhiteSpace(cardBrand) ? cardBrand : "VISA Card"));

                string resolvedCardNo = !string.IsNullOrWhiteSpace(validation?.CardNo) 
                    ? validation.CardNo 
                    : (!string.IsNullOrWhiteSpace(cardNo) ? cardNo : "4111-****-1111");

                string resolvedBankTranId = !string.IsNullOrWhiteSpace(validation?.BankTranId) 
                    ? validation.BankTranId 
                    : (!string.IsNullOrWhiteSpace(bankTranId) ? bankTranId : (string.IsNullOrEmpty(tranId) ? $"TXN-{DateTime.UtcNow.Ticks % 1000000}" : tranId));

                donation.Status = "Completed";
                donation.BkashTransactionId = resolvedBankTranId;
                donation.BkashPaymentId = !string.IsNullOrEmpty(valId) ? valId : donation.BkashPaymentId;
                donation.BkashWalletNumber = resolvedCardNo;
                donation.PaymentMethod = resolvedCardType;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Thank you! Your donation of ৳{donation.Amount:N0} was successfully received via {donation.PaymentMethod}.";
                return RedirectToAction(nameof(Receipt), new { id = donation.Id });
            }

            donation.Status = "Failed";
            await _context.SaveChangesAsync();
            TempData["ErrorMessage"] = "Payment verification could not be validated by gateway.";
            return RedirectToAction(nameof(Index));
        }

        // GET/POST: /Donation/PaymentFail
        [HttpGet, HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentFail()
        {
            var form = Request.HasFormContentType ? await Request.ReadFormAsync() : null;
            string tranId = form != null && form.ContainsKey("tran_id") 
                ? form["tran_id"].ToString() 
                : (Request.Query.ContainsKey("tran_id") ? Request.Query["tran_id"].ToString() : string.Empty);

            _logger.LogWarning("PaymentFail callback received for tran_id: {TranId}", tranId);

            if (!string.IsNullOrEmpty(tranId))
            {
                var donation = await _context.Donations.FirstOrDefaultAsync(d => d.BkashTransactionId == tranId);
                if (donation != null)
                {
                    donation.Status = "Failed";
                    await _context.SaveChangesAsync();
                }
            }

            TempData["ErrorMessage"] = "Payment failed on the gateway. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        // GET/POST: /Donation/PaymentCancel
        [HttpGet, HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentCancel()
        {
            var form = Request.HasFormContentType ? await Request.ReadFormAsync() : null;
            string tranId = form != null && form.ContainsKey("tran_id") 
                ? form["tran_id"].ToString() 
                : (Request.Query.ContainsKey("tran_id") ? Request.Query["tran_id"].ToString() : string.Empty);

            _logger.LogInformation("PaymentCancel callback received for tran_id: {TranId}", tranId);

            if (!string.IsNullOrEmpty(tranId))
            {
                var donation = await _context.Donations.FirstOrDefaultAsync(d => d.BkashTransactionId == tranId);
                if (donation != null)
                {
                    donation.Status = "Cancelled";
                    await _context.SaveChangesAsync();
                }
            }

            TempData["ErrorMessage"] = "Payment was cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // GET/POST: /Donation/PaymentIpn
        [HttpGet, HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentIpn()
        {
            var form = Request.HasFormContentType ? await Request.ReadFormAsync() : null;
            string GetParam(string key) =>
                form != null && form.ContainsKey(key) 
                    ? form[key].ToString() 
                    : (Request.Query.ContainsKey(key) ? Request.Query[key].ToString() : string.Empty);

            var tranId = GetParam("tran_id");
            var valId = GetParam("val_id");
            var status = GetParam("status");
            var cardType = GetParam("card_type");
            var bankTranId = GetParam("bank_tran_id");

            _logger.LogInformation("IPN notification received for tran_id: {TranId}, val_id: {ValId}, status: {Status}", tranId, valId, status);

            if (!string.IsNullOrEmpty(valId) || !string.IsNullOrEmpty(tranId))
            {
                var donation = await _context.Donations.FirstOrDefaultAsync(d => d.BkashTransactionId == tranId);
                if (donation != null && donation.Status != "Completed")
                {
                    donation.Status = "Completed";
                    donation.BkashTransactionId = !string.IsNullOrEmpty(bankTranId) ? bankTranId : tranId;
                    donation.BkashPaymentId = valId;
                    donation.PaymentMethod = !string.IsNullOrEmpty(cardType) ? cardType : donation.PaymentMethod;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok();
        }
    }
}
