using System.ComponentModel.DataAnnotations;
using TokoBuku.Application.Common.Models;
using TokoBuku.Application.Features.Books.DTOs;
using TokoBuku.Web.App.Models;

namespace TokoBuku.Web.App.ViewModels;

public class HomeViewModel
{
    public List<BookDto> BestSellers { get; set; } = new();
    public List<BookDto> DiscountedBooks { get; set; } = new();
    public List<BookDto> LatestBooks { get; set; } = new();
    public List<string> Categories { get; set; } = new();
}

public class CatalogViewModel
{
    public PaginatedList<BookDto> Books { get; set; } = null!;
    public List<string> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }
}

public class BookDetailViewModel
{
    public BookDto Book { get; set; } = null!;
    public List<BookDto> RelatedBooks { get; set; } = new();
}

public class CheckoutViewModel : IValidatableObject
{
    public Cart Cart { get; set; } = new();

    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(100, ErrorMessage = "Nama tidak boleh melebihi 100 karakter.")]
    [Display(Name = "Nama Lengkap")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alamat email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    [Display(Name = "Alamat Email")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Format nomor telepon tidak valid.")]
    [Display(Name = "No. Telepon / WhatsApp")]
    public string? CustomerPhone { get; set; }

    [Required]
    [Display(Name = "Metode Pemenuhan Pesanan")]
    public string FulfillmentMethod { get; set; } = "StorePickUp"; // "StorePickUp" or "HomeDelivery"

    [Display(Name = "Alamat Pengiriman")]
    public string? ShippingAddress { get; set; }

    [Display(Name = "Catatan Pesanan")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Metode pembayaran wajib dipilih.")]
    [Display(Name = "Metode Pembayaran")]
    public string PaymentMethod { get; set; } = "QRIS"; // "QRIS", "BankBCA", "BankMandiri"

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FulfillmentMethod == "HomeDelivery" && string.IsNullOrWhiteSpace(ShippingAddress))
        {
            yield return new ValidationResult(
                "Alamat pengiriman wajib diisi jika memilih opsi Kirim ke Alamat.",
                new[] { nameof(ShippingAddress) }
            );
        }
    }
}

public class OrderSuccessViewModel
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string FulfillmentMethod { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount { get; set; }
    public List<CartItem> Items { get; set; } = new();
}

public class OrderTrackingViewModel
{
    public string? SearchInvoice { get; set; }
    public bool HasSearched { get; set; }
    public bool IsFound { get; set; }
    public string? ErrorMessage { get; set; }

    public string? InvoiceNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? CustomerName { get; set; }
    public string? Status { get; set; }
    public string? FulfillmentMethod { get; set; }
    public string? ShippingAddress { get; set; }
    public decimal NetAmount { get; set; }
    public List<CartItem> Items { get; set; } = new();
}
