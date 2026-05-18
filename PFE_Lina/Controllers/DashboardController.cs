using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartDelivery.Domaine.Models;
using SmartDelivery.Domaine.Models.DTOs;
using SmartDelivery.Domaine.Queries;

namespace SmartDelivery.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediateur;
        public DashboardController(IMediator mediateur) => _mediateur = mediateur;

        // GET api/dashboard/admin — Admin uniquement
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> KpiAdmin()
        {
            var kpi = await _mediateur.Send(new GetKpiAdminQuery());
            return Ok(ResultatOperation<KpiAdminDto>.Ok(kpi));
        }

        // GET api/dashboard/dispatcher — Dispatcher et Admin
        [HttpGet("dispatcher")]
        [Authorize(Roles = "Admin,Dispatcher")]
        public async Task<IActionResult> KpiDispatcher()
        {
            var kpi = await _mediateur.Send(new GetKpiDispatcherQuery());
            return Ok(ResultatOperation<KpiDispatcherDto>.Ok(kpi));
        }
    }
}
