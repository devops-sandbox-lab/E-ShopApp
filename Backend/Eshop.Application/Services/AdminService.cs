using Application.Helpers;
using AutoMapper;
using Eshop.Application.DTOs.Profile;
using Eshop.Application.GeneralResponse;
using Eshop.Application.Interfaces.Repository;
using Eshop.Application.Interfaces.Services;
using Eshop.Core.Entities;
using Eshop.Core.Enums;

namespace Eshop.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAccountRepository accountRepository;
        private readonly IAdminRepository adminRepository;
        private readonly IMapper mapper;

        public AdminService(IAccountRepository accountRepository, IAdminRepository adminRepository, IMapper mapper)
        {
            this.accountRepository = accountRepository;
            this.adminRepository = adminRepository;
            this.mapper = mapper;
        }
        public async Task<GeneralResponse<object>> AcceptSellerAccount(string sellerId)
        {
            Seller seller = await accountRepository.GetSellerByIdAsync(sellerId);
            if (seller == null)
            {
                return new GeneralResponse<object>
                {
                    Data = null,
                    Message = "Can not find seller",
                    Succeeded = false,
                    Errors = new List<string> { "Can not find seller" }
                };
            }
            if (seller.AccountStatus == SellerAccountStatus.Accepted)
            {
                return new GeneralResponse<object>
                {
                    Data = seller.AccountStatus.ToString(),
                    Message = "seller already accepted",
                    Succeeded = true,
                    Errors = null
                };
            }
            seller.AccountStatus = SellerAccountStatus.Accepted;
            await accountRepository.UpdateSellerAsync(seller);
            await accountRepository.save();

            return new GeneralResponse<object>
            {
                Data = seller.AccountStatus.ToString(),
                Message = "Seller account has been accepted",
                Succeeded = true,
                Errors = null
            };

        }

        public async Task<GeneralResponse<object>> DeclineSellerAccount(string sellerId)
        {
            Seller seller = await accountRepository.GetSellerByIdAsync(sellerId);
            if (seller == null)
            {
                return new GeneralResponse<object>
                {
                    Data = null,
                    Message = "Can not find seller",
                    Succeeded = false,
                    Errors = new List<string> { "Can not find seller" }
                };
            }
            if (seller.AccountStatus == SellerAccountStatus.Decline)
            {
                return new GeneralResponse<object>
                {
                    Data = seller.AccountStatus.ToString(),
                    Message = "seller already Declined",
                    Succeeded = true,
                    Errors = null
                };
            }
            seller.AccountStatus = SellerAccountStatus.Decline;
            await accountRepository.UpdateSellerAsync(seller);
            await accountRepository.save();

            return new GeneralResponse<object>
            {
                Data = seller.AccountStatus.ToString(),
                Message = "Seller account has been Decline",
                Succeeded = true,
                Errors = null
            };

        }

        public async Task<GeneralResponse<List<GetSellerProfileDTO>>> GetAllSellers(int page, int pageSize, SellerAccountStatus? status, bool? isBlocked)
        {
            var sellers = await adminRepository.getAllSellersAsync(status, isBlocked);

            var paginatedList = PaginationHelper.Paginate(sellers, page, pageSize);

            if (!paginatedList.Items.Any())
            {
                return new GeneralResponse<List<GetSellerProfileDTO>>
                {
                    Data = null,
                    Message = "There is no sellers",
                    Succeeded = true,
                    Errors = null
                };
            }
            var paginationInfo = PaginationHelper.GetPaginationInfo(paginatedList);

            List<GetSellerProfileDTO> SellerDTOs = mapper.Map<List<GetSellerProfileDTO>>(paginatedList.Items);

            var response = new GeneralResponse<List<GetSellerProfileDTO>>
            {
                Data = SellerDTOs,
                Message = "Sellers retrieved successfully",
                Succeeded = true,
                Errors = null,
                PaginationInfo = paginationInfo
            };

            return response;
        }
    }
}
