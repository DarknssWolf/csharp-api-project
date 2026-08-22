using System.Threading;
using CSharpAPIProject.Application.Interfaces.Data;
using CSharpAPIProject.Application.Interfaces.Services;
using CSharpAPIProject.Application.Interfaces.Services.Identity_Framework;
using CSharpAPIProject.Domain.Contracts.User;
using CSharpAPIProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPIProject.Infrastructure.Identity;

public class UserDataService(
    IGlobalIdentityService globalIdentityService,
    IApplicationDbContext context
) : IUserDataService
{
    
    public async Task<UserData?> GetUserData(string userId)
    {
        var applicationUser = await globalIdentityService.FindUserByIdAsync(userId, false);
        if (applicationUser == null) return null;

        var branchId = applicationUser.BranchId;
        var companyId = applicationUser.CompanyId;
        var departmentId = applicationUser.DepartmentId;
        var designationId = applicationUser.DesignationId;
        var teamId = applicationUser.TeamId;
        var provinceId = applicationUser.ProvinceId;
        var residenceCountryId = applicationUser.ResidenceCountryId;
        var serviceCountryId = applicationUser.ServiceCountryId;
        var registrationDate = applicationUser.CreationDate ?? DateTimeOffset.MinValue;

        var branchName = await GetBranchNameAsync(branchId);
        var companyName = await GetCompanyNameAsync(companyId);
        var departmentName = await GetDepartmentNameAsync(departmentId);
        var designationName = await GetDesignationNameAsync(designationId);
        var provinceName = await GetProvinceNameAsync(provinceId);
        var residenceCountryName = await GetCountryNameAsync(residenceCountryId);
        var serviceCountryName = await GetCountryNameAsync(serviceCountryId);
        var teamName = await GetTeamNameAsync(teamId);

        return new UserData()
        {
            BranchId = branchId,
            BranchName = branchName,
            CompanyId = companyId,
            CompanyName = companyName,
            DepartmentId = departmentId,
            DepartmentName = departmentName,
            DesignationId = designationId,
            DesignationName = designationName,
            ProvinceId = provinceId,
            ProvinceName = provinceName,
            ResidenceCountryId = residenceCountryId,
            ResidenceCountryName = residenceCountryName,
            ServiceCountryId = serviceCountryId,
            ServiceCountryName = serviceCountryName,
            TeamId = teamId,
            TeamName = teamName,
            RegistrationDate = registrationDate
        };
    }

    private async Task<string?> GetBranchNameAsync(Guid? branchId)
    {
        if (branchId == null) return null;
        return await context.Branches
            .AsNoTracking()
            .Where(b => b.BranchId == branchId)
            .Select(b => b.Name)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetCompanyNameAsync(Guid? companyId)
    {
        if (companyId == null) return null;
        return await context.Companies
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetDepartmentNameAsync(Guid? departmentId)
    {
        if (departmentId == null) return null;
        return await context.Departments
            .AsNoTracking()
            .Where(d => d.DepartmentId == departmentId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetDesignationNameAsync(Guid? designationId)
    {
        if (designationId == null) return null;
        return await context.Designations
            .AsNoTracking()
            .Where(d => d.DesignationId == designationId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetProvinceNameAsync(Guid? provinceId)
    {
        if (provinceId == null) return null;
        return await context.Provinces
            .AsNoTracking()
            .Where(p => p.ProvinceId == provinceId)
            .Select(p => p.Name)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetCountryNameAsync(Guid? countryId)
    {
        if (countryId == null) return null;
        return await context.Countries
            .AsNoTracking()
            .Where(c => c.CountryId == countryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
    }

    private async Task<string?> GetTeamNameAsync(Guid? teamId)
    {
        if (teamId == null) return null;
        return await context.Teams
            .AsNoTracking()
            .Where(t => t.TeamId == teamId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync();
    }

}
