using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Eshop.Application.DTOs;
using Eshop.Application.Interfaces.Services;

namespace Eshop.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService reviewService;

        public ReviewController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }
        [HttpPost("add-review")]

        public async Task<IActionResult> AddReview([FromForm] AddReviewDTO addReview)
        {

            string userId = User.FindFirstValue("uid");
            bool res = await reviewService.AddReview(addReview, userId);
            if (res)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res);
            }
        }
    }
}
