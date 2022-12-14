using Ardalis.Result;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PaperSquare.Data.Data;
using PaperSquare.Infrastructure.Extensions;
using PaperSquare.Infrastructure.Shared.Dto;

namespace PaperSquare.Infrastructure.Shared
{
    public class QueryService<TEntity, TType, TModel, TSearch> : IQueryService<TModel, TSearch, TType> where TEntity : class where TModel : class where TSearch : SearchDto
    {
        protected readonly PaperSquareDbContext _dbContext;
        protected readonly IMapper _mapper;
        protected DbSet<TEntity> _entities => _dbContext.Set<TEntity>();

        public QueryService(PaperSquareDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public virtual async Task<Result<IEnumerable<TModel>>> GetAll(TSearch search = null)
        {
            var entities = ApplyFilters(_entities, search);

            return Result.Success(_mapper.Map<IEnumerable<TModel>>(entities.ToPagedList(search.Page, search.PageSize)));
        }

        public virtual async Task<Result<TModel>> GetById(TType id)
        {
            var entity = await _entities.FindAsync(id);
            
            return Result.Success(_mapper.Map<TModel>(entity));
        }

        #region Virtual

        public virtual IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, TSearch search = null)
        {
            return query;
        }

        #endregion Virtual
    }
}
