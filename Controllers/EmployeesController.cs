using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using TestWeb.DbContexts;
using TestWeb.Dtos;
using TestWeb.Entities;
using TestWeb.Exceptions;

namespace TestWeb.Controllers;

[ApiController]
[Route("[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db) {
        _db = db;
    }

    [HttpGet]
    public async Task<IEnumerable<EmployeeRowDto>> List(string filter)
    {
        var query = _db.Employees.Select(e => new EmployeeRowDto
        {
            Id = e.Id,
            NameSurname = $"{e.Surname} {e.Name}"
        });

        if (!string.IsNullOrEmpty(filter))
        {
            query = query.Where(e => e.NameSurname.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        return await query.ToListAsync();
    }

    [HttpPost]
    public async Task AddVacation(AddVacationDto dto)
    {
        var nameSurname = dto.NameSurname.Split(' ');
        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.Name == nameSurname[1] && e.Surname == nameSurname[0]);

        if (dto.End < dto.Start)
        {
            throw new HttpException(400, "The End Date can't be minor than the Start Date ");
        }
        if (employee == null)
        {
            throw new HttpException(404, "Employee not found");
        }
        employee.Vacations.Add(new VacationEntity { Start = dto.Start, End = dto.End });
        await _db.SaveChangesAsync();

    }



    [HttpGet]
    public async Task<EmployeeVacationDto> GetVacationDays(int employeeId) {
        var employee = await _db.Employees
            .Include(x => x.Vacations)
            .SingleOrDefaultAsync(e => e.Id == employeeId);
        if (employee == null) 
        {
            throw new HttpException(404, "Employee not found");
        }
        var result = employee.Vacations.Aggregate(TimeSpan.FromDays(0), (acc, curr) => acc += curr.End - curr.Start);

        return new EmployeeVacationDto {
            NameSurname = $"{employee.Name} {employee.Surname}",
            TotalVacationDays = 0
        };
    }
}
