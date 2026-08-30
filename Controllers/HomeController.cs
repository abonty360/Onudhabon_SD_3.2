using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Models;

namespace Onudhabon_ISD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
{
    var userCount = await _context.Users.CountAsync();

    var materialCount = await _context.Materials.CountAsync();

    var volunteerCount = await _context.Users
        .CountAsync(u => u.Role == "Volunteer");

    var latestPosts = await _context.ForumPosts
        .OrderByDescending(p => p.CreatedAt)
        .Take(3)
        .ToListAsync();

    ViewBag.UserCount = userCount;
    ViewBag.MaterialCount = materialCount;
    ViewBag.VolunteerCount = volunteerCount;

    return View(latestPosts);
}

        [Authorize(Roles = "Admin")]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Users()
        {
            return RedirectToAction("Dashboard", "Admin", new { tab = "volunteers" });
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}