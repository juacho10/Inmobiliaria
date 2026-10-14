using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Inmobiliaria.Repository;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<Propietario> _propietarioRepo;
    private readonly IRepository<Inquilino> _inquilinoRepo;
    private readonly IRepository<Contrato> _contratoRepo;
    private readonly IRepository<Pago> _pagoRepo;

    public HomeController(
        ILogger<HomeController> logger,
        IRepository<Propietario> propietarioRepo,
        IRepository<Inquilino> inquilinoRepo,
        IRepository<Contrato> contratoRepo,
        IRepository<Pago> pagoRepo)
    {
        _logger = logger;
        _propietarioRepo = propietarioRepo;
        _inquilinoRepo = inquilinoRepo;
        _contratoRepo = contratoRepo;
        _pagoRepo = pagoRepo;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalPropietarios = (await _propietarioRepo.GetAllAsync()).Count();
        ViewBag.TotalInquilinos = (await _inquilinoRepo.GetAllAsync()).Count();
        ViewBag.TotalContratosVigentes = (await _contratoRepo.FindAsync(c => c.Vigente)).Count();
        ViewBag.TotalPagos = (await _pagoRepo.GetAllAsync()).Count();
        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    public IActionResult Autenticado()
    {
        return View();
    }

    [Authorize(Policy = "Administrador")]
    public IActionResult SuperPrivado()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}