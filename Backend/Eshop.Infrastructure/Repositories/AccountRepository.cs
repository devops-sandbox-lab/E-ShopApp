using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eshop.Application.Configurations;
using Eshop.Application.DTOs;
using Eshop.Application.Interfaces.Repository;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;
using Eshop.Core.Enums;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Eshop.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IUserOTPService _userOTPService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<GoogleAuthConfig> googleAuthConfig;
        private readonly IOptions<JWT> jWT;

        public AccountRepository(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
             IConfiguration configuration,
             IUserOTPService userOTPService,
             IHttpContextAccessor httpContextAccessor,
             IOptions<GoogleAuthConfig> googleAuthConfig,
             IOptions<JWT> JWT
    )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            this._userOTPService = userOTPService;
            this._httpContextAccessor = httpContextAccessor;
            this.googleAuthConfig = googleAuthConfig;
            jWT = JWT;
        }


        public async Task<AuthResponseDTO> RegisterUserAsync(ApplicationUser user, string password, string role)
        {
            if (await _userManager.FindByEmailAsync(user.Email) is not null)
            {
                return new AuthResponseDTO() { Message = "Email is already registered" };
            }


            user.UserName = GenerateUsernameFromEmail(user.Email);
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);

                await _userOTPService.SaveAndSendOTPAsync(user.Email, user.FirstName, user.LastName);
                return new AuthResponseDTO()
                {
                    Message = "Registration successful",
                    IsEmailConfirmed = user.EmailConfirmed,
                    Name = user.FirstName + " " + user.LastName,
                    Email = user.Email,
                    Succeeded = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(await CreateJwtToken(user)),
                    Role = role
                };
            }
            else
            {
                return new AuthResponseDTO()
                {
                    Message = string.Join(", ", result.Errors.Select(error => error.Description)),
                    IsEmailConfirmed = false,
                    Errors = result.Errors.Select(error => error.Description).ToList()

                };
            }
        }

        public async Task<AuthResponseDTO> CustomerRegisterAsync(Customer customer, CustomerRegisterDTO registerDto)
        {
            return await RegisterUserAsync(customer, registerDto.Password, UserRole.Customer.ToString());
        }
        public async Task<AuthResponseDTO> SellerRegisterAsync(Seller seller, SellerRegisterDTO registerDto)
        {
            return await RegisterUserAsync(seller, registerDto.Password, UserRole.Seller.ToString());
        }
        public async Task<AuthResponseDTO> ConfirmEmailAsync(string email, string otp)
        {
            var isValid = await _userOTPService.VerifyOTPAsync(email, otp);
            if (!isValid)
            {
                return new AuthResponseDTO { Message = "Invalid or expired OTP. Please request a new OTP." };
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthResponseDTO { Message = "User not found." };
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            var checkUserType = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return new AuthResponseDTO
            {
                Message = "Email confirmed successfully.",
                IsEmailConfirmed = true,
                Succeeded = true,
                Name = user.FirstName + " " + user.LastName,
                Email = user.Email,
                Token = checkUserType != "Owner" ? new JwtSecurityTokenHandler().WriteToken(await CreateJwtToken(user)) : null,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
            };
        }

        public async Task<bool> SendNewOTPAsync(string email)
        {
            var currentUser = await _userManager.FindByEmailAsync(email);

            if (currentUser == null)
            {
                return false;
            }

            var checkUserType = (await _userManager.GetRolesAsync(currentUser)).FirstOrDefault();
            ApplicationUser user;

            if (checkUserType == "Seller")
            {
                user = await _context.Sellers.FirstOrDefaultAsync(u => u.Id == currentUser.Id);
            }
            else
            {
                user = await _context.Customers.FirstOrDefaultAsync(u => u.Id == currentUser.Id);
            }

            if (user == null)
            {
                return false;
            }

            await _userOTPService.SendNewOTPAsync(email, user.FirstName, user.LastName);

            return true;
        }
        public async Task<AuthResponseDTO> Login(LoginUserDTO loginUser)
        {
            var user = await _userManager.FindByEmailAsync(loginUser.Email);

            if (user != null)
            {
                bool found = await _userManager.CheckPasswordAsync(user, loginUser.Password);
                if (found)
                {
                    var checkUserType = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                    Seller seller = null;

                    if (checkUserType == "Seller")
                    {
                        seller = await _context.Sellers.FirstOrDefaultAsync(u => u.Id == user.Id);
                    }
                    var Token = await CreateJwtToken(user);
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(Token);

                    SetCookie("Token", tokenString);
                    SetCookie("Role", checkUserType);
                    SetCookie("Username", user.FirstName + " " + user.LastName);

                    if (!user.EmailConfirmed)
                    {
                        return new AuthResponseDTO()
                        {
                            Message = "Email not confirmed",
                            IsEmailConfirmed = false,
                            ExpireTime = Token.ValidTo,
                            Token = tokenString,
                            Succeeded = true,
                            Role = checkUserType,
                            Name = user.FirstName + " " + user.LastName,
                            Email = user.Email,
                            AccountStatus = seller?.AccountStatus.ToString(),
                            IsBlocked = user.IsBlocked,
                        };
                    }
                    return new AuthResponseDTO()
                    {
                        Message = "Login successful",
                        IsEmailConfirmed = true,
                        Succeeded = true,
                        Name = user.FirstName + " " + user.LastName,
                        Email = user.Email,
                        ExpireTime = Token.ValidTo,
                        Token = tokenString,
                        Role = checkUserType,
                        AccountStatus = seller?.AccountStatus.ToString(),
                        IsBlocked = user.IsBlocked,
                    };
                }
                else
                {
                    return new AuthResponseDTO()
                    {
                        Message = "Login failed: Incorrect email or password",
                        IsEmailConfirmed = user.EmailConfirmed,
                        Succeeded = false
                    };
                }
            }

            return new AuthResponseDTO()
            {
                Message = "Login failed: User not found",
                IsEmailConfirmed = false,
                Succeeded = false
            };
        }


        public async Task<IdentityResult> ChangePasswordAsync(string userEmail, ChangePasswordDTO changePasswordModel)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            if (user.SecurityQuestion != changePasswordModel.SecurityQuestionAnswer)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Incorrect security question answer" });
            }

            var passwordVerificationResult = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, changePasswordModel.OldPassword);
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Old password is incorrect" });
            }

            if (changePasswordModel.OldPassword == changePasswordModel.NewPassword)
            {
                return IdentityResult.Failed(new IdentityError { Description = "New password cannot be the same as the old password" });
            }

            return await _userManager.ChangePasswordAsync(user, changePasswordModel.OldPassword, changePasswordModel.NewPassword);
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userEmail, string token, string newPassword)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = System.Text.Encoding.ASCII.GetBytes(_configuration["JWT:key"]);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWT:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Invalid token: email not found" });
                }

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found" });
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

                return result;
            }
            catch (SecurityTokenException)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid token" });
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "An error occurred while resetting the password" });
            }

        }



        public async Task<bool> ForgetPassword(string Email)
        {
            var User = await _userManager.FindByEmailAsync(Email);
            if (User is null)
            {
                return false;
            }
            var TokenGenerated = await CreateJwtToken(User);
            var Token = new JwtSecurityTokenHandler().WriteToken(TokenGenerated);

            var result = _userOTPService.SendForgetPasswordLinkAsync(Email, Token, User.FirstName, User.LastName);

            if (result is null)
            {
                return false;
            }
            return true;


        }
        public async Task<AuthResponseDTO> GoogleSignIn(string model)
        {
            Payload payload = new();

            try
            {
                payload = await ValidateAsync(model, new ValidationSettings
                {
                    Audience = new[] { googleAuthConfig.Value.ClientId }
                });

            }
            catch (Exception ex)
            {
                return new AuthResponseDTO
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }

            if (payload == null)
            {
                return new AuthResponseDTO
                {
                    Succeeded = false,
                    Message = "Google login failed"
                };
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                Customer customer = new Customer
                {
                    FirstName = payload.Name,
                    LastName = payload.FamilyName ?? " ",
                    Email = payload.Email,
                    ProfileImage = payload.Picture,
                    EmailConfirmed = true,
                    SecurityQuestion = "answer",
                    UserName = GenerateUsernameFromEmail(payload.Email),
                };

                try
                {
                    var result = await _userManager.CreateAsync(customer);
                    if (!result.Succeeded)
                    {
                        return new AuthResponseDTO
                        {
                            Succeeded = false,
                            Message = "Failed to create user"
                        };
                    }

                    await _context.SaveChangesAsync();

                    string token = new JwtSecurityTokenHandler().WriteToken(await CreateJwtToken(customer));

                    return new AuthResponseDTO
                    {
                        Succeeded = true,
                        Email = payload.Email,
                        Role = "Customer",
                        Message = "Logged in successfully via Google",
                        Token = token,
                    };
                }
                catch
                {
                    return new AuthResponseDTO
                    {
                        Succeeded = false,
                        Message = "An error occurred while creating the user"
                    };
                }
            }
            return new AuthResponseDTO
            {
                Succeeded = false,
                Message = "User already exists"
            };
        }

        //helper functions 
        private string GenerateUsernameFromEmail(string email)
        {
            return email.ToLower();
        }
        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("uid", user.Id),
                    new Claim("signature", "Tendez-Website"),
                    new Claim("FirstName", user.FirstName),
                }
                .Union(userClaims)
                .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var durationDaysString = _configuration["JWT:DurationDays"];

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: signingCredentials
            );


            return jwtSecurityToken;
        }

        private void SetCookie(string key, string value)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            };


            _httpContextAccessor.HttpContext.Response.Cookies.Append(key, value, cookieOptions);
        }

        public async Task<Seller> GetSellerByIdAsync(string id)
        {
            try
            {
                return await _context.Sellers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> UpdateSellerAsync(Seller seller)
        {
            _context.Entry(seller).State = EntityState.Modified;
            try
            {
                var existingSeller = await _context.Sellers.FindAsync(seller.Id);
                if (existingSeller == null)
                {
                    return false;
                }

                _context.Entry(existingSeller).CurrentValues.SetValues(seller);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task save()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<Customer> GetCustomerByIdAsync(string id)
        {
            try
            {
                return await _context.Customers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> UpdateCustomerAsync(Customer customer)
        {
            _context.Entry(customer).State = EntityState.Modified;
            try
            {
                var existingCustomer = await _context.Customers.FindAsync(customer.Id);
                if (existingCustomer == null)
                {
                    return "faild";
                }

                _context.Entry(existingCustomer).CurrentValues.SetValues(customer);
                await _context.SaveChangesAsync();
                var Token = new JwtSecurityTokenHandler().WriteToken(await CreateJwtToken(existingCustomer));
                return Token.ToString();

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
