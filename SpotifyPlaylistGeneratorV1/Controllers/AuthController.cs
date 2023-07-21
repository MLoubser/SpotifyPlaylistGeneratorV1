using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SpotifyPlaylistGeneratorV1.Authentication;
using Microsoft.AspNetCore.Authorization;
using SpotifyPlaylistGeneratorV1.Models.Views;

namespace SpotifyPlaylistGeneratorV1.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        #region Login Section
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginData, string? returnUrl)
        {
            if(ModelState.IsValid)
            {
                var signIn = await _signInManager.PasswordSignInAsync(loginData.Email,loginData.Password,true,false);

                if(signIn.Succeeded)
                {
                    return (string.IsNullOrEmpty(returnUrl)) ? RedirectToAction("Index", "Home") : LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(nameof(LoginViewModel.Email), "Invalid Credentials");           
                }
            }
            return View(loginData);
        }
        #endregion

        #region Register Section
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerData)
        {
            if(ModelState.IsValid)
            {
                var emailExists = await _userManager.FindByEmailAsync(registerData.Email);

                if (emailExists == null)
                {
                    var result = await _userManager.CreateAsync(new ApplicationUser {
                        UserName = registerData.Email,
                        Email = registerData.Email,
                        FullNameX = registerData.FirstName,
                        LastNameX = registerData.LastName
                    }, registerData.Password);

                    if(result.Succeeded)
                    {
                        return RedirectToAction("Login", "Auth");
                    }
                    else
                    {
                        //ModelState.AddModelError(nameof(registerData.Email), "Failed to create account");
                        foreach(var err in result.Errors)
                        {
                            ModelState.AddModelError(String.Empty, err.Description);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(registerData.Email), "Email already in use");
                }
            }

            return View(registerData);
        }
        #endregion

        #region Logout Section
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        #endregion
    }
}

