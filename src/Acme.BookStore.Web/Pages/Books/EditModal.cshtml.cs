using Acme.BookStore.Books;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities.Caching;

namespace Acme.BookStore.Web.Pages.Books
{
    public class EditModalModel : BookStorePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateUpdateBookDto Book { get; set; }

        private readonly IBookAppService _bookAppService;

        private readonly IEntityCache<BookDto, Guid> _bookCache;
        private readonly IDistributedCache<BookCacheItem> _cache;
        public EditModalModel(IBookAppService bookAppService, IEntityCache<BookDto, Guid> bookCache, IDistributedCache<BookCacheItem> cache)
        {
            _bookAppService = bookAppService;
            _bookCache = bookCache;
            _cache = cache;
        }

        public async Task OnGetAsync()
        {
            //var bookDto = await _bookAppService.GetAsync(Id);
            var bootDto = await GetAsync(Id);
            var book = ConvertToBookDto(bootDto);
            //var chache = await _bookCache.GetAsync(Id);
            Book = ObjectMapper.Map<BookDto, CreateUpdateBookDto>(book);
        }
        public async Task<BookCacheItem> GetAsync(Guid bookId)
        {
            return await _cache.GetOrAddAsync(
                bookId.ToString(), //Cache key
                async () => await GetBookFromDatabaseAsync(bookId),
                () => new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1)
                }
            );
        }
        private async Task<BookCacheItem> GetBookFromDatabaseAsync(Guid bookId)
        {
            try
            {
                var bookDto = await _bookAppService.GetAsync(Id);
                var ret = ConvertToBookCacheItem(bookDto);
                return ret;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private BookCacheItem ConvertToBookCacheItem(BookDto bookDto)
        {
            // Your conversion logic here
            return new BookCacheItem
            {
                Name = bookDto.Name,
                Type = bookDto.Type,
                PublishDate = bookDto.PublishDate,
                Price = bookDto.Price
            };
        }
        private BookDto ConvertToBookDto(BookCacheItem bookCache)
        {
            return new BookDto
            {
                Name = bookCache.Name,
                Type = bookCache.Type,
                PublishDate = bookCache.PublishDate,
                Price = bookCache.Price
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await _bookAppService.UpdateAsync(Id, Book);
            return NoContent();
        }
    }
}
