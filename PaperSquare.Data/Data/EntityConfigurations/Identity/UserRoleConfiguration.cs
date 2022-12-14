using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaperSquare.Core.Models.Identity;

namespace PaperSquare.Data.Data.EntityConfigurations.Identity
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable(nameof(UserRole));
            builder.HasOne(userRole => userRole.User).WithMany(user => user.Roles).HasForeignKey(userRole => userRole.UserId).IsRequired();
            builder.HasOne(userRole => userRole.Role).WithMany(role => role.Roles).HasForeignKey(userRole => userRole.RoleId).IsRequired();
        }
    }
}
