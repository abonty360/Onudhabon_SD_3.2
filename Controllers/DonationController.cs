using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;
using System.Security.Claims;

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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            decimal totalDonated = 0m;
            int totalDonorsCount = 0;
            var recentDonations = new List<Donation>();

            try
            {
                // Ensure table exists in database
                await EnsureDonationsTableExistsAsync();

                totalDonated = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .SumAsync(d => (decimal?)d.Amount) ?? 0m;

                totalDonorsCount = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .CountAsync();

                recentDonations = await _context.Donations
                    .Where(d => d.PaymentStatus == "Completed")
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(6)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch donations from database. Providing fallback defaults.");
            }

            ViewBag.TotalDonated = totalDonated;
            ViewBag.TotalDonorsCount = totalDonorsCount;
            ViewBag.RecentDonations = recentDonations;

            // Pre-fill user details if logged in
            var model = new DonationInitiateViewModel { Amount = 500 };
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                model.DonorName = User.Identity.Name ?? string.Empty;
                model.DonorEmail = User.FindFirstValue(ClaimTypes.Email);
                model.DonorPhone = User.FindFirst("PhoneNumber")?.Value ?? string.Empty;
            }

            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ProcessBkashPayment([FromBody] BkashPaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid payment request." });
            }

            if (string.IsNullOrWhiteSpace(request.DonorName))
            {
                return BadRequest(new { success = false, message = "Donor full name is required." });
            }

            if (string.IsNullOrWhiteSpace(request.DonorPhone))
            {
                return BadRequest(new { success = false, message = "Donor phone number is required." });
            }

            if (request.Amount < 10)
            {
                return BadRequest(new { success = false, message = "Minimum contribution amount is ৳10." });
            }

            if (string.IsNullOrWhiteSpace(request.BkashWalletNumber) || request.BkashWalletNumber.Length != 11 || !request.BkashWalletNumber.StartsWith("01"))
            {
                return BadRequest(new { success = false, message = "Please enter a valid 11-digit bKash account number (e.g. 017XXXXXXXX)." });
            }

            if (string.IsNullOrWhiteSpace(request.Pin) || request.Pin.Length != 5)
            {
                return BadRequest(new { success = false, message = "Please enter your 5-digit bKash PIN." });
            }

            var transactionId = "TRX" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Random.Shared.Next(100, 999);
            var invoiceNumber = "INV-" + DateTime.UtcNow.ToString("yyMMdd") + "-" + Random.Shared.Next(1000, 9999);

            var donation = new Donation
            {
                DonorName = request.DonorName.Trim(),
                DonorEmail = string.IsNullOrWhiteSpace(request.DonorEmail) ? null : request.DonorEmail.Trim(),
                DonorPhone = request.DonorPhone.Trim(),
                Amount = request.Amount,
                Purpose = string.IsNullOrWhiteSpace(request.Purpose) ? "General Education & Child Support Fund" : request.Purpose.Trim(),
                Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
                IsAnonymous = request.IsAnonymous,
                TransactionId = transactionId,
                InvoiceNumber = invoiceNumber,
                PaymentMethod = "bKash",
                PaymentStatus = "Completed",
                BkashWalletNumber = request.BkashWalletNumber.Trim(),
                CreatedAt = DateTime.UtcNow,
                __v = 0
            };

            try
            {
                await EnsureDonationsTableExistsAsync();
                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist donation record to database.");
            }

            return Ok(new
            {
                success = true,
                message = "Contribution processed successfully!",
                transactionId = donation.TransactionId,
                invoiceNumber = donation.InvoiceNumber,
                redirectUrl = Url.Action(nameof(Receipt), new { id = donation.TransactionId })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Receipt(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(nameof(Index));
            }

            Donation? donation = null;
            try
            {
                await EnsureDonationsTableExistsAsync();

                if (int.TryParse(id, out int numId))
                {
                    donation = await _context.Donations
                        .FirstOrDefaultAsync(d => d.Id == numId || d.TransactionId == id || d.InvoiceNumber == id);
                }
                else
                {
                    donation = await _context.Donations
                        .FirstOrDefaultAsync(d => d.TransactionId == id || d.InvoiceNumber == id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch donation by transaction ID {Id}", id);
            }

            if (donation == null)
            {
                // Fallback demo donation if not found
                donation = new Donation
                {
                    TransactionId = id,
                    InvoiceNumber = "INV-" + DateTime.UtcNow.ToString("yyMMdd") + "-0001",
                    DonorName = User.Identity?.Name ?? "Valued Benefactor",
                    DonorPhone = "017XXXXXXXX",
                    Amount = 500,
                    Purpose = "General Education & Child Support Fund",
                    PaymentMethod = "bKash",
                    PaymentStatus = "Completed",
                    CreatedAt = DateTime.UtcNow
                };
            }

            var viewModel = new DonationReceiptViewModel
            {
                Donation = donation
            };

            return View(viewModel);
        }

        private async Task EnsureDonationsTableExistsAsync()
        {
            try
            {
                const string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Donations')
BEGIN
    CREATE TABLE [Donations] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [DonorName] NVARCHAR(150) NOT NULL,
        [DonorEmail] NVARCHAR(256) NULL,
        [DonorPhone] NVARCHAR(50) NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [Purpose] NVARCHAR(255) NOT NULL,
        [Message] NVARCHAR(1000) NULL,
        [IsAnonymous] BIT NOT NULL DEFAULT 0,
        [TransactionId] NVARCHAR(100) NOT NULL,
        [InvoiceNumber] NVARCHAR(100) NOT NULL,
        [PaymentMethod] NVARCHAR(50) NOT NULL DEFAULT 'bKash',
        [PaymentStatus] NVARCHAR(50) NOT NULL DEFAULT 'Completed',
        [BkashWalletNumber] NVARCHAR(50) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [__v] INT NULL DEFAULT 0
    );

    -- Seed initial sample donations
    INSERT INTO [Donations] ([DonorName], [DonorPhone], [Amount], [Purpose], [IsAnonymous], [TransactionId], [InvoiceNumber], [PaymentMethod], [PaymentStatus], [CreatedAt], [__v])
    VALUES 
    ('Ahmed Foysal', '01711223344', 5000.00, 'Community Classrooms & Educator Support', 0, 'TRX20260901001', 'INV-260901-1001', 'bKash', 'Completed', DATEADD(day, -5, GETUTCDATE()), 0),
    ('Dr. Rashida Begum', '01822334455', 2500.00, 'Student Learning Materials & Textbooks', 0, 'TRX20260903002', 'INV-260903-1002', 'bKash', 'Completed', DATEADD(day, -3, GETUTCDATE()), 0),
    ('Anonymous Benefactor', '01933445566', 1000.00, 'General Education & Child Support Fund', 1, 'TRX20260906003', 'INV-260906-1003', 'bKash', 'Completed', DATEADD(day, -1, GETUTCDATE()), 0);
END";
                await _context.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "EnsureDonationsTableExistsAsync encountered an error: {Message}", ex.Message);
            }
        }
    }
}
