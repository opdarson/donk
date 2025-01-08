
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace donk.Models;

[ModelMetadataType(typeof(Usersmetadata))]
public partial class Users
{
}

public partial class Usersmetadata


{

    public int Id { get; set; }

    [DisplayName("帳號")]
    [RegularExpression(@"^\S+$", ErrorMessage = "輸入不能包含空格")]
    [Required(ErrorMessage = "請輸入帳號")]
    [BindRequired]
    public string Username { get; set; }
    [BindRequired]
    [Required(ErrorMessage = "請輸入密碼")]
    [DisplayName("密碼")]
    public string Password { get; set; }






}