namespace Eshop.Application.DTOs
{
    public class PaginationInfoDTO
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
    }
}
