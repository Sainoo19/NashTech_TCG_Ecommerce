using System;
using System.Collections.Generic;
using System.Text;

namespace NashTech_TCG_ShareViewModels.ViewModels
{
    public class PagedResultViewModel<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public PagedResultViewModel()
        {
            Items = new List<T>();
        }

        public PagedResultViewModel(IEnumerable<T> items, int currentPage, int pageSize, int totalCount)
        {
            Items = items;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}
