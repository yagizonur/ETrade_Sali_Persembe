﻿using ETICARET.Business.Abstract;
using ETICARET.WebUI.EmailService;
using ETICARET.WebUI.Extensions;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ETICARET.WebUI.Controllers
{
    [AutoValidateAntiforgeryToken] 
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private ICartService _cartService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
            };

            var result = await _userManager.CreateAsync(user,model.Password);

            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Action("ConfirmEmail", "Account", new
                {
                    userId = user.Id,
                    token = code
                });

                string siteUrl = "http://localhost:5164";
                string activeUrl = $"{siteUrl} {callbackUrl}";

                string body = $"Hesabınızı onaylayınız. <br> <br> Lütfen email hesabınızı onaylamak için linke <a href ='{activeUrl}' target='_blank'> tıklayınız.</a>";

                MailHelper.SendEmail(body, model.Email, "ETicaret Hesap Aktifleştirme Onayı");

                return RedirectToAction("Login", "Account");
            }

            ModelState.AddModelError("", "Bilinmeyen bir hata oluştu.");

            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if(userId == null || token == null)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Geçersiz Token",
                    Message = "Hesap onay bilgileri yanlış",
                    Css = "danger"
                });

                return Redirect("~/");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    _cartService.InitialCart(userId);

                    TempData.Put("message", new ResultModel()
                    {
                        Title = "Hesap onayı",
                        Message = "Hesabınız onaylanmıştır",
                        Css = "success"
                    });

                    return RedirectToAction("Login", "Account");
                }
            }

            TempData.Put("message", new ResultModel()
            {
                Title = "Hesap onayı",
                Message = "Hesabınız onaylanmıştır.",
                Css = "danger"

            });

            return Redirect("~/");
        }

        public IActionResult Login(string returnUrl = null)
        {
            return View(
                new LoginModel()
                {
                    ReturnUrl = returnUrl,
                });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            ModelState.Remove("ReturnUrl");

            if (!ModelState.IsValid)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Giriş Bilgileri",
                    Message = "Bilgileriniz hatalıdır",
                    Css = "danger"
                });

                return View(model);

            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
            {
                ModelState.AddModelError("", "Bu email ile daha önce hesap oluşturulmamıştır.");
                return View(model);
            }

            if (user.LockoutEnd != null)
            {
                ModelState.AddModelError("", "5 kere yanlış girildiği için hesabınız kilitlenmiştir.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, true);

            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? "~/");
            }

            ModelState.AddModelError("", "Email veya şifre yanlış");
            return View(model);

        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            TempData.Put("message", new ResultModel()
            {
                Title = "Oturum kapatıldı",
                Message = "Hesabınız güvenli bir şekilde sonlandırıldı.",
                Css = "success"
            });

            return Redirect("~/");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Forgot Password",
                    Message = "Email boş geçilemez.",
                    Css = "danger"
                });

                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if(user is null)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Forgot Password",
                    Message = "Bu email adresi ile bir kullanıcı bulunamadı!",
                    Css = "danger"
                });
                return View();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action("ResetPassword", "Account", new
            {
                token = code
            });

            string siteUrl = "http://localhost:5164";
            string activeUrl = $"{siteUrl} {callbackUrl}";

            string body = $"Parolanızı yenilemek için <a href='{activeUrl}' target='_blank' tıklayınız...</a>";

            MailHelper.SendEmail(body, email, "ETicaret Şifre Yenileme");

            TempData.Put("message", new ResultModel()
            {
                Title = "Forgot Password",
                Message = "Email adresinize şifre yenileme maili gönderilmiştir.",
                Css = "success"

            });

            return RedirectToAction("Login");
        }

        public IActionResult ResetPassword(string token)
        {
            if(token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new ResetPasswordModel() { Token = token };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user is null)
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Reset Password",
                    Message = "Bu email adresi ile bir kullanıcı bulunamadı!",
                    Css = "danger"
                });

                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ResetPasswordAsync(user,model.Token,model.Password);

            if (result.Succeeded) 
            {
                return RedirectToAction("Login", "Account");
            
            }

            else
            {
                TempData.Put("message", new ResultModel()
                {
                    Title = "Reset Password",
                    Message = "Şifreniz uygun değildir",
                    Css = "danger"
                });

                return View(model);
            }
        }
        }
    }
