using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public class AuthorsResourceParameters
    {
        int pageSize = 10;
        const int MaxPageSize = 20;
        public int PageNumber { get; set; } = 1;
        public string Genre { get; set; }
        public string SearchQuery { get; set; }
        public string OrderBy { get; set; } = "Name";
        public string Fields { get; set; }

        public int PageSize
        {
            get => pageSize;
            set => pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

    }
}
