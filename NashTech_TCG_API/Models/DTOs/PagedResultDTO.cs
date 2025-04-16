namespace NashTech_TCG_API.Models.DTOs
{
    public class PagedResultDTO<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public PagedResultDTO()
        {
            Items = new List<T>();
        }

        public PagedResultDTO(IEnumerable<T> items, int currentPage, int pageSize, int totalCount)
        {
            Items = items;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}