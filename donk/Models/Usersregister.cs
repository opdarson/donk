
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace donk.Models;

public partial class Usersregister
{
    [RegularExpression(@"^\S+$", ErrorMessage = "輸入不能包含空格")]
    [DisplayName("帳號")]
    [BindRequired]
    [Required(ErrorMessage = "請輸入帳號")]
    public string Username { get; set; }

    [DisplayName("密碼")]
    [BindRequired]
    [Required(ErrorMessage = "請輸入密碼")]
    public string Password { get; set; }





    [Required(ErrorMessage = "確認密碼為必填項。")]
    [Display(Name = "確認密碼")]
    [Compare("Password", ErrorMessage = "密碼與確認密碼不一致。")]
    public string ConfirmPassword { get; set; }


    [DisplayName("電子信箱")]
    [BindRequired]
    [Required(ErrorMessage = "請輸入郵箱帳號")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    public string Email { get; set; }

    [DisplayName("電話號碼")]
    [Phone]
    [Required(ErrorMessage = "請輸入電話號碼")]
    public string Phone { get; set; }
    [DisplayName("地址")]
    [Required(ErrorMessage = "請輸入地址")]
    public string Address { get; set; }
}