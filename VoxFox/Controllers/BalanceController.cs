using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces;
using VoxFox.Interfaces.Balance;
using VoxFox.Models.DTOs;
using VoxFox.Models.DTOs.Money;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BalanceController : ControllerBase
    {
        private readonly IBalanceService _balanceService;

        public BalanceController(IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        /// <summary>Получить свой баланс</summary>
        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();
            var result = await _balanceService.GetBalanceAsync(userId.Value);
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            return Ok(result.Data);
        }

        /// <summary>Пополнить баланс</summary>
        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();
            if (request.Amount <= 0) return BadRequest(new { error = "Сумма должна быть больше нуля" });
            var result = await _balanceService.TopUpAsync(userId.Value, request.Amount);
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            return Ok(result.Data);
        }

        /// <summary>Купить курс</summary>
        [HttpPost("purchase/{courseId}")]
        public async Task<IActionResult> Purchase([FromRoute] Guid courseId)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();
            var result = await _balanceService.PurchaseCourseAsync(userId.Value, courseId);
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            return Ok(result.Data);
        }

        /// <summary>История транзакций текущего пользователя</summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetMyTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = User.GetUserId();
            if (userId == null) return Unauthorized();
            var result = await _balanceService.GetUserTransactionsAsync(userId.Value, page, pageSize);
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            return Ok(result.Data);
        }

        /// <summary>Все транзакции платформы — только для админа</summary>
        [HttpGet("admin/transactions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _balanceService.GetAllTransactionsAsync(page, pageSize);
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            return Ok(result.Data);
        }

        /// <summary>Статистика доходов платформы — только для админа</summary>
        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPlatformStats()
        {
            var result = await _balanceService.GetPlatformStatsAsync();
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            return Ok(result.Data);
        }

        /// <summary>Отменить покупку (возврат) — только для админа</summary>
        [HttpPost("admin/refund/{transactionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Refund([FromRoute] Guid transactionId)
        {
            var result = await _balanceService.RefundPurchaseAsync(transactionId);
            if (!result.Success) return StatusCode(result.StatusCode ?? 400, new { error = result.Message });
            return Ok(result.Data);
        }
    }
}
