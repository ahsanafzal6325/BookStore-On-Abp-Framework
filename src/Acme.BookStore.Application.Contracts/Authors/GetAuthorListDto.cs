using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Authors
{
    public class GetAuthorListDto : PagedAndSortedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
