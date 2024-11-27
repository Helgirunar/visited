using backend.Data;
using backend.Models;
using backend1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Eventing.Reader;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.Models.Inputs;



namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VisitCounterController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public VisitCounterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        
        public async Task<IActionResult> All()
        {
            var visitCounters = await _context.VisitCounters.ToListAsync();

            return Ok(visitCounters);
        }

        [HttpGet("getPrivate")]
        public async Task<IActionResult> getByUserId()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }

            var visitCounters = await _context.VisitCounters
                .Where(vc => vc.UserId == userId)  // Assuming VisitCounters has a UserId field
                .ToListAsync();

            return Ok(visitCounters);
        }

        [HttpGet("id")]
        public async Task<IActionResult> Get(int id)
        {
            // Find the VisitCounter by ID from the database
            var visitCounter = await _context.VisitCounters
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visitCounter == null)
            {
                // Return a 404 if the VisitCounter with the specified ID is not found
                return NotFound(new { message = $"VisitCounter with ID {id} not found" });
            }

            // Return the found VisitCounter as JSON
            return Ok(visitCounter);
        }

        // POST api/visitcounter
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VisitCounter visitCounter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }
            visitCounter.UserId = userId;
            Console.WriteLine(ModelState);
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            // Add the new VisitCounter to the DbContext
            _context.VisitCounters.Add(visitCounter);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Return a response with the created VisitCounter, including its ID
            return CreatedAtAction(
                nameof(Get), // Name of the GET method to show the created resource
                new { id = visitCounter.Id }, // Route values to generate the correct URL
                visitCounter); // Return the created resource
        }
        // PUT api/visitcounter/{id} - Update method
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] VisitCounter updatedVisitCounter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }
            Console.WriteLine(JsonSerializer.Serialize(updatedVisitCounter));

            // Find the existing VisitCounter by ID
            var existingVisitCounter = await _context.VisitCounters.FirstOrDefaultAsync(v => v.Id == id);

            if (existingVisitCounter == null)
            {
                // If the VisitCounter with the given ID doesn't exist, return NotFound
                return NotFound(new { message = $"VisitCounter with ID {id} not found" });
            }

            //Update part of the model which is allowed to be updated.
            existingVisitCounter.Name = updatedVisitCounter.Name;
            existingVisitCounter.Address = updatedVisitCounter.Address;
            existingVisitCounter.Count = updatedVisitCounter.Count;

            _context.VisitCounters.Update(existingVisitCounter);
            // Update the fields of the existing VisitCounter with the new data
            existingVisitCounter.Count += 1;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated VisitCounter
            return Ok(existingVisitCounter);
        }

        // PUT api/visitcounter/{id}/increase - Increase counter method
        [HttpPost("{id}/increase")]
        public async Task<IActionResult> Increase(int id)
        {
            Console.WriteLine(id);

            // Find the existing VisitCounter by ID
            var existingVisitCounter = await _context.VisitCounters.FirstOrDefaultAsync(v => v.Id == id);

            if (existingVisitCounter == null)
            {
                // If the VisitCounter with the given ID doesn't exist, return NotFound
                return NotFound(new { message = $"VisitCounter with ID {id} not found" });
            }

            // Update the fields of the existing VisitCounter with the new data
            existingVisitCounter.Count += 1;

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Return the updated VisitCounter
            return Ok(existingVisitCounter);
        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> Remove(int id)
        {

            var existingVisitCounter = await _context.VisitCounters.FirstOrDefaultAsync(v => v.Id == id);

            if (existingVisitCounter == null)
            {
                // If the VisitCounter with the given ID doesn't exist, return NotFound
                return NotFound(new { message = $"VisitCounter with ID {id} not found" });
            }
            _context.VisitCounters.Remove(existingVisitCounter);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
