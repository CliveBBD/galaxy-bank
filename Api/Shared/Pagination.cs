using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Shared
{
    public enum SortOrder {
        ASC,
        DESC
    }

    public class Pagination
    {
        public string? SortColumn { get; set; } = null;
        public SortOrder? SortOrder { get; set; } = Shared.SortOrder.ASC;
        public int? Offset { get; set; } = 0;
        public int? Limit { get; set; } = int.MaxValue;

    }
}