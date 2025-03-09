using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
	public class EditProfileViewModel
	{
		[Required]
		public string? UserName { get; set; }

		[Required]
		public string? Name { get; set; }

		[Required]
		[EmailAddress]
		public string? Email { get; set; }

		[DataType(DataType.Upload)]
		public IFormFile? ImageUpload { get; set; }

		[FileExtensions(Extensions = "jpg,jpeg,png")]
		public string? ImageFileName => ImageUpload?.FileName;

		public string? Image { get; set; }
		public string? Message { get; set; }
		public string? AlertColor { get; set; }
	}
}
