using System.ComponentModel.DataAnnotations;
#nullable disable


namespace donk.Models
{
    public class ChangePasswordViewModel
    {

        [Required(ErrorMessage = "舊密碼為必填項。")]
        [DataType(DataType.Password)]
        [Display(Name = "舊密碼")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "新密碼為必填項。")]
        [DataType(DataType.Password)]
        [Display(Name = "新密碼")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "確認新密碼為必填項。")]
        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼與確認新密碼不一致。")]
        public string ConfirmPassword { get; set; }
    }
}
