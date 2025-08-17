using AltecSystem.Application.DTOs.User;
using MediatR;
using System.Collections.Generic;

namespace AltecSystem.Application.Queries.User
{
    public class GetUsersByNameQuery : IRequest<IReadOnlyList<UserDetailsDto>>
    {
        public string Search { get; }
        public int Page { get; }
        public int PageSize { get; }

        public GetUsersByNameQuery(string search, int page, int pageSize)
        {
            Search = search;
            Page = page;
            PageSize = pageSize;
        }
    }
}