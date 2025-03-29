using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models.ViewModels
{
    public class MovieViewModel
    {
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phim")]
        [Display(Name = "Tên phim")]
        public string Title { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Đạo diễn")]
        public int DirectorId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời lượng phim")]
        [Range(1, 500, ErrorMessage = "Thời lượng phải từ 1 đến 500 phút")]
        [Display(Name = "Thời lượng (phút)")]
        public int Duration { get; set; }

        [Display(Name = "Ngày khởi chiếu")]
        public DateOnly? ReleaseDate { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateOnly? EndDate { get; set; }

        [Display(Name = "URL Poster")]
        public string PosterUrl { get; set; }

        [Display(Name = "URL Trailer")]
        public string TrailerUrl { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Coming Soon";

        [Display(Name = "Đánh giá")]
        [Range(0, 10, ErrorMessage = "Đánh giá phải từ 0 đến 10")]
        public decimal? Rating { get; set; }
    }
}