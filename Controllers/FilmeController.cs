using AutoMapper;
using FilmesApi.Data;
using FilmesApi.Data.DTO;
using FilmesApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmesApi.Controllers;

[ApiController]
[Route("filmes")]
public class FilmeController : ControllerBase
{
    private readonly AppDbContext _dbcontext;
    private readonly IMapper _mapper;
    public FilmeController(AppDbContext dbContext, IMapper mapper)
    {
        _dbcontext = dbContext;
        _mapper = mapper;

    }

/// <summary>
/// Adiciona um filme ao Banco de Dados
/// </summary>
/// <param name="filmeDTO">Objeto com os campos necessários para criação de filme</param>
/// <returns>IActionResult</returns>
/// <response code="201"> Caso a inserção seja feita com sucesso</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created )]
    public async Task<IActionResult> AdcionaFilme([FromBody] CreateFilmeDTO filmeDTO)
    {
        Filme filme = _mapper.Map<Filme>(filmeDTO);
        await _dbcontext.AddAsync(filme);
        _dbcontext.SaveChanges();

        return Created("", filme);

    }

    [HttpGet]
    public IEnumerable<ReadFilmeDTO> RecuperaFilmes([FromQuery] int skip = 0, [FromQuery] int take = 50 )
    {
        

        return  _mapper.Map<List<ReadFilmeDTO>>(_dbcontext.Filmes.Skip(skip).Take(take));

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> RecuperaFilmePorId([FromRoute] int id)
    {
        var filme = await _dbcontext.Filmes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (filme == null)
            return NotFound();
        var filmeDTO = _mapper.Map<ReadFilmeDTO>(filme);
        return Ok(filmeDTO);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> AtualizaFilme(int id, [FromBody] UpdateFilmeDTO filmeDTO)
    {
        var filme = await _dbcontext.Filmes.FirstOrDefaultAsync(filme => filme.Id == id);

        if (filme is null) return NotFound();
        _mapper.Map(filmeDTO, filme);
        await _dbcontext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> AtualizaFilmeParcial(int id, [FromBody] JsonPatchDocument<UpdateFilmeDTO> patch)
    {
        var filme = await _dbcontext.Filmes.FirstOrDefaultAsync(filme => filme.Id == id);

        if (filme is null) return NotFound();

        var filmeParaAtualizar = _mapper.Map<UpdateFilmeDTO>(filme);

        patch.ApplyTo(filmeParaAtualizar, ModelState);
        if (!TryValidateModel(filmeParaAtualizar))
        {
            return ValidationProblem(ModelState);
        }
        _mapper.Map(filmeParaAtualizar, filme);
        await _dbcontext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletaFilme(int id)
    {
        var filme = await _dbcontext.Filmes.FirstOrDefaultAsync(filme => filme.Id == id);

        if (filme is null) return NotFound();

        _dbcontext.Remove(filme);
        _dbcontext.SaveChangesAsync();

        return NoContent();
    }
}
