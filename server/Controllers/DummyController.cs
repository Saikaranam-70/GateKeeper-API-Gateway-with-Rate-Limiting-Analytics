using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using GateKeeper.Database;
using GateKeeper.Services;
using GateKeeper.Exceptions;

namespace GateKeeper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DummyController : ControllerBase
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly ICacheService _cacheService;

        public DummyController(DbConnectionFactory dbConnectionFactory, ICacheService cacheService)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDummyData()
        {
            // 1. Database Connection Check (Queries Current PostgreSQL time)
            DateTime dbTime;
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                dbTime = await connection.ExecuteScalarAsync<DateTime>("SELECT NOW();");
            }

            // 2. Redis Cache Connection Check (Increments a counter in Redis)
            var cacheKey = "gatekeeper:dummy:hit_counter";
            var cachedHits = await _cacheService.GetAsync<int?>(cacheKey);
            
            int currentHits = (cachedHits ?? 0) + 1;
            await _cacheService.SetAsync(cacheKey, currentHits, TimeSpan.FromMinutes(5));

            // 3. Showcase Error Handling trigger (via query param 'triggerError=true')
            if (HttpContext.Request.Query.ContainsKey("triggerError") && HttpContext.Request.Query["triggerError"] == "true")
            {
                throw new BadRequestException("This is a dummy bad request error to showcase global error handling middleware.");
            }

            return Ok(new
            {
                message = "GateKeeper dummy end-to-end API is running successfully!",
                databaseTime = dbTime,
                redisHitCounter = currentHits,
                isCached = cachedHits.HasValue
            });
        }
    }
}
