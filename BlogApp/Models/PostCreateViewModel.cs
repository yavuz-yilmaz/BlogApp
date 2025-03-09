using BlogApp.Entity;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class PostCreateViewModel
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Açıklama alanı zorunludur")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "URL alanı zorunludur")]
        [Display(Name = "URL")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "URL sadece küçük harf, rakam ve tire içerebilir")]
        public string Url { get; set; }
        
        [DataType(DataType.Upload)]
        [Display(Name = "Resim")]
        public IFormFile? ImageUpload { get; set; }

        [FileExtensions(Extensions = "jpg,jpeg,png", ErrorMessage = "Lütfen sadece jpg, jpeg veya png dosyası yükleyin")]
        public string? ImageFileName => ImageUpload?.FileName;
        
        public string? Image { get; set; }
        
        [Required(ErrorMessage = "İçerik alanı zorunludur")]
        [Display(Name = "İçerik")]
        public string Content { get; set; }

        public bool IsActive { get; set; }

        public List<Tag> Tags { get; set; } = new();
    }
}
