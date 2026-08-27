namespace AMYL.Api.Shared.Abstractions
{
    public class PaginatedList<T>
    {
        public List<T>? Value { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public int TotalCount { get; set; }
    }
}
