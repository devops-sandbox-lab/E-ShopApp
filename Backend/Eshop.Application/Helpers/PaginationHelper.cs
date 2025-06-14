using Eshop.Application.DTOs;
using Eshop.Application.Helpers;

namespace Application.Helpers
{
    public static class PaginationHelper
    {

        public static PaginatedList<T> Paginate<T>(IQueryable<T> source, int page, int pageSize)
        {
            var totalItems = source.Count();
            var paginatedItems = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(paginatedItems, totalItems, page, pageSize);
        }


        public static PaginationInfoDTO GetPaginationInfo<T>(PaginatedList<T> paginatedList)
        {
            return new PaginationInfoDTO
            {
                TotalItems = paginatedList.TotalItems,
                TotalPages = paginatedList.TotalPages,
                CurrentPage = paginatedList.CurrentPage,
                StartPage = paginatedList.StartPage,
                EndPage = paginatedList.EndPage
            };
        }
    }
}
