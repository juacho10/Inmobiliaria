using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authorization;

namespace Inmobiliaria.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [AllowAnonymous] // ✅ PERMITIR ACCESO SIN AUTENTICACIÓN
    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous] // ✅ PERMITIR ACCESO SIN AUTENTICACIÓN
    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize] // ✅ REQUIERE AUTENTICACIÓN
    public IActionResult Autenticado()
    {
        return View();
    }

    [Authorize(Policy = "Administrador")] // ✅ POLÍTICA ESPECÍFICA (Page 8 del PDF)
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