using Eshop.Application.DTOs;
using Eshop.Core.Entities;

namespace Eshop.Application.GeneralResponse
{
    public class GeneralResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
        public PaginationInfoDTO PaginationInfo { get; set; }

        public static implicit operator GeneralResponse<T>(GeneralResponse<List<Product>> v)
        {
            throw new NotImplementedException();
        }
    }
}
