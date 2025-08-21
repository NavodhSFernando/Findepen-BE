using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Services;
using FinDepen_Backend.Repositories;

namespace FinDepen_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReceiptController : ControllerBase
    {
        private readonly ILogger<ReceiptController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IReceiptProcessingService _receiptProcessingService;

        public ReceiptController(
            ILogger<ReceiptController> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IReceiptProcessingService receiptProcessingService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _receiptProcessingService = receiptProcessingService;
        }

        [HttpPost("process")]
        public async Task<ActionResult<ReceiptProcessingResponse>> ProcessReceipt([FromBody] ProcessReceiptRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            try
            {
                // Validate user authentication
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized receipt processing attempt");
                    return Unauthorized(new ReceiptProcessingResponse
                    {
                        Success = false,
                        Error = "User not authenticated"
                    });
                }

                // Validate request
                if (string.IsNullOrEmpty(request.FileBase64))
                {
                    _logger.LogWarning("Receipt processing attempted with empty image for user: {UserId}", userId);
                    return BadRequest(new ReceiptProcessingResponse
                    {
                        Success = false,
                        Error = "Receipt image is required"
                    });
                }

                // Validate base64 format
                if (!IsValidBase64(request.FileBase64))
                {
                    _logger.LogWarning("Invalid base64 format provided for user: {UserId}", userId);
                    return BadRequest(new ReceiptProcessingResponse
                    {
                        Success = false,
                        Error = "Invalid image format"
                    });
                }

                _logger.LogInformation("Processing receipt for user: {UserId} ({Email})", userId, userEmail);

                // Call the AWS API through the backend
                var awsApiUrl = "https://z9yrdck3l2.execute-api.us-east-1.amazonaws.com/process-receipt";
                
                using var httpClient = _httpClientFactory.CreateClient();
                
                // Set timeout for the request
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var awsRequest = new
                {
                    file_base64 = request.FileBase64
                };

                var jsonContent = JsonSerializer.Serialize(awsRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to AWS API for user: {UserId}", userId);

                var response = await httpClient.PostAsync(awsApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AWS API request failed with status: {StatusCode}, Error: {Error} for user: {UserId}", 
                        response.StatusCode, errorContent, userId);
                    
                    return StatusCode((int)response.StatusCode, new ReceiptProcessingResponse
                    {
                        Success = false,
                        Error = $"Failed to process receipt: {response.StatusCode}",
                        Message = errorContent
                    });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var awsResponse = JsonSerializer.Deserialize<AwsReceiptResponse>(responseContent);
                    
                    if (awsResponse == null)
                    {
                        _logger.LogError("Failed to deserialize AWS response for user: {UserId}", userId);
                        return StatusCode(500, new ReceiptProcessingResponse
                        {
                            Success = false,
                            Error = "Invalid response from receipt processing service"
                        });
                    }

                    // Use the service to process the receipt data
                    var receiptResponse = _receiptProcessingService.ProcessReceiptData(awsResponse);

                    if (receiptResponse.Success)
                    {
                        _logger.LogInformation("Receipt processed successfully for user: {UserId}. Merchant: {Merchant}, Amount: {Amount}", 
                            userId, receiptResponse.Data?.Merchant, receiptResponse.Data?.Total ?? receiptResponse.Data?.Amount);
                    }
                    else
                    {
                        _logger.LogWarning("Receipt processing failed for user: {UserId}. Error: {Error}", 
                            userId, receiptResponse.Error);
                    }

                    return Ok(receiptResponse);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse AWS response for user: {UserId}. Response: {Response}", 
                        userId, responseContent);
                    
                    return StatusCode(500, new ReceiptProcessingResponse
                    {
                        Success = false,
                        Error = "Invalid response format from receipt processing service"
                    });
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while processing receipt for user: {UserId}", userId);
                return StatusCode(408, new ReceiptProcessingResponse
                {
                    Success = false,
                    Error = "Request timeout. Please try again."
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while processing receipt for user: {UserId}", userId);
                return StatusCode(503, new ReceiptProcessingResponse
                {
                    Success = false,
                    Error = "Network error. Please check your connection and try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing receipt for user: {UserId}", userId);
                return StatusCode(500, new ReceiptProcessingResponse
                {
                    Success = false,
                    Error = "An unexpected error occurred while processing the receipt"
                });
            }
        }

        [HttpPost("process-with-transaction")]
        public async Task<ActionResult<ReceiptWithTransactionResponse>> ProcessReceiptWithTransaction([FromBody] ProcessReceiptRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            try
            {
                // Validate user authentication
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized receipt processing attempt");
                    return Unauthorized(new ReceiptWithTransactionResponse
                    {
                        Success = false,
                        Error = "User not authenticated"
                    });
                }

                // Validate request
                if (string.IsNullOrEmpty(request.FileBase64))
                {
                    _logger.LogWarning("Receipt processing attempted with empty image for user: {UserId}", userId);
                    return BadRequest(new ReceiptWithTransactionResponse
                    {
                        Success = false,
                        Error = "Receipt image is required"
                    });
                }

                // Validate base64 format
                if (!IsValidBase64(request.FileBase64))
                {
                    _logger.LogWarning("Invalid base64 format provided for user: {UserId}", userId);
                    return BadRequest(new ReceiptWithTransactionResponse
                    {
                        Success = false,
                        Error = "Invalid image format"
                    });
                }

                _logger.LogInformation("Processing receipt with transaction data for user: {UserId} ({Email})", userId, userEmail);

                // Call the AWS API through the backend
                var awsApiUrl = "https://z9yrdck3l2.execute-api.us-east-1.amazonaws.com/process-receipt";
                
                using var httpClient = _httpClientFactory.CreateClient();
                
                // Set timeout for the request
                httpClient.Timeout = TimeSpan.FromSeconds(30);
                
                var awsRequest = new
                {
                    file_base64 = request.FileBase64
                };

                var jsonContent = JsonSerializer.Serialize(awsRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to AWS API for user: {UserId}", userId);

                var response = await httpClient.PostAsync(awsApiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AWS API request failed with status: {StatusCode}, Error: {Error} for user: {UserId}", 
                        response.StatusCode, errorContent, userId);
                    
                    return StatusCode((int)response.StatusCode, new ReceiptWithTransactionResponse
                    {
                        Success = false,
                        Error = $"Failed to process receipt: {response.StatusCode}",
                        Message = errorContent
                    });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var awsResponse = JsonSerializer.Deserialize<AwsReceiptResponse>(responseContent);
                    
                    if (awsResponse == null)
                    {
                        _logger.LogError("Failed to deserialize AWS response for user: {UserId}", userId);
                        return StatusCode(500, new ReceiptWithTransactionResponse
                        {
                            Success = false,
                            Error = "Invalid response from receipt processing service"
                        });
                    }

                    // Use the service to process the receipt data
                    var receiptResponse = _receiptProcessingService.ProcessReceiptData(awsResponse);

                    if (receiptResponse.Success)
                    {
                        // Transform to transaction data
                        var transactionData = _receiptProcessingService.TransformToTransactionData(receiptResponse);
                        
                        var result = new ReceiptWithTransactionResponse
                        {
                            Success = true,
                            Data = receiptResponse.Data,
                            TransactionData = transactionData,
                            Error = null,
                            Message = null
                        };

                        _logger.LogInformation("Receipt processed with transaction data for user: {UserId}. Title: {Title}, Amount: {Amount}", 
                            userId, transactionData?.Title, transactionData?.Amount);

                        return Ok(result);
                    }
                    else
                    {
                        _logger.LogWarning("Receipt processing failed for user: {UserId}. Error: {Error}", 
                            userId, receiptResponse.Error);
                        
                        return Ok(new ReceiptWithTransactionResponse
                        {
                            Success = false,
                            Data = receiptResponse.Data,
                            TransactionData = null,
                            Error = receiptResponse.Error,
                            Message = receiptResponse.Message
                        });
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse AWS response for user: {UserId}. Response: {Response}", 
                        userId, responseContent);
                    
                    return StatusCode(500, new ReceiptWithTransactionResponse
                    {
                        Success = false,
                        Error = "Invalid response format from receipt processing service"
                    });
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while processing receipt for user: {UserId}", userId);
                return StatusCode(408, new ReceiptWithTransactionResponse
                {
                    Success = false,
                    Error = "Request timeout. Please try again."
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while processing receipt for user: {UserId}", userId);
                return StatusCode(503, new ReceiptWithTransactionResponse
                {
                    Success = false,
                    Error = "Network error. Please check your connection and try again."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing receipt for user: {UserId}", userId);
                return StatusCode(500, new ReceiptWithTransactionResponse
                {
                    Success = false,
                    Error = "An unexpected error occurred while processing the receipt"
                });
            }
        }

        private static bool IsValidBase64(string base64String)
        {
            try
            {
                // Check if the string is not null or empty
                if (string.IsNullOrEmpty(base64String))
                    return false;

                // Check if the string length is valid for base64
                if (base64String.Length % 4 != 0)
                    return false;

                // Try to convert from base64
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
