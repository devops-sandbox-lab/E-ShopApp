using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eshop.Application.DTOs;

namespace Eshop.Application.Interfaces.Services
{
    public interface IReviewService
    {
        Task<bool> AddReview(AddReviewDTO addReview, string userId);
    }
}
