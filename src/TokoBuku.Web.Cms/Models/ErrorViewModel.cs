namespace TokoBuku.Web.Models;

public class ErrorViewModel
{
    public string RequestId { get; set; } = string.Empty;
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("dd MMMM yyyy, HH:mm:ss 'UTC'");
    public string? Path { get; set; }
    public string? HttpMethod { get; set; }

    // User-facing friendly message (Maintenance / Incident)
    public string Title { get; set; } = "Sistem Sedang Mengalami Kendala / Pemeliharaan";
    public string UserMessage { get; set; } = "Mohon maaf atas ketidaknyamanan ini. Terjadi kendala saat memproses permintaan Anda. Sistem kami telah mencatat insiden ini ke dalam audit log secara otomatis.";

    // Developer / Administrator Debugging context
    public bool IsDeveloperOrAdmin { get; set; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerExceptionMessage { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
