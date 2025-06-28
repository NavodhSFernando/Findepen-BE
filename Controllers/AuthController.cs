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
        var user = _mapper.Map<ApplicationUser>(model);

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized("Invalid email or password");

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
        if (!result.Succeeded) return Unauthorized("Invalid email or password");

        var token = _jwtService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return BadRequest("Email cannot be empty.");
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest("User not found");

        var token = GenerateOtpToken();

        user.PasswordResetOtp = token;
        user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(1);
        await _userManager.UpdateAsync(user);

        // Send OTP to user's email
        await _emailService.SendPasswordResetOtpAsync(model.Email, token);

        return Ok(new { message = "Password reset link sent to your email" });
    }

    private string GenerateOtpToken()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest("User not found");

        if (user.PasswordResetOtp != model.Otp) return BadRequest("Invalid OTP");
        if (user.PasswordResetOtpExpiry < DateTime.UtcNow) return BadRequest("OTP expired");

        return Ok(new { message = "Otp verified succesfully" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
        if (user == null) return BadRequest("User not found");

        user.PasswordHash = _passwordHasher.HashPassword(user, resetPasswordModel.NewPassword);

        user.PasswordResetOtp = null;
        user.PasswordResetOtpExpiry = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Password reset successfully" });
    }
}

