using AutoMapper;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using FinDepen_Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtService jwtService, IMapper mapper, IEmailService emailService, IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _mapper = mapper;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var user = _mapper.Map<ApplicationUser>(model);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Registration failed", errors });
            }

            return Ok(new { message = "User registered successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) 
                return Unauthorized(new { message = "Invalid email or password" });

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded) 
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _jwtService.GenerateToken(user);
            return Ok(new { 
                Token = token,
                User = new { 
                    Id = user.Id, 
                    Name = user.Name, 
                    Email = user.Email 
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred during login", error = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) 
                return BadRequest(new { message = "If a user with this email exists, a password reset OTP has been sent" });

            var token = GenerateOtpToken();

            user.PasswordResetOtp = token;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(10); // Extended to 10 minutes
            await _userManager.UpdateAsync(user);

            // Send OTP to user's email
            await _emailService.SendPasswordResetOtpAsync(model.Email, token);

            return Ok(new { message = "If a user with this email exists, a password reset OTP has been sent" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while processing your request", error = ex.Message });
        }
    }

    private string GenerateOtpToken()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel model)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) 
                return BadRequest(new { message = "Invalid email or OTP" });

            if (user.PasswordResetOtp != model.Otp) 
                return BadRequest(new { message = "Invalid OTP" });
            
            if (user.PasswordResetOtpExpiry < DateTime.UtcNow) 
                return BadRequest(new { message = "OTP has expired. Please request a new one" });

            return Ok(new { message = "OTP verified successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while verifying OTP", error = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null) 
                return BadRequest(new { message = "Invalid email or OTP" });

            if (user.PasswordResetOtp != resetPasswordModel.Otp) 
                return BadRequest(new { message = "Invalid OTP" });
            
            if (user.PasswordResetOtpExpiry < DateTime.UtcNow) 
                return BadRequest(new { message = "OTP has expired. Please request a new one" });

            user.PasswordHash = _passwordHasher.HashPassword(user, resetPasswordModel.NewPassword);
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;
            
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { message = "Password reset failed", errors });
            }

            return Ok(new { message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while resetting password", error = ex.Message });
        }
    }
}

