using Acme.BookStore.Books;
using Acme.BookStore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities.Caching;
using Volo.Abp.EntityFrameworkCore;

namespace Acme.BookStore.Web.Pages.Books
{
    public class EditModalModel : BookStorePageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateUpdateBookDto Book { get; set; }
        public BookDto getBook { get; set; }
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
            var IdsList = new List<Guid>();
            var getBoks = await _bookAppService.GetListAsync(new PagedAndSortedResultRequestDto { SkipCount = 0, MaxResultCount = 20 });
            foreach (var item in getBoks.Items)
            {
                IdsList.Add(item.Id);
            }
            var bootDto = await GetAsync(Id);
            var book = ConvertToBookDto(bootDto);
            //var chache = await _bookCache.GetAsync(Id);
            GetAllBooks(IdsList);

            Book = ObjectMapper.Map<BookDto, CreateUpdateBookDto>(book);
        }
        //public async Task AddAllCache(BookDto books)
        //{
        //    var IdsList = new List<Guid>();
        //    var getBoks = await _bookAppService.GetListAsync(new PagedAndSortedResultRequestDto { SkipCount = 0, MaxResultCount = 20 });

        //    foreach (var item in getBoks.Items)
        //    {
        //        IdsList.Add(item.Id);
        //    }
        //    List<string> stringIdsList = IdsList.Select(id => id.ToString()).ToList();
        //    var res = await _cache.GetOrAddManyAsync(
        //        stringIdsList, //Cache key
        //        async () => await GetBookFromDatabaseAsync(bookId),
        //        () => new DistributedCacheEntryOptions
        //        {
        //            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1)
        //        };
        //}
        public async Task<BookCacheItem> GetAsync(Guid bookId)
        {
            return await _cache.GetOrAddAsync(
                bookId.ToString(), //Cache key
                async () => await GetBookFromDatabaseAsync(bookId),
                () => new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
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

        #region GetorAddAllData
        public async Task GetAllBooks(IEnumerable<Guid> bookIds)
        {
            //var getBoks = await _bookAppService.GetListAsync(new PagedAndSortedResultRequestDto { SkipCount = 0, MaxResultCount = 20 });
            var IdsList = new List<Guid>();
            var cacheKeys = bookIds.Select(id => id.ToString());
            foreach (var item in bookIds)
            {
                IdsList.Add(item);
            }
            List<string> stringIdsList = IdsList.Select(id => id.ToString()).ToList();
            var res = await _cache.GetOrAddManyAsync(
                stringIdsList, //Cache key
                async keys => await GetBooksFromDatabaseAsync(keys.Select(Guid.Parse)), // Assuming GetBooksFromDatabaseAsync takes IEnumerable<Guid>
                () => new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
                });
        }
        private async Task<List<KeyValuePair<string, BookCacheItem?>>> GetBooksFromDatabaseAsync(IEnumerable<Guid> bookIds)
        {
            var result = new List<KeyValuePair<string, BookCacheItem?>>();

            foreach (var bookId in bookIds)
            {
                try
                {
                    var bookDto = await _bookAppService.GetAsync(bookId);
                    var bookCacheItem = ConvertToBookCacheItem(bookDto);

                    result.Add(new KeyValuePair<string, BookCacheItem?>(bookId.ToString(), bookCacheItem));
                }
                catch (Exception)
                {
                    // Handle the exception, you might log it or take other actions
                    result.Add(new KeyValuePair<string, BookCacheItem?>(bookId.ToString(), null));
                }
            }

            return result;
        }

        #endregion


        //private async Task<List<BookDto>> GetAllBooks()
        //{
        //    var requestDto = new PagedAndSortedResultRequestDto(); // You may need to customize this request object based on your needs
        //    var list = await _bookAppService.GetListAsync(requestDto);
        //    var sendList = new List<BookDto>();



        //    return list;
        //}
    }
}
