using ExpMedia.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using System.Threading;
using ExpMedia.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpMedia.Application.Activities
{
    public class List
    {
        public class Query : IRequest<List<Activity>> { }
        public class Tata : IRequest<List<Activity>> { }

        public class Handler : IRequestHandler<Query, List<Activity>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<List<Activity>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _context.Activities.OrderByDescending(w=> w.Title).AsNoTracking().ToListAsync();
            }
      
        }


    }
}
