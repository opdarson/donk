
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace donk.Models;

public class UsersEditProfile
{
    public string Username { get; set; } // 唯讀
    public string Email { get; set; } // 唯讀
    [Required]
    [Phone]
    public string Phone { get; set; }
    [Required]
    public string Address { get; set; }
}