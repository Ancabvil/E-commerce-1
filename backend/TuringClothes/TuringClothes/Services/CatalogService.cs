using TuringClothes.Database;
using TuringClothes.Enums;
using TuringClothes.Model;
using TuringClothes.Pagination;

namespace TuringClothes.Services
{
    public class CatalogService
    {

        private readonly SmartSearchService _smartSearchService;

        public CatalogService(SmartSearchService smartSearchService)
        {
            _smartSearchService = smartSearchService;
        }

        public PagedResults<Product> GetPaginationCatalog(PaginationParams request)
        {

            var query = _smartSearchService.Search(request.Query);

            query = request.OrderBy switch
            {
                OrderCatalog.OrderField.Price => request.Direction == OrderCatalog.OrderDirection.Ascending
                ? query.OrderBy(x => x.Price)
                : query.OrderByDescending(x => x.Price),
                OrderCatalog.OrderField.Name => request.Direction == OrderCatalog.OrderDirection.Ascending
                ? query.OrderBy(x => x.Name)
                : query.OrderByDescending(x => x.Name),
                _ => query
            };
            var totalNumberOfRecords = query.Count();

            var totalPageCount = (int)Math.Ceiling((double)totalNumberOfRecords / request.PageSize);

            if (totalPageCount < request.PageNumber)
            {
                request.PageNumber = totalPageCount;
            }

            var skipAmount = request.PageSize * (request.PageNumber - 1);

            var results = query.Skip(skipAmount).Take(request.PageSize).ToList();

            

            
            return new PagedResults<Product>
            {
                Results = results,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalNumberOfPages = totalPageCount,
                TotalNumberOfRecords = totalNumberOfRecords
            };



        }


    }
}
