using Microsoft.AspNetCore.Identity;
using RideShare.Application.Common.Interfaces;
using RideShare.Identity.Entities;

namespace RideShare.Identity.Services;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<Guid>> roleManager) : IIdentityService
{
    public async Task<RegisterUserResult> RegisterUserAsync(string email, string password, string role, CancellationToken cancellationToken = default)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return new RegisterUserResult(false, null, ["An account with this email already exists."]);

        var user = new ApplicationUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
            return new RegisterUserResult(false, null, createResult.Errors.Select(e => e.Description).ToList());

        await EnsureRoleExistsAsync(role);
        await userManager.AddToRoleAsync(user, role);

        return new RegisterUserResult(true, user.Id, []);
    }

    public async Task<ValidateCredentialsResult> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return new ValidateCredentialsResult(false, null, null, []);

        var passwordValid = await userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return new ValidateCredentialsResult(false, null, null, []);

        var roles = await userManager.GetRolesAsync(user);
        return new ValidateCredentialsResult(true, user.Id, user.Email, roles.ToList());
    }

    private async Task EnsureRoleExistsAsync(string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
    }
}
