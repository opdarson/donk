global using System;                        // 提供基礎類別庫的功能，例如 DateTime、Exception、Math 等。
global using System.Collections.Generic;    // 用於集合相關的類別，例如 List<T>, Dictionary<TKey, TValue>。
global using System.Linq;                   // 用於 LINQ 查詢操作，例如 Where、Select、OrderBy 等。
global using System.Threading.Tasks;        // 用於異步操作 (async/await)，例如 Task 類別。
global using Microsoft.AspNetCore.Mvc;      // ASP.NET Core MVC 的基礎功能，包含 Controller、Action 等類別。
global using Microsoft.EntityFrameworkCore; // 用於 Entity Framework Core，支援資料庫存取和操作。

// 如果需要加入自定義的命名空間
global using donk.Models;                   // 專案中自定義的模型命名空間，例如 Users、Products 等資料模型。
global using Microsoft.AspNetCore.Authorization; // 用於授權驗證，例如 [Authorize] 屬性。
global using System.Security.Claims;        // 用於管理使用者的 Claims，例如身份驗證和授權相關功能。
global using X.PagedList;                   // 用於分頁功能，例如 ToPagedList() 方法。
global using System.Diagnostics;            // 提供診斷工具類別，例如 Debug 和 Activity。
global using Microsoft.AspNetCore.Http;     // 提供對 HTTP 內容（如 Session、Cookie）的操作。
global using Microsoft.AspNetCore.Identity; // ASP.NET Core 身份管理功能，例如 PasswordHasher、IdentityUser。
global using Microsoft.EntityFrameworkCore.Query; // 用於處理查詢的進階功能，例如查詢相關的 LINQ 擴展。
