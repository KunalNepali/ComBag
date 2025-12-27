using System.ComponentModel.DataAnnotations;

namespace ComBag.Models
{
    public class BlogComment
    {
        public int Id { get; set; }

        [Required]
        public int BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        [Display(Name = "Your Name")]
        public string AuthorName { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string AuthorEmail { get; set; }

        [Required(ErrorMessage = "Comment is required")]
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Comment")]
        public string Content { get; set; }

        public DateTime CommentDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Approved")]
        public bool IsApproved { get; set; } = true;

        // For reply functionality
        public int? ParentCommentId { get; set; }
        public BlogComment ParentComment { get; set; }
        public List<BlogComment> Replies { get; set; }
    }
}