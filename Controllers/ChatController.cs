using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onudhabon_ISD.Data;
using Onudhabon_ISD.Services;

namespace Onudhabon_ISD.Controllers
{
    public class ChatController : Controller
    {
        private readonly ILlmChatService _llmService;
        private readonly IPdfKnowledgeService _pdfService;
        private readonly ApplicationDbContext _context;

        public ChatController(
            ILlmChatService llmService,
            IPdfKnowledgeService pdfService,
            ApplicationDbContext context)
        {
            _llmService = llmService;
            _pdfService = pdfService;
            _context = context;
        }

        [HttpPost]
        public IActionResult UploadPdfForChat(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "Please select a valid PDF file." });
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { success = false, message = "Only PDF (.pdf) documents are supported." });
            }

            if (file.Length > 25 * 1024 * 1024)
            {
                return BadRequest(new { success = false, message = "PDF file size must be 25MB or less." });
            }

            try
            {
                using var stream = file.OpenReadStream();
                var extractedText = _pdfService.ExtractTextFromStream(stream, 40);

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Could not extract readable text from this PDF. Please ensure it is not password-protected or image-only scanned."
                    });
                }

                var words = extractedText.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                var preview = extractedText.Length > 280 ? extractedText.Substring(0, 280) + "..." : extractedText;

                return Ok(new
                {
                    success = true,
                    fileName = file.FileName,
                    text = extractedText,
                    wordCount = words,
                    preview
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error processing PDF: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest(new { error = "Message cannot be empty." });
            }

            // Retrieve recent course lectures & materials from DB to provide context to the LLM
            var recentLectures = await _context.Lectures
                .OrderByDescending(l => l.CreatedAt)
                .Take(5)
                .Select(l => $"{l.Title} (Class: {l.ClassLevel}, Subject: {l.Subject})")
                .ToListAsync();

            var recentMaterials = await _context.Materials
                .OrderByDescending(m => m.Date)
                .Take(5)
                .Select(m => $"{m.Title} (Subject: {m.Subject})")
                .ToListAsync();

            string lectureList = recentLectures.Any() ? string.Join("; ", recentLectures) : "None available yet.";
            string materialList = recentMaterials.Any() ? string.Join("; ", recentMaterials) : "None available yet.";

            string contextInfo = $@"You are 'Onudhabon AI Assistant', an intelligent educational chatbot for the Onudhabon platform.
    - Support both English and Bengali queries fluently.
    - Explain academic concepts clearly, concisely, and step-by-step.

    Available Content:
    - Recent Lectures: {lectureList}
    - Recent Materials: {materialList}

    PLATFORM NAVIGATION GUIDE:
    When the user asks where a page is or how to navigate anywhere, provide:
    1. Clear step-by-step UI directions (which navbar item or dropdown to click).
    2. A direct clickable link formatted as Markdown: [Page Name](URL).

    SITE MAP & ROUTES:
    - Home: Path '/' -> Top navigation bar > 'Home'
    - Recorded Lectures: Path '/Lecture' -> Top navigation bar > 'Recorded Lectures'
    - Study Materials: Path '/Material' -> Top navigation bar > 'Study Materials'
    - Upload Lecture: Path '/Lecture/Upload' -> Click '+ Upload' dropdown in navbar > select 'Upload Lecture Video' (Educators only)
    - Upload Material: Path '/Material/Upload' -> Click '+ Upload' dropdown in navbar > select 'Upload Study Material' (Educators only)
    - User Profile: Path '/Account/Profile' -> Click profile avatar/name at top right > select 'View Profile'
    - Login: Path '/Account/Login' -> Click 'Login' at top right
    - Register: Path '/Account/Register' -> Click 'Register' at top right
    - Admin Dashboard: Path '/Admin/Dashboard' -> Click '🛡️ Admin Dashboard' in navbar (Admins only)

    FEW-SHOT EXAMPLES:
    User: ""Where can I find lecture videos?""
    Assistant: ""You can find all lecture videos under Recorded Lectures:
    1. Look at the top navigation bar and click on **Recorded Lectures**.
    2. Or go directly by clicking here: [Open Recorded Lectures](/Lecture)""

    User: ""How can I upload study notes?""
    Assistant: ""To upload study notes (for Educators):
    1. In the top navigation bar, click the **+ Upload** button.
    2. Select **Upload Study Material** from the dropdown menu.
    3. Or navigate directly: [Upload Study Material](/Material/Upload)""

    User: ""আমি আমার প্রোফাইল কোথায় দেখতে পাব?""
    Assistant: ""আপনার প্রোফাইল দেখতে:
    1. উপরের ডানপাশে আপনার নামের ড্রপডাউনে ক্লিক করুন।
    2. **View Profile** অপশনে যান।
    3. অথবা সরাসরি যেতে ক্লিক করুন: [View Profile](/Account/Profile)""
";

            // 2. Add PDF Knowledge Context (Attached PDF and/or platform materials)
            var pdfKnowledgeContext = await _pdfService.GetGroundingContextForQueryAsync(request.Message, request.AttachedPdfText);
            if (!string.IsNullOrWhiteSpace(pdfKnowledgeContext))
            {
                contextInfo += "\n\n" + pdfKnowledgeContext + @"
    PDF DOCUMENT QUESTION-ANSWERING RULES:
    1. When answering questions regarding the attached document or platform PDF study materials, prioritize and strictly use the verified facts, definitions, formulas, and explanations extracted from the PDF above.
    2. If the user asks for a summary, provide key takeaways, headings, and core points clearly.
    3. If the user asks about a specific topic, explain it based on the document, quoting or citing page references if present.
    4. If the question cannot be answered from the document, clarify what information is available in the document and guide the student accordingly.
";
            }

            if (request.History != null && request.History.Any())
            {
                var recentHistory = request.History
                    .Where(h => !string.IsNullOrWhiteSpace(h.Text))
                    .TakeLast(6)
                    .Select(h => $"{(h.Role == "user" ? "User" : "Assistant")}: {h.Text}");

                contextInfo += "\n\nRECENT CONVERSATION HISTORY:\n" + string.Join("\n", recentHistory);
            }

            var reply = await _llmService.GetChatResponseAsync(request.Message, contextInfo);

            return Ok(new { reply });
        }

        public class ChatRequest
        {
            public string Message { get; set; } = string.Empty;
            public List<ChatMessageItem>? History { get; set; }
            public string? AttachedPdfText { get; set; }
            public string? AttachedPdfName { get; set; }
        }

        public class ChatMessageItem
        {
            public string Role { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
        }
    }
}
