using ChineseSaleApi.Attributes;
using ChineseSaleApi.Dto;
using ChineseSaleApi.ServiceInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ChineseSaleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryService _service;
        private readonly IFileExportService _exportService;
        private readonly ILogger<LotteryController> _logger;

        public LotteryController(ILotteryService service, IFileExportService exportService, ILogger<LotteryController> logger)
        {
            _service = service;
            _exportService = exportService;
            _logger = logger;
        }

        //read
        [HttpGet]
        public async Task<IActionResult> GetAllLotteries()
        {
            try
            {
                var lotteries = await _service.GetAllLotteries();
                return Ok(lotteries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all lotteries.");
                return StatusCode(500, "An unexpected error occurred while retrieving lotteries.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLotteryById(int id)
        {
            try
            {
                var lottery = await _service.GetLotteryById(id);
                if (lottery == null)
                {
                    return NotFound();
                }
                return Ok(lottery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get lottery by id {LotteryId}.", id);
                return StatusCode(500, "An unexpected error occurred while retrieving the lottery.");
            }
        }
        [Authorize]
        [Admin]
        [HttpGet("{id}/report")]
        public async Task<IActionResult> GetLotteryReport(int id)
        {
            try
            {
                var report = await _service.GetLotteryReport(id);
                if (report == null)
                {
                    return NotFound($"Lottery with ID {id} not found.");
                }
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate lottery report for lottery {LotteryId}.", id);
                return StatusCode(500, "An unexpected error occurred while generating the lottery report.");
            }
        }
        [Authorize]
        [Admin]
        [HttpGet("{id}/report/export")]
        public async Task<IActionResult> ExportLotteryReport(int id, [FromQuery] string format = "csv")
        {
            try
            {
                var report = await _service.GetLotteryReport(id);
                if (report == null)
                {
                    return NotFound($"Lottery with ID {id} not found.");
                }

                byte[] fileContent;
                string fileName;
                string contentType;

                switch (format.ToLower())
                {
                    case "csv":
                        fileContent = _exportService.ExportReportToCsv(report);
                        fileName = _exportService.GetFileName(id, "csv");
                        contentType = _exportService.GetContentType("csv");
                        break;

                    case "json":
                        fileContent = _exportService.ExportReportToJson(report);
                        fileName = _exportService.GetFileName(id, "json");
                        contentType = _exportService.GetContentType("json");
                        break;

                    case "pdf":
                    case "html":
                        fileContent = _exportService.ExportReportToPdf(report);
                        fileName = _exportService.GetFileName(id, "html");
                        contentType = _exportService.GetContentType("html");
                        break;

                    default:
                        return BadRequest("Unsupported format. Please use 'csv', 'json', or 'pdf'.");
                }

                return File(fileContent, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export lottery report for lottery {LotteryId}.", id);
                return StatusCode(500, "An unexpected error occurred while exporting the lottery report.");
            }
        }

        [Authorize]
        [Admin]
        [HttpGet("giftId/{giftId}")]
        public async Task<IActionResult> lottery(int giftId)
        {
            try
            {
                var lottery = await _service.Lottery(giftId);
                if (lottery == null)
                {
                    return NotFound();
                }
                return Ok(lottery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get lottery by gift id {GiftId}.", giftId);
                return StatusCode(500, "An unexpected error occurred while retrieving the lottery.");
            }
        }
        //create
        [Authorize]
        [Admin]
        [HttpPost]
        public async Task<IActionResult> AddLottery([FromBody] CreateLotteryDto lottery)
        {
            try
            {
                await _service.AddLottery(lottery);
                return Created(nameof(GetLotteryById), lottery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add lottery.");
                return StatusCode(500, "An unexpected error occurred while adding the lottery.");
            }
        }
        [Authorize]
        [Admin]
        [HttpPut]
        public async Task<IActionResult> UpdateLottery([FromBody] UpdateLotteryDto lottery)
        {
            try
            {
                var success = await _service.UpdateLottery(lottery);
                if (success == null)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update lottery {LotteryId}.", lottery?.Id);
                return StatusCode(500, "An unexpected error occurred while updating the lottery.");
            }
        }

        //delete
        [Authorize]
        [Admin]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLottery(int id)
        {
            try
            {
                await _service.DeleteLottery(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete lottery {LotteryId}.", id);
                return StatusCode(500, "An unexpected error occurred while deleting the lottery.");
            }
        }
        [Authorize]
        [Admin]
        [HttpPut]
        [Route("DrawWinners/{giftId}")]
        public async Task<IActionResult> DrawWinners(int giftId)
        {
            try
            {
                var winner = await _service.Lottery(giftId);
                return Ok("The winner is " + winner.FirstName + " " + winner.LastName + "!!! ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to draw winner for gift {GiftId}.", giftId);
                return StatusCode(500, "An unexpected error occurred while drawing a winner.");
            }
        }
    }
}