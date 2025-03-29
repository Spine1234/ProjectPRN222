using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models.ViewModels
{
    public class RoomViewModel
    {
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phòng")]
        [Display(Name = "Tên phòng")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập sức chứa")]
        [Range(1, 1000, ErrorMessage = "Sức chứa phải từ 1 đến 1000")]
        [Display(Name = "Sức chứa")]
        public int Capacity { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Available";
    }
}