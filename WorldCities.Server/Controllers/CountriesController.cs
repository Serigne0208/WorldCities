using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorldCities.Server.Data;
using WorldCities.Server.Data.Models;

namespace WorldCities.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CountriesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/Countries
        [HttpGet]
        public async Task<ActionResult<ApiResult<CountryDTO>>> GetCountries(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
        {
            return await ApiResult<CountryDTO>.CreateAsync(
                    _context.Countries.AsNoTracking()
                       .Select(c => new CountryDTO()
                        {
                           Id = c.Id,
                           Name = c.Name,
                           ISO2 = c.ISO2,
                           ISO3 = c.ISO3,
                           TotCities = c.Cities!.Count
                        }),
                    pageIndex,
                    pageSize,
                    sortColumn,
                    sortOrder,
                    filterColumn,
                    filterQuery);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Country>> GetCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            return country;
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "RegisteredUser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, Country country)
        {
            if (id != country.Id)
            {
                return BadRequest();
            }

            _context.Entry(country).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "RegisteredUser")]
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(int id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }

        [HttpPost]
        [Route("IsDupeField")]
        public bool IsDupeField(
              int countryId,
              string fieldName,
              string fieldValue)
        {
            if (string.IsNullOrEmpty(fieldValue)) return false;

            switch (fieldName)
            {

                case "name":
                    return _context.Countries.Any(
                    c => c.Name.ToLower() == fieldValue.ToLower() && c.Id != countryId);
                case "iso2":
                    return _context.Countries.Any(
                        c => c.ISO2 == fieldValue && c.Id != countryId);
                case "iso3":
                    return _context.Countries.Any(
                    c => c.ISO3 == fieldValue && c.Id != countryId);
                default:
                    return false;
                  
            }

            // Alternative approach (using System.Linq.Dynamic.Core)
            //return (ApiResult<Country>.IsValidProperty(fieldName, true))
            //? _context.Countries.Any(
            //string.Format("{0} == @0 && Id != @1", fieldName),
            //fieldValue,
            //countryId)
            //: false;
        }

        //[HttpPost]
        //[Route("IsDupeField")]
        //public IActionResult IsDupeField([FromBody] IsDupeFieldDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    bool isDuplicate = dto.FieldName switch
        //    {
        //        "name" => _context.Countries.Any(c => c.Name == dto.FieldValue && c.Id != dto.CountryId),
        //        "iso2" => _context.Countries.Any(c => c.ISO2 == dto.FieldValue && c.Id != dto.CountryId),
        //        "iso3" => _context.Countries.Any(c => c.ISO3 == dto.FieldValue && c.Id != dto.CountryId),
        //        _ => false
        //    };

        //    return Ok(isDuplicate);
        //}

        //public class IsDupeFieldDto
        //{
        //    [Required]
        //    public int CountryId { get; set; }

        //    [Required]
        //    [RegularExpression("^(name|iso2|iso3)$", ErrorMessage = "Invalid field name.")]
        //    public string FieldName { get; set; } = string.Empty;

        //    [Required]
        //    [StringLength(100, ErrorMessage = "Field value is too long.")]
        //    public string FieldValue { get; set; } = string.Empty;
        //}

    }
}