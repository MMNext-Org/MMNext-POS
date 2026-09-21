using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MMNextPOS.Application.Services;

namespace MMNextPOS.Application.Services
{
    /// <summary>
    /// Cloudflare Turnstile verification service.
    /// Verifies Turnstile tokens server-side against the Cloudflare API.
    /// </summary>
    public class CloudflareTurnstileVerificationService : ITurnstileVerificationService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;

        public CloudflareTurnstileVerificationService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration
                .GetSection("Cloudflare:Turnstile:SecretKey")
                .Value ?? throw new InvalidOperationException("Turnstile SecretKey not configured");
            _httpClient = new HttpClient { BaseAddress = new Uri("https://api.cloudflare.com") };
        }

        public async Task<bool> VerifyAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var content = new StringContent(
                $"{{\"secret\": \"{_secretKey}\", \"response\": \"{token}\"}}",
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/turnstile/verify", content);

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TurnstileVerificationResult>(json);

            return result?.Success ?? false;
        }

        private class TurnstileVerificationResult
        {
            [JsonPropertyName("success")] public bool Success { get; set; }
            [JsonPropertyName("error-codes")] public string[]? ErrorCodes { get; set; }
        }
    }
}
