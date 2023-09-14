using Acme.BookStore.Books;
using AutoMapper;

namespace Acme.BookStore.Web;

public class BookStoreWebAutoMapperProfile : Profile
{
    public BookStoreWebAutoMapperProfile()
    {

        CreateMap<BookDto, CreateUpdateBookDto>();
        //Define your AutoMapper configuration here for the Web project.
    }
}
