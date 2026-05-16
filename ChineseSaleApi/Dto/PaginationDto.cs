namespace ChineseSaleApi.Dto
{
    public class PaginationParamsDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
    public class PaginatedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage => (PageNumber * PageSize) < TotalCount;
        public bool HasPreviousPage => PageNumber > 1;
    }
}
