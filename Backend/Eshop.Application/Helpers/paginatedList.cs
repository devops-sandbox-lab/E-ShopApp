namespace Eshop.Application.Helpers
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int TotalItems { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public PaginatedList() { }

        public PaginatedList(List<T> items, int totalItems, int page, int pageSize = 6)
        {
            Items = items;
            TotalItems = totalItems;
            PageSize = pageSize;

            TotalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            CurrentPage = Math.Min(Math.Max(page, 1), TotalPages);

            int maxPagesToShow = 10;
            int halfMaxPagesToShow = maxPagesToShow / 2;

            StartPage = Math.Max(CurrentPage - halfMaxPagesToShow, 1);
            EndPage = Math.Min(StartPage + maxPagesToShow - 1, TotalPages);
            if (EndPage == TotalPages)
            {
                StartPage = Math.Max(TotalPages - maxPagesToShow + 1, 1);
            }
        }
    }
}