using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using Eshop.Application.Services;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckOutController : ControllerBase
    {
        private readonly StripeService _stripeService;
        private const string StripeWebhookSecret = "";

        public CheckOutController(StripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CheckOut()
        {
            var userId = User.FindFirstValue("uid");

            try
            {
                var sessionId = await _stripeService.CreateCheckoutSessionAsync(userId);
                return Ok(new { SessionId = sessionId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        //If You wnat to handle it from front
        [HttpPost("success")]
        public async Task<IActionResult> PaymentSuccess([FromQuery] string session_id)
        {
            try
            {
                bool success = await _stripeService.HandleSuccessfulPayment(session_id);
                if (success)
                {
                    return Ok(new { Message = "Payment processed successfully" });
                }
                else
                {
                    return BadRequest(new { Error = "Failed to process payment" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> PaymentFailed([FromQuery] string session_id)
        {
            try
            {
                bool success = await _stripeService.ProcessFailedPaymentAsync(session_id);
                if (success)
                {
                    return Ok(new { Message = "Payment cancellation processed successfully" });
                }
                else
                {
                    return BadRequest(new { Error = "Failed to process payment cancellation" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        ///  if you want to handle by hooks  (For the one who will implmenet the payment )
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [Route("handle")]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {

                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], StripeWebhookSecret);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        var sessionCompleted = stripeEvent.Data.Object as Session;
                        if (sessionCompleted != null)
                        {
                            await _stripeService.HandleSuccessfulPayment(sessionCompleted.Id);
                        }
                        break;

                    case "payment_intent.payment_failed":
                        var failedPaymentSession = stripeEvent.Data.Object as Session;
                        if (failedPaymentSession != null)
                        {
                            await _stripeService.ProcessFailedPaymentAsync(failedPaymentSession.Id);
                        }
                        break;

                    default:
                        return BadRequest("Unhandled event type");
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return BadRequest(new { Error = $"Webhook Error: {ex.Message}" });
            }
        }
    }
}
