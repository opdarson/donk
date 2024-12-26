
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

    [DisplayName("電子信箱")]
    [BindRequired]
    [Required(ErrorMessage = "請輸入郵箱帳號")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    public string Email { get; set; }
}