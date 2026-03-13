using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;
using GestaoRH.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetorController : ControllerBase
{
    private readonly SetorService _setorService;
    private readonly IUnitOfWork  _uof;

    public SetorController(SetorService setorService, IUnitOfWork uof)
    {
        _setorService = setorService;
        _uof          = uof;
    }

    /// <summary>Lista todos os setores ativos (usado no async select do front)</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var setores = await _setorService.Listar();
        return Ok(setores.Select(SetorService.ToResponse));
    }

    /// <summary>Busca setor por ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var setor = await _setorService.ObterPorId(id);
            return Ok(SetorService.ToResponse(setor));
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    /// <summary>Cadastra novo setor (RH)</summary>
    [HttpPost]
    public async Task<IActionResult> Cadastrar([FromBody] SetorCadastroDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var setor = await _setorService.Cadastrar(dto);
            await _uof.CommitAsync();
            return CreatedAtAction(nameof(ObterPorId), new { id = setor.Id }, SetorService.ToResponse(setor));
        }
        catch (ArgumentException ex)          { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex)  { return Conflict(ex.Message); }
        catch (Exception ex)                  { return StatusCode(500, ex.Message); }
    }

    /// <summary>Atualiza setor (RH)</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] SetorAtualizarDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var setor = await _setorService.Atualizar(id, dto);
            await _uof.CommitAsync();
            return Ok(SetorService.ToResponse(setor));
        }
        catch (KeyNotFoundException ex)       { return NotFound(ex.Message); }
        catch (InvalidOperationException ex)  { return Conflict(ex.Message); }
        catch (Exception ex)                  { return StatusCode(500, ex.Message); }
    }

    /// <summary>Desativa setor (RH)</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        try
        {
            await _setorService.Desativar(id);
            await _uof.CommitAsync();
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex)            { return StatusCode(500, ex.Message); }
    }
}
