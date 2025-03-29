using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models.ViewModels
{
    public class ShowtimeViewModel
    {
        public int ShowtimeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phim")]
        [Display(Name = "Phim")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        [Display(Name = "Phòng chiếu")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian bắt đầu")]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thời gian kết thúc")]
        [Display(Name = "Thời gian kết thúc")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá vé")]
        [Range(0, 1000000, ErrorMessage = "Giá vé phải lớn hơn 0")]
        [Display(Name = "Giá vé cơ bản")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Scheduled";
    }
}