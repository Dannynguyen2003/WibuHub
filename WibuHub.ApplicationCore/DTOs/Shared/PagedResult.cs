using System;
using System.Collections.Generic;
using System.Linq;

namespace WibuHub.ApplicationCore.DTOs.Shared
{
    // Thêm <T> để tái sử dụng cho nhiều kiểu dữ liệu khác nhau
    public class PagedResult<T>
    {
        // Danh sách dữ liệu của trang hiện tại
        public IEnumerable<T> Items { get; set; }

        // Trang hiện tại (thường bắt đầu từ 1)
        public int PageIndex { get; set; }

        // Số lượng item trên một trang
        public int PageSize { get; set; }

        // Tổng số lượng item trong toàn bộ Database (để tính tổng số trang)
        public long TotalCount { get; set; }

        // Tổng số trang (Tính toán tự động)
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        // Hỗ trợ UI: Có trang trước không?
        public bool HasPreviousPage => PageIndex > 1;

        // Hỗ trợ UI: Có trang sau không?
        public bool HasNextPage => PageIndex < TotalPages;

        public PagedResult() { }

        public PagedResult(IEnumerable<T> items, int pageIndex, int pageSize, long totalCount)
        {
            Items = items;
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}