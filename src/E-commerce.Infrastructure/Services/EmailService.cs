using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Common.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace E_commerce.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        try
        {
            using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception
            throw new Exception($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task SendVerificationEmailAsync(string to, string userName, string verificationToken, CancellationToken cancellationToken = default)
    {
        var subject = "Verify Your Email Address";
        var verificationLink = $"https://yourdomain.com/verify-email?token={verificationToken}";

        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .button {{ display: inline-block; padding: 12px 24px; margin: 20px 0; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; }}
                    .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Email Verification</h1>
                    </div>
                    <div class='content'>
                        <h2>Hello {userName},</h2>
                        <p>Thank you for registering with E-Commerce! Please verify your email address by clicking the button below:</p>
                        <p style='text-align: center;'>
                            <a href='{verificationLink}' class='button'>Verify Email Address</a>
                        </p>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all;'>{verificationLink}</p>
                        <p>This link will expire in 24 hours.</p>
                        <p>If you didn't create an account, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 E-Commerce. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string to, string userName, string resetToken, CancellationToken cancellationToken = default)
    {
        var subject = "Reset Your Password";
        var resetLink = $"https://yourdomain.com/reset-password?token={resetToken}";

        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #FF5722; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .button {{ display: inline-block; padding: 12px 24px; margin: 20px 0; background-color: #FF5722; color: white; text-decoration: none; border-radius: 4px; }}
                    .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 12px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Password Reset Request</h1>
                    </div>
                    <div class='content'>
                        <h2>Hello {userName},</h2>
                        <p>We received a request to reset your password. Click the button below to create a new password:</p>
                        <p style='text-align: center;'>
                            <a href='{resetLink}' class='button'>Reset Password</a>
                        </p>
                        <p>Or copy and paste this link into your browser:</p>
                        <p style='word-break: break-all;'>{resetLink}</p>
                        <div class='warning'>
                            <strong>Security Notice:</strong> This link will expire in 1 hour. If you didn't request a password reset, please ignore this email or contact support if you have concerns.
                        </div>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 E-Commerce. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to E-Commerce!";

        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .button {{ display: inline-block; padding: 12px 24px; margin: 20px 0; background-color: #2196F3; color: white; text-decoration: none; border-radius: 4px; }}
                    .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    .features {{ background-color: white; padding: 15px; margin: 20px 0; border-radius: 4px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Welcome to E-Commerce!</h1>
                    </div>
                    <div class='content'>
                        <h2>Hello {userName},</h2>
                        <p>Thank you for joining E-Commerce! We're excited to have you as part of our community.</p>
                        <div class='features'>
                            <h3>What you can do:</h3>
                            <ul>
                                <li>Browse thousands of products</li>
                                <li>Get exclusive deals and discounts</li>
                                <li>Track your orders in real-time</li>
                                <li>Save your favorite items</li>
                            </ul>
                        </div>
                        <p style='text-align: center;'>
                            <a href='https://yourdomain.com/products' class='button'>Start Shopping</a>
                        </p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 E-Commerce. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }

    public async Task SendOrderConfirmationEmailAsync(string to, string userName, string orderId, decimal totalAmount, CancellationToken cancellationToken = default)
    {
        var subject = $"Order Confirmation - #{orderId}";

        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f9f9f9; }}
                    .order-info {{ background-color: white; padding: 15px; margin: 20px 0; border-radius: 4px; }}
                    .button {{ display: inline-block; padding: 12px 24px; margin: 20px 0; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; }}
                    .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    .total {{ font-size: 24px; font-weight: bold; color: #4CAF50; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Order Confirmed!</h1>
                    </div>
                    <div class='content'>
                        <h2>Hello {userName},</h2>
                        <p>Thank you for your order! We've received your order and will begin processing it shortly.</p>
                        <div class='order-info'>
                            <h3>Order Details</h3>
                            <p><strong>Order Number:</strong> {orderId}</p>
                            <p><strong>Total Amount:</strong> <span class='total'>${totalAmount:F2}</span></p>
                        </div>
                        <p>We'll send you another email when your order ships.</p>
                        <p style='text-align: center;'>
                            <a href='https://yourdomain.com/orders/{orderId}' class='button'>Track Order</a>
                        </p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2025 E-Commerce. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(to, subject, body, true, cancellationToken);
    }
}
