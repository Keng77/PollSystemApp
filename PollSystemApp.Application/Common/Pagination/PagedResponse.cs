namespace PollSystemApp.Application.Common.Pagination
{
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; }
        public PaginationMetadata MetaData { get; set; }

        public PagedResponse(List<T> items, PaginationMetadata metaData)
        {
            Items = items;
            MetaData = metaData;
        }
    }
}