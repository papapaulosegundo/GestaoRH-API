using GestaoRH.Models.DTOs;
using GestaoRH.Repositories;
using GestaoRH.Services;
using GestaoRH.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GestaoRH.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FuncionarioController : ControllerBase
{
    private readonly FuncionarioService _funcionarioService;
    private readonly IUnitOfWork        _uof;
    private readonly IConfiguration     _config;

    public FuncionarioController(FuncionarioService funcionarioService, IUnitOfWork uof, IConfiguration config)
    {
        _funcionarioService = funcionarioService;
        _uof                = uof;
        _config             = config;
    }

    /// <summary>
    /// Cadastra um novo funcionário — apenas RH (requer token).
    /// Retorna dados incluindo a senha temporária gerada automaticamente.
    /// </summary>
    [HttpPost("cadastrar")]
    public async Task<IActionResult> Cadastrar([FromBody] FuncionarioCadastroDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var funcionario = await _funcionarioService.Cadastrar(dto);
            await _uof.CommitAsync();

            // Retorna RhResponse para que o RH veja a senha temporária
            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = funcionario.Id },
                FuncionarioService.ToRhResponse(funcionario));
        }
        catch (ArgumentException ex)          { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex)  { return Conflict(ex.Message); }
        catch (KeyNotFoundException ex)       { return NotFound(ex.Message); }
        catch (Exception ex)                  { return StatusCode(500, ex.Message); }
    }

    /// <summary>
    /// Login do funcionário pelo CPF — retorna JWT.
    /// Rota pública (liberada no middleware).
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] FuncionarioLoginDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var funcionario = await _funcionarioService.Login(dto.Cpf, dto.Senha);

            return Ok(new
            {
                Funcionario  = FuncionarioService.ToResponse(funcionario),
                SenhaTrocada = funcionario.SenhaTrocada,
                Jwt          = Jwt.GenerateFuncionarioToken(funcionario, _config)
            });
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        catch (Exception ex)                   { return StatusCode(500, ex.Message); }
    }

    /// <summary>Troca senha do funcionário (requer token)</summary>
    [HttpPatch("{id:int}/trocar-senha")]
    public async Task<IActionResult> TrocarSenha(int id, [FromBody] FuncionarioTrocarSenhaDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            await _funcionarioService.TrocarSenha(id, dto);
            await _uof.CommitAsync();
            return Ok(new { mensagem = "Senha alterada com sucesso." });
        }
        catch (KeyNotFoundException ex)        { return NotFound(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(ex.Message); }
        catch (ArgumentException ex)           { return BadRequest(ex.Message); }
        catch (Exception ex)                   { return StatusCode(500, ex.Message); }
    }

    /// <summary>Lista todos os funcionários ativos — apenas RH</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _funcionarioService.Listar();
        // RH vê a senha temporária na listagem
        return Ok(lista.Select(FuncionarioService.ToRhResponse));
    }

    /// <summary>Lista funcionários de um setor — apenas RH</summary>
    [HttpGet("setor/{setorId:int}")]
    public async Task<IActionResult> ListarPorSetor(int setorId)
    {
        var lista = await _funcionarioService.ListarPorSetor(setorId);
        return Ok(lista.Select(FuncionarioService.ToRhResponse));
    }

    /// <summary>Busca funcionário por ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var funcionario = await _funcionarioService.ObterPorId(id);
            return Ok(FuncionarioService.ToRhResponse(funcionario));
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
    }

    /// <summary>Atualiza dados do funcionário — RH</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] FuncionarioAtualizarDto dto)
    {
        try
        {
            if (dto is null) return BadRequest("Body vazio.");
            var funcionario = await _funcionarioService.Atualizar(id, dto);
            await _uof.CommitAsync();
            return Ok(FuncionarioService.ToResponse(funcionario));
        }
        catch (KeyNotFoundException ex)       { return NotFound(ex.Message); }
        catch (InvalidOperationException ex)  { return Conflict(ex.Message); }
        catch (ArgumentException ex)          { return BadRequest(ex.Message); }
        catch (Exception ex)                  { return StatusCode(500, ex.Message); }
    }

    /// <summary>Desativa funcionário — RH</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        try
        {
            await _funcionarioService.Desativar(id);
            await _uof.CommitAsync();
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        catch (Exception ex)            { return StatusCode(500, ex.Message); }
    }
}
