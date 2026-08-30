using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;

namespace Onudhabon_ISD.Controllers
{
    public class DonationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DonationController> _logger;

        public DonationController(ApplicationDbContext context, ILogger<DonationController> logger)
        {
            _context = context;
            _logger = logger;
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
                // Validate bKash wallet number
                var cleanWallet = model.BkashWalletNumber.Trim();
                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanWallet, @"^(?:\+?880|0)1[3-9]\d{8}$"))
                {
                    return Json(new { success = false, message = "Invalid bKash account number. Must be a valid 11-digit Bangladeshi mobile number." });
                }

                // Validate OTP (in bKash simulation, must be non-empty and 4-6 digits)
                if (string.IsNullOrWhiteSpace(model.OtpCode) || model.OtpCode.Trim().Length < 4)
                {
                    return Json(new { success = false, message = "Please provide the valid bKash OTP verification code." });
                }

                // Validate PIN (in bKash gateway simulation, must be 5 digits)
                if (string.IsNullOrWhiteSpace(model.Pin) || model.Pin.Trim().Length != 5 || !model.Pin.All(char.IsDigit))
                {
                    return Json(new { success = false, message = "Invalid bKash PIN. Must be a 5-digit numerical PIN." });
                }

                // Generate authentic-looking bKash Transaction ID (e.g., TRX9K2L4P8)
                var random = new Random();
                const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
                var trxSuffix = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                var bkashTrxId = $"TRX{trxSuffix}";
                var bkashPaymentId = $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{random.Next(1000, 9999)}";

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
                    PaymentMethod = "bKash",
                    BkashWalletNumber = cleanWallet,
                    BkashTransactionId = bkashTrxId,
                    BkashPaymentId = bkashPaymentId,
                    Status = "Completed",
                    IsAnonymous = model.IsAnonymous,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    __v = 0
                };

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Donation successful: ID {Id}, TRXID {TrxId}, Amount ৳{Amount}", donation.Id, bkashTrxId, donation.Amount);

                return Json(new
                {
                    success = true,
                    donationId = donation.Id,
                    transactionId = bkashTrxId,
                    paymentId = bkashPaymentId,
                    amount = donation.Amount,
                    redirectUrl = Url.Action("Receipt", "Donation", new { id = donation.Id })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bKash donation payment.");
                return Json(new { success = false, message = "Payment processing failed. Please try again later." });
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
    }
}
