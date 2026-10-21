using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UserRole.ViewModel
{
    public class CreatePostViewModel
    {
        [Display(Name = "Caption")]
        public string? Caption { get; set; }

        [Required(ErrorMessage = "Please select a file.")]
        public IFormFile? MediaFile { get; set; }
    }
}