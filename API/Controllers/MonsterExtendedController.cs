using System.Globalization;
using API.Models;
using CsvHelper;
using Lib.Repository.Entities;
using Lib.Repository.Repository;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MonsterExtendedController : BaseApiController
{
    private readonly IBattleOfMonstersRepository _repository;

    public MonsterExtendedController(IBattleOfMonstersRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll()
    {
        var listMonsters = await _repository.Monsters.GetAllAsync();

        return Ok(listMonsters);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Update(int id, [FromBody] Monster monster)
    {
        if(monster == null)
        {
            return BadRequest("Monster object is null");
        }
        var existingMonster = await _repository.Monsters.FindAsync(id);

        if (existingMonster == null)
        {
            return NotFound($"The monster with ID = {id} not found.");
        }

        existingMonster.Name = monster.Name;
        existingMonster.Attack = monster.Attack;
        existingMonster.Defense = monster.Defense;
        existingMonster.Hp = monster.Hp;
        existingMonster.ImageUrl = monster.ImageUrl;
        existingMonster.Speed = monster.Speed;

        _repository.Monsters.Update(id, existingMonster);
        await _repository.Save();
        return Ok();
    }
}
