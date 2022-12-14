using Ardalis.GuardClauses;
using Ardalis.Result;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PaperSquare.Core.Infrastructure.CurrentUserAccessor;
using PaperSquare.Core.Models.Identity;
using PaperSquare.Core.Permissions;
using PaperSquare.Data.Data;
using PaperSquare.Infrastructure.Features.UserManagement.Dto;
using PaperSquare.Infrastructure.Shared;

namespace PaperSquare.Infrastructure.Features.UserManagement
{
    public class UserService: CRUDService<User, UserDto, string, UserSearchDto, UserInsertDto, UserUpdateDto>, IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUser _currentUser;
        public UserService(PaperSquareDbContext paperSquareDbContext, UserManager<User> userManager, IMapper mapper, ICurrentUser currentUser) : base(paperSquareDbContext, mapper)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public override async Task<Result<UserDto>> Insert(UserInsertDto insert)
        {
            Guard.Against.Null(insert, nameof(insert));

            var user = _mapper.Map<User>(insert);

            SetDefaultsForUser(user);

            if (!CheckIfPasswordsMatch(insert))
            {
                return Result.Error("Password and confirm password doesn`t match!");
            }

            var result = await _userManager.CreateAsync(user, insert.Password);

            if (!result.Succeeded)
            {
                return Result.Error(result.Errors.Select(err => err.Description).ToArray());
            }

            result = await _userManager.AddToRoleAsync(user, Roles.RegisteredUser);

            if (!result.Succeeded)
            {
                await _dbContext.Database.RollbackTransactionAsync();

                return Result.Error(result.Errors.Select(err => err.Description).ToArray());
            }

            return Result.SuccessWithMessage("User successfully added!");
        }

        public override async Task<Result<UserDto>> Update(string type, UserUpdateDto update)
        {
            Guard.Against.Null(update, nameof(update));

            if (type != _currentUser.Id)
            {
                return Result.Error("You don`t have permissions to edit!");
            }

            var user = await _entities.FindAsync(type);

            if (user is null)
            {
                return Result.NotFound("User not found!");
            }

            user.LastUpdated = DateTime.UtcNow;

            _mapper.Map(update, user); 

            await _dbContext.SaveChangesAsync();

            return Result.Success(_mapper.Map<UserDto>(user));
        }

        public override async Task<Result<UserDto>> Delete(string type)
        {
            var user = await _entities.FindAsync(type);

            if (user is null)
            {
                return Result.NotFound("User not found!");
            }

            user.IsDeleted = true;

            await _dbContext.SaveChangesAsync();

            return Result.Success(_mapper.Map<UserDto>(user));
        }

        #region Utils

        private bool CheckIfPasswordsMatch(UserInsertDto request)
        {
            return request.Password == request.ConfirmPassword;
        }

        private void SetDefaultsForUser(User user)
        {
            user.CreationDate = DateTime.UtcNow;
            user.BirthDate = DateTime.UtcNow;
        }

        public override IQueryable<User> ApplyFilters(IQueryable<User> query, UserSearchDto search = null)
        {
            var filteredQuery = base.ApplyFilters(query, search);

            if (!string.IsNullOrWhiteSpace(search.FirstName))
            {
                filteredQuery = filteredQuery.Where(user => user.Firstname.ToLower().Contains(search.FirstName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(search.LastName))
            {
                filteredQuery = filteredQuery.Where(user => user.Lastname.ToLower().Contains(search.LastName.ToLower()));
            }

            return filteredQuery;
        }

        #endregion Utils
    }
}
