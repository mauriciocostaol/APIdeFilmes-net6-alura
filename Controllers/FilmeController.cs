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

    [HttpPost]
    public async Task<IActionResult> AdcionaFilme([FromBody] CreateFilmeDTO filmeDTO)
    {
        Filme filme = _mapper.Map<Filme>(filmeDTO);
        await _dbcontext.AddAsync(filme);
        _dbcontext.SaveChanges();

        return Created("", filme);

    }

    [HttpGet]
    public async Task<IActionResult> RecuperaFilmes()
    {
        var filmes = await _dbcontext.Filmes.AsNoTracking().ToListAsync();

        if (filmes == null)
            return NotFound("Nenhum filme encontrado");

        return Ok(filmes);

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> RecuperaFilmePorId([FromRoute] int id)
    {
        var filme = await _dbcontext.Filmes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (filme == null)
            return NotFound();
        return Ok(filme);
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
}
